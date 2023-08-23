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
    private List<Color> terrainVertColors;
    private List<Vector3> terrainVertNormals;
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

        chunksWidth = 100;
        chunksHeight = 100;

        terrainDimX = 50;
        terrainDimZ = 50;

        // Create a Tringulated Mesh from the sequence of points
        //for (int X = 0; )
        terrainVertPos = new List<Vector3>();
        terrainVertColors = new List<Color>();
        terrainVertNormals = new List<Vector3>();

        float waveFrequency = 10;
        float XOffset = 0;
        float ZOffset = 0;
        float XSpacing = 0.5f;
        float ZSpacing = 0.5f;

        for (uint z = 0; z < terrainDimZ; ++z)
        {
            XOffset = 0;
            for (int x = 0; x < terrainDimX; ++x)
            {
                float YOffset = Mathf.Sin(Mathf.Deg2Rad * ((x + z) * waveFrequency + Time.fixedTime * 100));
                terrainVertPos.Add(new Vector3(XOffset, YOffset, ZOffset));
                //terrainVertColors.Add(new Color(z / terrainDimZ, 0.0f, 0.0f));

                if (debugging == true)
                {
                    // Debugging
                    GameObject newGO = Instantiate(sphereObject, new Vector3(XOffset, YOffset, ZOffset), Quaternion.identity);
                    newGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    newGO.name = x.ToString() + z.ToString();
                }

                terrainVertColors.Add(new Color(0.0f, 1.0f, 0.0f));

                XOffset += XSpacing;
            }
            ZOffset += ZSpacing;
        }

        if(debugging)
            Debug.Log("vertPos : " + terrainVertPos.Count);

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

        if (debugging == true)
        {
            Debug.Log("Vertex Index Count : " + vertexIndex.Count);
            Debug.Log("Vertex Colors Count : " + terrainVertColors.Count);
            Debug.Log("Vertex Normals Count : " + terrainVertNormals.Count);
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        mesh.vertices = terrainVertPos.ToArray();
        mesh.triangles = vertexIndex.ToArray();
        mesh.colors = terrainVertColors.ToArray();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
