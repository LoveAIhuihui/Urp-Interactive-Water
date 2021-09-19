using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObj : MonoBehaviour
{
    [Range(0,1)]
    public float InteractiveStength = 0.01f;

    Vector3 lastPos;
    private Renderer m_renderer;

    void Start()
    {
        lastPos = transform.position;
        m_renderer = GetComponent<Renderer>();
        m_renderer.material.SetFloat("_InteractiveStength", InteractiveStength);
    }

    void Update()
    {
        if(m_renderer.enabled)
        {
            m_renderer.enabled = false;
        }

        if((transform.position - lastPos).sqrMagnitude > 0.01f)
        {
            lastPos = transform.position;
            m_renderer.enabled = true;
        }
    }
}
