using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rope : MonoBehaviour
{
    class RopeNode
    {
        public bool locked;
        public Vector3 position;
        public Vector3 previousPosition;
        
        public RopeNode(Vector3 position)
        {
            this.position = position;
            this.previousPosition = position;
            this.locked = false;
        }
    }
    
    class RopeSegment
    {
        public int node0Index;
        public int node1Index;

        public RopeSegment(int node0Index, int node1Index)
        {
            this.node0Index = node0Index;
            this.node1Index = node1Index;
        }
    }

    
    [Range(1,5)] public int m_StiffnessIterationCounts = 2;
    public int m_SegmentCount = 10;
    public float m_TotalLength = 1f;
    private float m_singleSegmentLength;
    
    private List<RopeNode> m_nodes = new List<RopeNode>();
    private List<RopeSegment> m_ropeSegments = new List<RopeSegment>();
    private Mesh m_mesh;
    private LineRenderer m_lineRenderer;
    private MeshCollider m_meshCollider;
    private void Start()
    {
        m_mesh = new Mesh();
        m_singleSegmentLength = m_TotalLength / m_SegmentCount;
        m_lineRenderer = GetComponent<LineRenderer>();
        m_meshCollider = GetComponent<MeshCollider>();
        m_lineRenderer.positionCount = m_SegmentCount + 1;
        m_lineRenderer.useWorldSpace = false;
        
        for (int i = 0; i < m_SegmentCount + 1; i++)
        {
            RopeNode _ropeNode = new RopeNode(transform.position + Vector3.down * i * m_singleSegmentLength);
            m_nodes.Add(_ropeNode);
        }

        for (int i = 0; i < m_SegmentCount; i++)
        {
            RopeSegment _ropeSegment = new RopeSegment(i, i + 1);
            m_ropeSegments.Add(_ropeSegment);
        }
        
        m_nodes[0].locked = true;
    }

    private void FixedUpdate()
    {
        m_nodes[0].position = transform.position;
        
        for(int i = 0; i < m_nodes.Count; i++)
        {
            RopeNode _ropeNode = m_nodes[i];
            if (!_ropeNode.locked)
            {
                Vector3 _currentPosition = _ropeNode.position;
                _ropeNode.position = _ropeNode.position + (_currentPosition - _ropeNode.previousPosition) +
                                     Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
                _ropeNode.previousPosition = _currentPosition;
            }
        }
        for (int j = 0; j < m_StiffnessIterationCounts; j++)
        {
            for (int i = 0; i < m_ropeSegments.Count; i++)
            {
                RopeSegment _ropeSegment = m_ropeSegments[i];
                RopeNode _node0 = m_nodes[_ropeSegment.node0Index];
                RopeNode _node1 = m_nodes[_ropeSegment.node1Index];
                Vector3 _delta = _node1.position - _node0.position;
                float _distance = _delta.magnitude;
                float _error = _distance - m_singleSegmentLength;
                Vector3 _errorVector = _delta.normalized * _error;
                if(!_node0.locked)
                    _node0.position += _errorVector * 0.5f;
                if(!_node1.locked)
                    _node1.position -= _errorVector * 0.5f;
            }
        }
        
        m_lineRenderer.BakeMesh(m_mesh, false);
        m_meshCollider.sharedMesh = m_mesh;
    }

    private void Update()
    {
        for (int i = 0; i < m_nodes.Count; i++)
        {
            m_lineRenderer.SetPosition(i, m_nodes[i].position);
        }
    }
}
