using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

[RequireComponent(typeof(MeshFilter))]
public class Terrain : MonoBehaviour
{
    struct BoundingBox
    {
        Vector3 minBounds;
        Vector3 maxBounds;
    }
    struct Chunks
    {
        List<Vector3> terrainVertPos;
        List<int> vertexIndex;

        BoundingBox BB;
    };

    // Divide the terrain into chunks of 10x10 with the visible terrain of about 20x20 chunks
    private List<Chunks> chunks;


    private List<Vector3> terrainVertPos;
    private List<int> vertexIndex;

    private int terrainDimX;
    private int terrainDimZ;

    private int chunksWidth;
    private int chunksHeight;

    // Debugging Elements
    private GameObject sphereObject;
    public bool debugging;


    void Start()
    {
        debugging = false;
        
        if (debugging == true) {
            // Debugging Terrain Points with Spheres
            sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        chunksWidth = 20;
        chunksHeight = 20;

        terrainDimX = 1000;
        terrainDimZ = 1000;

        // Create a Tringulated Mesh from the sequence of points
        //for (int X = 0; )
        terrainVertPos = new List<Vector3>();

        float waveFrequency = 10;
        float XOffset = 0;
        float XSpacing = 0.5f;

        for (int z = 0; z < terrainDimZ; ++z)
        {
            XOffset = 0;
            for (int x = 0; x < terrainDimX; ++x)
            {
                terrainVertPos.Add(new Vector3(XOffset, Mathf.Sin(Mathf.Deg2Rad * ((x + z) * waveFrequency + Time.fixedTime * 100)), z));
                //terrainVertPos.Add(new Vector3(XOffset, 0, z));

                if (debugging == true)
                {
                    // Debugging
                    GameObject newGO = Instantiate(sphereObject, new Vector3(XOffset, 0, z), Quaternion.identity);
                    newGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    newGO.name = x.ToString() + z.ToString();
                }

                XOffset += XSpacing;
            }
        }

        vertexIndex = new List<int>();

        // Generate the indixes for the triangulated points
        for(int z = 0; z < (terrainDimZ- 1); z++) 
        {
            for (int x = 0; x < (terrainDimX - 1); x++)
            {
                int indexPosRef = x + z * terrainDimX;
                // Add the 6 indexes that make up the trinagles
                vertexIndex.Add(indexPosRef);
                vertexIndex.Add(indexPosRef + terrainDimX);
                vertexIndex.Add(indexPosRef + 1);
                vertexIndex.Add(indexPosRef + 1);
                vertexIndex.Add(indexPosRef + terrainDimX);
                vertexIndex.Add(indexPosRef + 1 + terrainDimX);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        mesh.vertices = terrainVertPos.ToArray();
        mesh.triangles = vertexIndex.ToArray();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
