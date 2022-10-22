using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour
{
    protected RabbitSpawn _rabbitSpawn;
    [SerializeField] protected AStarGlobal _pathFinder;
    public Node[] waypoints;
    protected int _waypointIndex = 0;
    [SerializeField] protected float _travelDist;
    [SerializeField] protected float _minDist;
    protected Vector3 _dir;
    [SerializeField] protected Node _start;
    [SerializeField] protected Node _end;
    protected bool repathPending;
    public bool RepathPending { set { repathPending = true; } }

    [SerializeField] protected bool _relocating;

    protected bool _moving;
    public bool Moving { get => _moving; }

    protected float _travelTime;
    [SerializeField] protected float _travelTimeMax = 1.5f;

    // Start is called before the first frame update
    public void Initialise(Node start, Node end, RabbitSpawn spawn, Node[] nodes)
    {
        if (_pathFinder == null)
            _pathFinder = GameObject.Find("GameManager").GetComponent<AStarGlobal>();
        _start = start;
        _end = end;
        waypoints = nodes;
        NewDirection();
    }

    protected void Move()
    {
        _moving = true;

        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) > _minDist)
            transform.position += _dir * _travelDist * Time.deltaTime * 1f / 0.75f;
        else
            transform.position = waypoints[_waypointIndex].transform.position;
        _travelTime -= Time.deltaTime;
    }

    virtual protected void NewDirection()
    {
        _dir = (waypoints[_waypointIndex].transform.position - transform.position).normalized;

        _travelDist = Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position);
        _travelTime = _travelTimeMax * (_relocating ? Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) : 1);
    }

    virtual protected void Repath()
    {
        repathPending = false;
    }

    public void TerminatePath()
    {
        Debug.Log("Path terminated");
        _end = null;
    }

    

    protected void UpdateWaypointIndex()
    {
        Debug.Log("Updating waypoints");

        _moving = false;

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
        }
        else
            NewDirection();
    }

    public void StartNewJourney()
    {
        if (_start != null && _end != null)
        {
            _waypointIndex = 0;

            List<Node> nodes = _pathFinder.FindShortestPath(_start, _end);
            if (nodes != null && nodes.Count > 1)
            {
                waypoints = nodes.ToArray();
                NewDirection();
            }
        }
    }

}
