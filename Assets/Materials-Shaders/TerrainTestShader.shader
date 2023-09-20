Shader "RGB20/TerrainTestShader"
{
    Properties{
        [NoScaleOffset] _HeightMap("Height map", 2D) = "white" {}
        // This keyword enum allows us to choose between partitioning modes. It's best to try them out for yourself
        [KeywordEnum(INTEGER, FRAC_EVEN, FRAC_ODD, POW2)] _PARTITIONING("Partition algoritm", Float) = 3
        _FactorInside("Inside factor", float) = 1
        // This factor is applied differently per factor mode
        //  Constant: not used
        //  World: this is the ideal edge length in world units. The algorithm will try to keep all edges at this value
        //  Screen: this is the ideal edge length in screen pixels. The algorithm will try to keep all edges at this value
        //  World with depth: similar to world, except the edge length is decreased quadratically as the camera gets closer 
        _TessellationFactor("Tessellation factor", Float) = 0.03
        // This value is added to the tessellation factor. Use if your model should be more or less tessellated by default
        _TessellationBias("Tessellation bias", Float) = 0
        _TessellationSmoothing("Tessellation smoothing", Float) = 0.3
        _HeightMapAltitude("HeightMap altitude", Float) = 1.0
        _FrustrumCullBias("Frustrum cull bias", Float) = 10.0
        _BackfacCullBias("Backface cull bias", Float) = 0.3
        _NormalStrength("Normal strength", Float) = 1
    }
    SubShader{
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        Pass {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma target 5.0 // 5.0 required for tessellation

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

        // Material keywords
        #pragma shader_feature_local _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2

        #pragma vertex Vertex
        #pragma hull Hull
        #pragma domain Domain
        #pragma fragment Fragment

        #include "TessTestShader.hlsl"
        ENDHLSL
        }
    }
}