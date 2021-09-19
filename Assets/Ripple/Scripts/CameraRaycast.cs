using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRaycast : MonoBehaviour
{
    public static float drawRadius = 0.2f;
    public static bool isRaycast = false;
    public static Vector4 currentPos;

    Camera m_camera;

    void Start()
    {
        m_camera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isRaycast = true;
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                currentPos = new Vector4(hit.textureCoord.x, hit.textureCoord.y, drawRadius);
                Shader.SetGlobalVector("_PositionPoint", currentPos);
            }
        }
        else
        isRaycast = false;
        


    }
}
