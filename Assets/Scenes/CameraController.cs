using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    // Start is called before the first frame update
    private Camera cameraHandle;
    
    
    void Start()
    {
        cameraHandle = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cameraHandle.transform.position += cameraHandle.transform.right * 5.0f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            cameraHandle.transform.position -= cameraHandle.transform.right * 5.0f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            cameraHandle.transform.position -= cameraHandle.transform.up * 5.0f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            cameraHandle.transform.position += cameraHandle.transform.up * 5.0f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            cameraHandle.transform.position += cameraHandle.transform.forward * 5.0f * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            cameraHandle.transform.position -= cameraHandle.transform.forward * 5.0f * Time.deltaTime;
        }
    }
}
