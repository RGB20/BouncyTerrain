#ifndef TESSELLATION_FACTORS_INCLUDED
#define TESSELLATION_FACTORS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#define NUM_BEZIER_CONTROL_POINTS 10

struct TessellationFactors {
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
    float3 bezierPoints[NUM_BEZIER_CONTROL_POINTS] : BEZIERPOS;
};

struct TessellationControlPoint {
    float3 positionWS : INTERNALTESSPOS;
    float4 positionCS : SV_POSITION;
    float3 normalWS : NORMAL;
    float4 tangentWS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Interpolators {
    float2 uv                       : TEXCOORD0;
    float3 normalWS                 : TEXCOORD1;
    float3 positionWS               : TEXCOORD2;
    float4 positionCS               : SV_POSITION;
    float4 tangentWS                : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityPerMaterial)
float _FactorInside;
float _TessellationFactor;
float _TessellationBias;
float _TessellationSmoothing;
float _HeightMapAltitude;
float _FrustrumCullBias;
float _BackfacCullBias;
float4 _MainTexture_TexelSize;
float _NormalStrength;
CBUFFER_END

float3 GetViewDirectionFromPosition(float3 positionWS) {
    return normalize(GetCameraPositionWS() - positionWS);
}

float4 GetShadowCoord(float3 positionWS, float4 positionCS) {
    // Calculate the shadow coordinate depending on the type of shadows currently in use
#if SHADOWS_SCREEN
    return ComputeScreenPos(positionCS);
#else
    return TransformWorldToShadowCoord(positionWS);
#endif
}


// Returns true if the point is outside the bounds set by lower and higher
bool IsOutOfBounds(float3 p, float3 lower, float3 higher) {
    return p.x < lower.x || p.x > higher.x || p.y < lower.y || p.y > higher.y || p.z < lower.z || p.z > higher.z;
}


// Returns true if the given vertex is outside the camera fustum and should be culled
bool IsPointOutOfFrustum(float4 positionCS, float tolerance) {
    float3 culling = positionCS.xyz;
    float w = positionCS.w;
    // UNITY_RAW_FAR_CLIP_VALUE is either 0 or 1, depending on graphics API
    // Most use 0, however OpenGL uses 1
    float3 lowerBounds = float3(-w - tolerance, -w - tolerance, -w * UNITY_RAW_FAR_CLIP_VALUE - tolerance);
    float3 higherBounds = float3(w + tolerance, w + tolerance, w + tolerance);
    return IsOutOfBounds(culling, lowerBounds, higherBounds);
}

// Returns true if the points in this triangle are wound counter-clockwise
bool ShouldBackFaceCull(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS, float tolerance) {
    float3 point0 = p0PositionCS.xyz / p0PositionCS.w;
    float3 point1 = p1PositionCS.xyz / p1PositionCS.w;
    float3 point2 = p2PositionCS.xyz / p2PositionCS.w;
    // In clip space, the view direction is float3(0, 0, 1), so we can just test the z coord
#if UNITY_REVERSED_Z
    return cross(point1 - point0, point2 - point0).z < -tolerance;
#else // In OpenGL, the test is reversed
    return cross(point1 - point0, point2 - point0).z > tolerance;
#endif
}

// Returns true if it should be clipped due to frustum or winding culling
bool ShouldClipPatch(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS) {
    bool allOutside = IsPointOutOfFrustum(p0PositionCS, _FrustrumCullBias) && IsPointOutOfFrustum(p1PositionCS, _FrustrumCullBias) && IsPointOutOfFrustum(p2PositionCS, _FrustrumCullBias);
    return false;// allOutside || ShouldBackFaceCull(p0PositionCS, p1PositionCS, p2PositionCS, _BackfacCullBias);
}

TessellationControlPoint Vertex(Attributes input) {
    TessellationControlPoint output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionWS = posnInputs.positionWS;
    output.positionCS = posnInputs.positionCS;
    output.normalWS = normalInputs.normalWS;
    output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w); // tangent.w containts bitangent multiplier
    output.uv = input.uv;
    return output;
}

// Calculate the tessellation factor for an edge
// This function needs the world and clip space positions of the connected vertices
float EdgeTessellationFactor(float scale, float bias, float3 p0PositionWS, float4 p0PositionCS, float3 p1PositionWS, float4 p1PositionCS) {
    float length = distance(p0PositionWS, p1PositionWS);
    float distanceToCamera = distance(GetCameraPositionWS(), (p0PositionWS + p1PositionWS) * 0.5);
    float factor = length / (scale * distanceToCamera * distanceToCamera);

    return max(1, factor + bias);
}

