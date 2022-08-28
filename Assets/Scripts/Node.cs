using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private float _directDistanceToEnd = 0;
    public float DirectDistanceToEnd
    {
        get => _directDistanceToEnd;
    }

    public void SetDirectDistanceToEnd(Vector3 endPos)
    {
        _directDistanceToEnd = Vector3.Distance(endPos, transform.position);
    }

    private float _pathWeight = int.MaxValue;
    public float PathWeight
    {
        get { return _pathWeight; }
        set { _pathWeight = value; }
    }

    public float PathWeightHeuristic
    {
        get { return _pathWeight + _directDistanceToEnd; }
        set { _pathWeight = value; }
    }

    [SerializeField] private Node _previousNode = null;
    public Node PreviousNode
    {
        get { return _previousNode; }
        set { _previousNode = value; }
    }

    [SerializeField] private List<Node> _neighbourNodes;
    public List<Node> NeighbourNodes
    {
        get
        {
            List<Node> result = new List<Node>(_neighbourNodes);
            return result;
        }
    }

    [SerializeField] private bool _disabled;
    public bool Disabled { get => _disabled; }

    void Start()
    {
        ResetNode();
        ValidateNeighbours();
    }

    private void OnValidate()
    {
        ValidateNeighbours();
    }

    public void ResetNode()
    {
        Debug.Log("Reset Triggered:" + gameObject.GetInstanceID());
        _pathWeight = int.MaxValue;
        _previousNode = null;
        _directDistanceToEnd = 0;
    }

    public void AddNeighbourNode(Node newNode)
    {
        if (!_neighbourNodes.Contains(newNode) && newNode != this)
        _neighbourNodes.Add(newNode);
    }

    private void ValidateNeighbours()
    {
        foreach (Node neighbour in _neighbourNodes)
        {
            if (neighbour == null) continue;
            if (!neighbour._neighbourNodes.Contains(this))
                neighbour.AddNeighbourNode(this);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Node neighbour in _neighbourNodes)
        {
            if (neighbour == null || neighbour._disabled) continue;
            Gizmos.color = _disabled ? Color.red : Color.cyan;
            Gizmos.DrawLine(transform.position, neighbour.transform.position);
        }
    }
    
    public void SetDisabled(bool active)
    {
        _disabled = active;
    }

}
