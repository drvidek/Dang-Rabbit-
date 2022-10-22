using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSpawn : MonoBehaviour
{
    [SerializeField] private GameObject _rabbitPrefab;
    [SerializeField] private float _spawnTimerMax = 1f;
    [SerializeField] private Node _startNode;
    [SerializeField] private Node _endNode;
    [SerializeField] private Node[] _path;
    [SerializeField] private AStarGlobal _pathFinder;
    private float _spawnTimer;
    private static List<Rabbit> _rabbits = new List<Rabbit>();
    [SerializeField] private GameObject _mound;

    void Start()
    {
        _startNode = GetComponent<Node>();
        _endNode = GameObject.Find("End").GetComponent<Node>();
        _spawnTimer = _spawnTimerMax;
        _path = _pathFinder.FindShortestPath(_startNode, _endNode).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (PathImpacted())
        {
            RelocateTriggered();
            return;
        }

        if (_spawnTimer <= 0)
        {
            Rabbit rabbit = Instantiate(_rabbitPrefab, _startNode.transform.position, Quaternion.identity, null).GetComponent<Rabbit>();
            rabbit.Initialise(_startNode, _endNode, this, _path);
            _rabbits.Add(rabbit);
            _spawnTimer = _spawnTimerMax;
        }
        else
            _spawnTimer -= Time.deltaTime;
    }

    public void RemoveRabbit(Rabbit rabbit)
    {
        if (_rabbits.Contains(rabbit))
        {
            _rabbits.Remove(rabbit);
        }
    }

    void RelocateTriggered()
    {
        Debug.Log("Relocate triggered");
        List<Node> nodes = _pathFinder.FindShortestPath(_startNode, _endNode);
        if (nodes != null && nodes.Count > 1)
        {
            _path = nodes.ToArray();
            return;
        }

        nodes = _pathFinder.FindShortestPath(_startNode, _endNode, false);
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i].Disabled)
            {
                Node newNode = nodes[i + 1];
                _startNode = newNode;
                nodes.RemoveRange(0, i);
                _path = nodes.ToArray();
                Instantiate(_mound, _startNode.transform);
                return;
            }
        }
    }

    public void RabbitRequestRelocate(Rabbit rabbit)
    {
        rabbit.Initialise(_startNode, _endNode, this, _path);
    }

    public static void RepathRabbits()
    {
        foreach (Rabbit rabbit in _rabbits)
        {
            rabbit.RepathPending = true;
        }
    }

    public bool PathImpacted()
    {
        Debug.Log("Check Path Impacted");

        for (int i = 0; i < _path.Length; i++)
        {
            if (_path[i].Disabled)
            {
                Debug.Log("Path Impacted");
                return true;
            }
        }
        return false;
    }
}