// The patch constant function runs once per triangle, or "patch"
// It runs in parallel to the hull function
TessellationFactors PatchConstantFunction(
    InputPatch<TessellationControlPoint, 3> patch) {
    UNITY_SETUP_INSTANCE_ID(patch[0]); // Set up instancing
    // Calculate tessellation factors
    TessellationFactors f = (TessellationFactors)0;
    // Check if this patch should be culled (it is out of view)
    if (ShouldClipPatch(patch[0].positionCS, patch[1].positionCS, patch[2].positionCS)) {
        f.edge[0] = f.edge[1] = f.edge[2] = f.inside = 0; // Cull the patch
    }
    else {
        // Calculate tessellation factors
        f.edge[0] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias,
            patch[1].positionWS, patch[1].positionCS, patch[2].positionWS, patch[2].positionCS);
        f.edge[1] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias,
            patch[2].positionWS, patch[2].positionCS, patch[0].positionWS, patch[0].positionCS);
        f.edge[2] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias,
            patch[0].positionWS, patch[0].positionCS, patch[1].positionWS, patch[1].positionCS);
        f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0;
    }
    return f;
}

// The hull function runs once per vertex. You can use it to modify vertex
// data based on values in the entire triangle
[domain("tri")] // Signal we're inputting triangles
[outputcontrolpoints(3)] // Triangles have three points
[outputtopology("triangle_cw")] // Signal we're outputting triangles
[patchconstantfunc("PatchConstantFunction")] // Register the patch constant function
// Select a partitioning mode based on keywords
#if defined(_PARTITIONING_INTEGER)
[partitioning("integer")]
#elif defined(_PARTITIONING_FRAC_EVEN)
[partitioning("fractional_even")]
#elif defined(_PARTITIONING_FRAC_ODD)
[partitioning("fractional_odd")]
#elif defined(_PARTITIONING_POW2)
[partitioning("pow2")]
#else 
[partitioning("fractional_odd")]
#endif
TessellationControlPoint Hull(
    InputPatch<TessellationControlPoint, 3> patch, // Input triangle
    uint id : SV_OutputControlPointID) { // Vertex index on the triangle

    return patch[id];
}

// Call this macro to interpolate between a triangle patch, passing the field name
#define BARYCENTRIC_INTERPOLATE(fieldName) \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z

// Barycentric interpolation as a function
float3 BarycentricInterpolate(float3 bary, float3 a, float3 b, float3 c) {
    return bary.x * a + bary.y * b + bary.z * c;
}


// Calculate Phong projection offset
float3 PhongProjectedPosition(float3 flatPositionWS, float3 cornerPositionWS, float3 normalWS) {
    return flatPositionWS - dot(flatPositionWS - cornerPositionWS, normalWS) * normalWS;
}

// Apply Phong smoothing
float3 CalculatePhongPosition(float3 bary, float smoothing, float3 p0PositionWS, float3 p0NormalWS,
    float3 p1PositionWS, float3 p1NormalWS, float3 p2PositionWS, float3 p2NormalWS) {
    float3 flatPositionWS = BarycentricInterpolate(bary, p0PositionWS, p1PositionWS, p2PositionWS);
    float3 smoothedPositionWS =
        bary.x * PhongProjectedPosition(flatPositionWS, p0PositionWS, p0NormalWS) +
        bary.y * PhongProjectedPosition(flatPositionWS, p1PositionWS, p1NormalWS) +
        bary.z * PhongProjectedPosition(flatPositionWS, p2PositionWS, p2NormalWS);
    return lerp(flatPositionWS, smoothedPositionWS, smoothing);
}

float3 CalculateBezierNormal(float3 bary, float3 bezierPoints[NUM_BEZIER_CONTROL_POINTS],
    float3 p0NormalWS, float3 p1NormalWS, float3 p2NormalWS) {
    return p0NormalWS * (bary.x * bary.x) +
        p1NormalWS * (bary.y * bary.y) +
        p2NormalWS * (bary.z * bary.z) +
        bezierPoints[7] * (2 * bary.x * bary.y) +
        bezierPoints[8] * (2 * bary.y * bary.z) +
        bezierPoints[9] * (2 * bary.z * bary.x);
}

