using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Sphere : MonoBehaviour
{
    // Start is called before the first frame update
    public int TerrainDimX;
    public int TerrainDimY;
    private int old_TerrainDimX;
    private int old_TerrainDimY;

    public float pointScale;
    private float old_pointScale = 0.1f;

    private GameObject sphereObject;

    public float waveFrequency;

    public float XSpacing;
    public float YSpacing;

    private List<List<Vector3>> GridPoints2D;

    public bool LoadPointsInGrid;

    public bool clearPoints;
    public bool PlayPauseSim;

    void Start()
    {
       sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
       sphereObject.transform.localScale = new Vector3(pointScale, pointScale, pointScale);

        TerrainDimX = 20;
        TerrainDimY = 20;

        old_TerrainDimX = 1;
        old_TerrainDimY = 1;

        pointScale = 0.1f;
        old_pointScale = 1.0f;

        waveFrequency = 10;

        XSpacing = 1;
        YSpacing = 1;

        GridPoints2D = new List<List<Vector3>>();
        LoadPointsInGrid = false;

        clearPoints = false;
        PlayPauseSim = false;
    }

    void AddGridPoints()
    {
        float XOffset = 0;

        for (int z = 0; z < TerrainDimX; ++z)
        {
            XOffset = 0;
            for (int x = 0; x < TerrainDimY; ++x)
            {
                GameObject newGO = Instantiate(sphereObject, new Vector3(XOffset, 0, z), Quaternion.identity);
                newGO.transform.localScale = new Vector3(pointScale, pointScale, pointScale);
                newGO.name = x.ToString() + z.ToString();
                XOffset += XSpacing;
            }
        }
    }

    void UpdateGridPoints() 
    {
        float XOffset = 0;

        for (int z = 0; z < TerrainDimX; ++z)
        {
            XOffset = 0;
            for (int x = 0; x < TerrainDimY; ++x)
            {
                string gameObjectName = x.ToString() + z.ToString();
                GameObject newGO = GameObject.Find(gameObjectName);
                newGO.transform.position = new Vector3(XOffset, Mathf.Sin(Mathf.Deg2Rad * ((x + z) * waveFrequency + Time.fixedTime * 100)), z);

                if (newGO.transform.position.y > 0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 1.0f, 0.7f, 0.0f));
                else if (newGO.transform.position.y < 0.5f && newGO.transform.position.y > -0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f, 0.0f));
                else if (newGO.transform.position.y < -0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f, 0.0f));

                XOffset += XSpacing;
            }
        }
    }

    void AddGridPointsToList()
    {
        GridPoints2D.Clear();

        for (int x = 0; x < TerrainDimX; ++x)
        {
            List<Vector3> rowPointsX = new List<Vector3>();
            for (int y = 0; y < TerrainDimY; ++y)
            {
                string gameObjectName = x.ToString() + y.ToString();
                GameObject newGO = GameObject.Find(gameObjectName);
                rowPointsX.Add(newGO.transform.position);
            }
            GridPoints2D.Add(rowPointsX);
        }

    }

    // Update is called once per frame
    void Update()
    {
   /*     if (old_TerrainDimX != TerrainDimX)
        {
            clearPoints = true;
        }

        if (old_TerrainDimY != TerrainDimY)
        {
            clearPoints = true;
        }

        if (old_pointScale != pointScale)
        {
            clearPoints = true;
        }

        if (clearPoints == true)
        {
            for (int x = 0; x < old_TerrainDimX; ++x)
            {
                for (int y = 0; y < old_TerrainDimY; ++y)
                {
                    string gameObjectName = x.ToString() + y.ToString();
                    Destroy(GameObject.Find(gameObjectName));
                }
            }

            old_TerrainDimX = TerrainDimX;
            old_TerrainDimY = TerrainDimY;
            old_pointScale = pointScale;


            AddGridPoints();
            clearPoints = false;
        }

        if (PlayPauseSim == true)
        { 
            UpdateGridPoints();
        }

        if (LoadPointsInGrid == true)
        {
            AddGridPointsToList();
            LoadPointsInGrid = false;
        }*/
    }
}
