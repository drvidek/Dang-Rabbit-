using System.Collections.Generic;
using UnityEngine;

public class Rabbit : MonoBehaviour
{
    [SerializeField] private AStar _pathFinder;
    public Node[] waypoints;
    private int _waypointIndex = 1;
    [SerializeField] private float _minDist;
    [SerializeField] private float _spd;
    [SerializeField] private float _maxSpd = 5f;
    private Vector3 _dir;
    [SerializeField] private Node _start;
    [SerializeField] private Node _end;
    [SerializeField] bool _aStar;
    private bool repathPending;
    public bool RepathPending { set { repathPending = true; } }

    private bool _journeyFound = false;

    [SerializeField] private bool _useAsync = false;

    // Start is called before the first frame update
    void Start()
    {
        if (_pathFinder == null)
            _pathFinder = GetComponent<AStar>();
        transform.position = _start.transform.position;
        GameManager.rabbits.Add(this);
        StartNewJourney();
    }

    // Update is called once per frame
    void Update()
    {
        if (_journeyFound && _end != null)
        {
            if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) > _minDist)
            {
                Move();
            }
            else
            {
                UpdateWaypointIndex();
            }
        }
        //else
        //    GetNodeOnClick();
    }

    void Move()
    {
        transform.position += _dir * _spd * Time.deltaTime;
    }

    void NewDirection()
    {
        _dir = (waypoints[_waypointIndex].transform.position - transform.position).normalized;
        //transform.forward = _dir;
    }

    private void Repath()
    {
        _start = waypoints[_waypointIndex];
        StartNewJourney();
        repathPending = false;
    }


    void UpdateWaypointIndex()
    {
        transform.position = waypoints[_waypointIndex].transform.position;
        if (repathPending)
        {
            Repath();
            return;
        }
        _waypointIndex++;
        if (_waypointIndex >= waypoints.Length)
        {
            _waypointIndex = 0;
            _start = _end;
            _end = null;
            _spd = 0;
            Vector3 angle = transform.eulerAngles;
            angle.x = 0;
            transform.eulerAngles = angle;
        }
        else
            NewDirection();
    }

    public async void StartNewJourney()
    {
        _journeyFound = false;
        if (_start != null && _end != null)
        {
            List<Node> nodes = new List<Node>();
            if (_useAsync)
            {
                nodes = await _pathFinder.FindShortestPath(_start, _end);
            }
            else
            {
                nodes = GameObject.Find("GameManager").GetComponent<AStarGlobal>().FindShortestPath(_start, _end);
            }
            waypoints = new Node[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                waypoints[i] = nodes[i];
            }
            _waypointIndex = 0;
            _spd = _maxSpd;
            _journeyFound = true;
            NewDirection();
        }
    }
}
