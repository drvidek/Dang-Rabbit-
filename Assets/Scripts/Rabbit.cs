using System.Collections.Generic;
using UnityEngine;

public class Rabbit : MonoBehaviour
{
    [SerializeField] private AStar _pathFinder;
    public Node[] waypoints;
    private int _waypointIndex = 1;
    [SerializeField] private float _travelDist;
    [SerializeField] private float _minDist;
    private Vector3 _dir;
    [SerializeField] private Node _start;
    [SerializeField] private Node _end;
    public Node End { get => _end; }
    private bool repathPending;
    public bool RepathPending { set { repathPending = true; } }

    private bool _moving;
    public bool Moving { get => _moving; }

    private float _travelTime;
    [SerializeField] private float _travelTimeMax = 1.5f;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRend;

    public void Initialise(Node start, Node end)
    {
        if (_pathFinder == null)
            _pathFinder = GetComponent<AStar>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRend == null)
            spriteRend = GetComponentInChildren<SpriteRenderer>();
        GameManager.rabbits.Add(this);
        _start = start;
        _end = end;
        StartNewJourney();
    }

    // Update is called once per frame
    void Update()
    {
        if (_end != null)
        {
            if (_travelTime > 0)
            {
                Move();
            }
            else
            {
                UpdateWaypointIndex();
            }
        }
    }

    void Move()
    {
        _moving = true;

        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) > _minDist)
            transform.position += _dir * _travelDist * Time.deltaTime * 1f / 0.75f;
        else
            transform.position = waypoints[_waypointIndex].transform.position;
        _travelTime -= Time.deltaTime;
    }

    void NewDirection()
    {
        _dir = (waypoints[_waypointIndex].transform.position - transform.position).normalized;
        spriteRend.flipX = (_dir.x < 0);

        _travelDist = Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position);
        animator.SetTrigger("Jump");
        _travelTime = _travelTimeMax;
    }

    private void Repath()
    {
        if (PathImpacted())
        {
            if (!waypoints[_waypointIndex].Disabled)
                _start = waypoints[_waypointIndex];
            else
                _start = waypoints[_waypointIndex - 1];
            StartNewJourney();
        }
        repathPending = false;
    }

    public void TerminatePath()
    {
        _end = null;
    }

    bool PathImpacted()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].Disabled)
                return true;
        }
        return false;
    }

    void UpdateWaypointIndex()
    {
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
            List<Node> nodes = GameObject.Find("GameManager").GetComponent<AStarGlobal>().FindShortestPath(_start, _end);
            waypoints = new Node[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                waypoints[i] = nodes[i];
            }
            _waypointIndex = 0;
            NewDirection();
        }
    }
}
