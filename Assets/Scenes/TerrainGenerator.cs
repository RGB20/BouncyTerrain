using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class TerrainGenerator : MonoBehaviour
{
    struct BoundingBox
    {
        Vector3 minBounds;
        Vector3 maxBounds;
    }

    // Chunks are centered around world (0,0,0)
    public struct Chunk
    {
        public List<Vector3> terrainVertPos;
        public List<Color> terrainVertColors;
        public List<Vector3> terrainVertNormals;
        public List<int> vertexIndex;

        public GameObject chunkGO;
        //public BoundingBox BB;
    };

    // Divide the terrain into chunks of 10x10 with the visible terrain of about 20x20 chunks
    private List<Chunk> chunks;

    private float terrainDimX;
    private float terrainDimZ;

    private float chunksWidth;
    private float chunksHeight;

    // Debugging Elements
    private GameObject sphereObject;
    public bool debugging;

    public GameObject TerrainChunkGO;

    void Start()
    {
        debugging = false;

        chunksWidth = 5;
        chunksHeight = 5;

        terrainDimX = 100;
        terrainDimZ = 100;

        float XSpacing = 1f;
        float ZSpacing = 1f;

        chunks = new List<Chunk>();

        for (int i = 0; i < chunksHeight; i++)
        {
            for (int j = 0; j < chunksWidth; j++)
            {
                Chunk chunk = new Chunk();
                chunk.chunkGO = GameObject.Instantiate(TerrainChunkGO);
            
                chunk.chunkGO.transform.localScale = new Vector3(1f, 1f, 1f);
                Vector3 chunkBottomLeftPos = new Vector3((-(chunksWidth * terrainDimX) / 2.0f) + ((float)j * terrainDimX - j) * XSpacing, 0.0f, (-(chunksHeight * terrainDimZ) / 2.0f) + ((float)i * terrainDimZ - i) * ZSpacing);
                chunk.chunkGO.transform.position = chunkBottomLeftPos;

                if (debugging == true)
                {
                    // Debugging Terrain Points with Spheres
                    sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphereObject.transform.localScale = new Vector3(1f, 1f, 1f);
                    sphereObject.name = "PositionTest";
                    sphereObject.transform.position = chunkBottomLeftPos;
                }

                // Create a Tringulated Mesh from the sequence of points
                chunk.terrainVertPos = new List<Vector3>();
                chunk.terrainVertColors = new List<Color>();
                chunk.terrainVertNormals = new List<Vector3>();

                float waveFrequency = 50;
                float ZOffset = 0;//


                for (uint z = 0; z < terrainDimZ; z++)
                {
                    float XOffset = 0;// 
                    for (int x = 0; x < terrainDimX; x++)
                    {
                        float YOffset = Mathf.PerlinNoise((float)XOffset/terrainDimX, (float)ZOffset/terrainDimZ) * waveFrequency; // Mathf.Sin(Mathf.Deg2Rad * ((x + z) * waveFrequency + Time.fixedTime * 100));

                        chunk.terrainVertPos.Add(new Vector3(XOffset, YOffset, ZOffset));

                        chunk.terrainVertColors.Add(new Color(0.0f, 1.0f, 0.0f));

                        XOffset += XSpacing;
                    }
                    ZOffset += ZSpacing;
                }

                if (debugging)
                    Debug.Log("vertPos : " + chunk.terrainVertPos.Count);

                chunk.vertexIndex = new List<int>();

                // Generate the indixes for the triangulated points
                for (int z = 0; z < (terrainDimZ - 1); z++)
                {
                    for (int x = 0; x < (terrainDimX - 1); x++)
                    {
                        int indexPosRef = x + z * (int)terrainDimX;
                        // Add the 6 indexes that make up the trinagles
                        chunk.vertexIndex.Add(indexPosRef);
                        chunk.vertexIndex.Add(indexPosRef + (int)terrainDimX);
                        chunk.vertexIndex.Add(indexPosRef + 1);
                        chunk.vertexIndex.Add(indexPosRef + 1);
                        chunk.vertexIndex.Add(indexPosRef + (int)terrainDimX);
                        chunk.vertexIndex.Add(indexPosRef + 1 + (int)terrainDimX);
                    }
                }

                if (debugging == true)
                {
                    Debug.Log("Vertex Index Count : " + chunk.vertexIndex.Count);
                    Debug.Log("Vertex Colors Count : " + chunk.terrainVertColors.Count);
                    Debug.Log("Vertex Normals Count : " + chunk.terrainVertNormals.Count);
                }

                Mesh mesh = new Mesh();
                chunk.chunkGO.GetComponent<MeshFilter>().mesh = mesh;
                mesh.Clear();
                mesh.vertices = chunk.terrainVertPos.ToArray();
                mesh.triangles = chunk.vertexIndex.ToArray();
                mesh.colors = chunk.terrainVertColors.ToArray();
                mesh.RecalculateNormals();

                // Add the mesh to the mesh collider for it to generate the collider
                // Do this only once per mesh and not every update as it is expensive to calculate
                chunk.chunkGO.GetComponent<MeshCollider>().sharedMesh = mesh;
                chunks.Add(chunk);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
