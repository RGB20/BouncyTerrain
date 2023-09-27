using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.VirtualTexturing;



public class TerrainPatchGenerator : MonoBehaviour
{
    // Debugging Elements
    private GameObject sphereObject;
    public bool debugging;

    public GameObject TerrainPatchGO;

    public float XSpacing;
    public float ZSpacing;

    public UInt32 patchWidth = 10000;
    public UInt32 patchHeight = 10000;

    public int terrainHeightMapWidth;
    public int terrainHeightMapHeight;

    void Start()
    {
        debugging = false;

        XSpacing = 1.0f;
        ZSpacing = 1.0f;

        float ZVertPos = 0;
        float XVertPos = 0;

        TerrainPatchGO.transform.localScale = new Vector3(1f, 1f, 1f);
        Vector3 patchCenterPos = new Vector3(0,0,0);
        TerrainPatchGO.transform.position = patchCenterPos;

        if (debugging == true)
        {
            // Debugging Terrain Points with Spheres
            sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereObject.transform.localScale = new Vector3(1f, 1f, 1f);
            sphereObject.name = "PositionTest";
            sphereObject.transform.position = patchCenterPos;
        }

        // Create a Tringulated Mesh from the sequence of points
        List<Vector3> terrainPatchVertPos = new List<Vector3>();
        List<Color> terrainPatchVertColors = new List<Color>();
        List<Vector3> terrainPatchVertNormals = new List<Vector3>();
        List<Vector2> terrainPatchUV = new List<Vector2>();

        ZVertPos = 0;// (chunksHeight * i) / (chunksHeight * terrainDimZ); // (-(chunksHeight * terrainDimZ) / 2.0f) + ((float)i * terrainDimZ - i) * ZSpacing;

        for (uint z = 0; z < patchHeight; z++)
        {
            XVertPos = 0;// (chunksWidth * j) / (chunksWidth * terrainDimX); // (-(chunksWidth * terrainDimX) / 2.0f) + ((float)j * terrainDimX - j) * XSpacing;
            for (int x = 0; x < patchWidth; x++)
            {
                //float perlinHeight = (float)perlinNoise.OctavePerlin((double)XVertPos/, (double)0.0f, (double)ZVertPos, 4, 8.0f);
                float YOffset = 0;// perlinHeight * waveFrequency;//// Mathf.Sin(Mathf.Deg2Rad * ((x + z) * waveFrequency + Time.fixedTime * 100));

                terrainPatchVertPos.Add(new Vector3(XVertPos, YOffset, ZVertPos));

                terrainPatchVertColors.Add(new Color(0.1f, 1.0f, 0.2f));

                terrainPatchUV.Add(new Vector2(x/(float)patchWidth, z/(float)patchHeight));

                XVertPos += XSpacing;
            }
            ZVertPos += ZSpacing;
        }

        if (debugging)
            Debug.Log("vertPos : " + terrainPatchVertPos.Count);
        
        List<int> patchVertexIndex = new List<int>();

        // Generate the indixes for the triangulated points
        for (int z = 0; z < (patchHeight - 1); z++)
        {
            for (int x = 0; x < (patchWidth - 1); x++)
            {
                int indexPosRef = x + z * (int)patchWidth;
                // Add the 6 indexes that make up the trinagles
                patchVertexIndex.Add(indexPosRef);
                patchVertexIndex.Add(indexPosRef + (int)patchWidth);
                patchVertexIndex.Add(indexPosRef + 1);
                patchVertexIndex.Add(indexPosRef + 1);
                patchVertexIndex.Add(indexPosRef + (int)patchWidth);
                patchVertexIndex.Add(indexPosRef + 1 + (int)patchWidth);
            }
        }

        if (debugging == true)
        {
            Debug.Log("Vertex Index Count : " + patchVertexIndex.Count);
            Debug.Log("Vertex Colors Count : " + terrainPatchVertColors.Count);
            Debug.Log("Vertex Normals Count : " + terrainPatchVertNormals.Count);
        }

        Mesh mesh = new Mesh();
        TerrainPatchGO.GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        mesh.vertices = terrainPatchVertPos.ToArray();
        mesh.triangles = patchVertexIndex.ToArray();
        mesh.colors = terrainPatchVertColors.ToArray();
        mesh.uv = terrainPatchUV.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        // Generate and Apply the perlin noise height map to the terrain mesh
        terrainHeightMapWidth = 1024;// 3053;
        terrainHeightMapHeight = 1024;// 3054;
        TerrainHeightMapGenerator heightMapGenerator = new();
        Texture2D heightMapTexture = new(terrainHeightMapWidth, terrainHeightMapHeight, TextureFormat.RGBAFloat, -1, false);

        byte[] bytes = File.ReadAllBytes("C:\\Users\\rudra\\Downloads\\" + "Image" + ".png");


        heightMapTexture.LoadImage(bytes);
        //heightMapTexture.SetPixels(heightMap.ToArray(), 0);
        heightMapTexture.Apply(true);


        TerrainPatchGO.GetComponent<Renderer>().material.SetTexture("_HeightMap", heightMapTexture);
        TerrainPatchGO.GetComponent<Renderer>().material.SetTexture("_NormalMap", heightMapTexture);
        TerrainPatchGO.GetComponent<Renderer>().material.SetTexture("_MainTexture", heightMapTexture);


        // Add the mesh to the mesh collider for it to generate the collider
        // Do this only once per mesh and not every update as it is expensive to calculate
        TerrainPatchGO.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