void CalculateBezierNormalAndTangent(float3 bary, float smoothing, float3 bezierPoints[NUM_BEZIER_CONTROL_POINTS],
    float3 p0NormalWS, float3 p0TangentWS, float3 p1NormalWS, float3 p1TangentWS, float3 p2NormalWS, float3 p2TangentWS,
    out float3 normalWS, out float3 tangentWS) {

    float3 flatNormalWS = BarycentricInterpolate(bary, p0NormalWS, p1NormalWS, p2NormalWS);
    float3 smoothedNormalWS = CalculateBezierNormal(bary, bezierPoints, p0NormalWS, p1NormalWS, p2NormalWS);
    normalWS = normalize(lerp(flatNormalWS, smoothedNormalWS, smoothing));

    float3 flatTangentWS = BarycentricInterpolate(bary, p0TangentWS, p1TangentWS, p2TangentWS);
    float3 flatBitangentWS = cross(flatNormalWS, flatTangentWS);
    tangentWS = normalize(cross(flatBitangentWS, normalWS));
}

TEXTURE2D(_HeightMap); SAMPLER(sampler_HeightMap);

// The domain function runs once per vertex in the final, tessellated mesh
// Use it to reposition vertices and prepare for the fragment stage
[domain("tri")] // Signal we're inputting triangles
Interpolators Domain(
    TessellationFactors factors, // The output of the patch constant function
    OutputPatch<TessellationControlPoint, 3> patch, // The Input triangle
    float3 barycentricCoordinates : SV_DomainLocation) { // The barycentric coordinates of the vertex on the triangle

    Interpolators output;

    // Setup instancing and stereo support (for VR)
    UNITY_SETUP_INSTANCE_ID(patch[0]);
    UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // Calculate positionWS, normalWS and tangentWS through whichever chosen algorithm
    float2 uv = BARYCENTRIC_INTERPOLATE(uv); // Interpolate UV

    float smoothing = _TessellationSmoothing;
    float3 positionWS = CalculatePhongPosition(barycentricCoordinates, smoothing,
        patch[0].positionWS, patch[0].normalWS,
        patch[1].positionWS, patch[1].normalWS,
        patch[2].positionWS, patch[2].normalWS);
    //float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);
    //float3 tangentWS = BARYCENTRIC_INTERPOLATE(tangentWS.xyz);
    float3 normalWS, tangentWS;
    CalculateBezierNormalAndTangent(barycentricCoordinates, smoothing, factors.bezierPoints,
        patch[0].normalWS, patch[0].tangentWS.xyz, patch[1].normalWS, patch[1].tangentWS.xyz, patch[2].normalWS, patch[2].tangentWS.xyz,
        normalWS, tangentWS);

    // Offset the position along the normal by the height of the terrain
    float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).r * _HeightMapAltitude;
    positionWS += normalWS * height;

    output.uv = uv;
    output.positionCS = TransformWorldToHClip(positionWS);
    output.normalWS = normalWS;
    output.positionWS = positionWS;
    output.tangentWS = float4(tangentWS, patch[0].tangentWS.w);

    return output;
}

// Sample the height map, using mipmaps
float SampleHeight(float2 uv) {
    return SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r;
}

// Calculate a normal vector by sampling the height map
float3 GenerateNormalFromHeightMap(float2 uv) {
    // Sample the height from adjacent pixels
    float texelSize = 1.0/1024.0;
    float left = SampleHeight(uv - float2(texelSize, 0));
    float right = SampleHeight(uv + float2(texelSize, 0));
    float down = SampleHeight(uv - float2(0, texelSize));
    float up = SampleHeight(uv + float2(0, texelSize));

    // Generate a tangent space normal using the slope along the U and V axes
    float3 normalTS = float3((left - right) / (texelSize * 2), (down - up) / (texelSize * 2), 1);

    normalTS.xy *= _NormalStrength; // Adjust the XY channels to create stronger or weaker normals
    return normalize(normalTS);
}

float4 Fragment(Interpolators input) : SV_Target{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 normalTS = GenerateNormalFromHeightMap(input.uv);

    float3x3 tangentToWorld = CreateTangentToWorld(input.normalWS, input.tangentWS.xyz, input.tangentWS.w);

    float3 normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld)); // Convert to world space

    // Fill the various lighting and surface data structures for the PBR algorithm
    InputData lightingInput = (InputData)0; // Found in URP/Input.hlsl
    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = normalWS;
    lightingInput.viewDirectionWS = GetViewDirectionFromPosition(lightingInput.positionWS);
    lightingInput.shadowCoord = GetShadowCoord(lightingInput.positionWS, input.positionCS);
    lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    SurfaceData surface = (SurfaceData)0; // Found in URP/SurfaceData.hlsl
    surface.albedo = 0.5;
    surface.alpha = 1;
    surface.metallic = 0.2;
    surface.smoothness = 0.5;
    surface.normalTS = normalTS;
    surface.occlusion = 1;

    return UniversalFragmentPBR(lightingInput, surface);
}

#endif