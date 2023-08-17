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


    void Start()
    {
        sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereObject.transform.localScale = new Vector3(pointScale, pointScale, pointScale);

        TerrainDimX = 10;
        TerrainDimY = 10;

        old_TerrainDimX = 1;
        old_TerrainDimY = 1;

        pointScale = 0.1f;
        old_pointScale = 1.0f;

        waveFrequency = 10;

        XSpacing = 1;
        YSpacing = 1;
}

    // Update is called once per frame
    void Update()
    {
        bool toDelete = false;

        if (old_TerrainDimX != TerrainDimX)
        {
            toDelete = true;
        }

        if (old_TerrainDimY != TerrainDimY)
        {
            toDelete = true;
        }

        if (old_pointScale != pointScale)
        {
            toDelete = true;
        }

        float XOffset = 0;
        float YOffset = 0;

        if (toDelete == true)
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

            for (int x = 0; x < TerrainDimX; ++x)
            {
                XOffset = 0;
                for (int y = 0; y < TerrainDimY; ++y)
                {
                    GameObject newGO = Instantiate(sphereObject, new Vector3(XOffset,0,YOffset), Quaternion.identity);
                    newGO.transform.localScale = new Vector3(pointScale, pointScale, pointScale);
                    newGO.name = x.ToString() + y.ToString();
                    XOffset += XSpacing;
                }
                YOffset += YSpacing;
            }
        }

        XOffset = 0;
        YOffset = 0;

        for (int x = 0; x < TerrainDimX; ++x)
        {
            XOffset = 0;
            for (int y = 0; y < TerrainDimY; ++y)
            {
                string gameObjectName = x.ToString() + y.ToString();
                GameObject newGO = GameObject.Find(gameObjectName);
                newGO.transform.position = new Vector3(XOffset, Mathf.Sin(Mathf.Deg2Rad * ((x + y) * waveFrequency + Time.fixedTime * 100)), YOffset);

                if (newGO.transform.position.y > 0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 1.0f, 0.7f, 0.0f));
                else if (newGO.transform.position.y < 0.5f && newGO.transform.position.y > -0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f, 0.0f));
                else if (newGO.transform.position.y < -0.5f)
                    newGO.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f, 0.0f));

                XOffset += XSpacing;
            }
            YOffset += YSpacing;
        }
    }
}
