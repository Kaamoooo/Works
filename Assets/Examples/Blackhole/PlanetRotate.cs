using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotate : MonoBehaviour
{
    public float m_RotateSpeed = 1.0f;
    
    private float m_radius = 0.0f;
    private float m_theta = 0.0f;
    void Start()
    {
        m_radius = transform.position.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        m_theta += m_RotateSpeed * Time.deltaTime;
        Vector3 pos = new Vector3(Mathf.Cos(m_theta), 0, Mathf.Sin(m_theta)) * m_radius;
        transform.position = pos;
    }
}
