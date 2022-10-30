using System.Collections.Generic;
using UnityEngine;

public class Rabbit : MonoBehaviour
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

    public Node End { get => _end; }
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRend;
    private ParticleSystem _digPart;

    public void Initialise(Node start, Node end, RabbitSpawn spawn, Node[] nodes)
    {
        if (_pathFinder == null)
            _pathFinder = GameObject.Find("GameManager").GetComponent<AStarGlobal>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRend == null)
            spriteRend = GetComponentInChildren<SpriteRenderer>();
        _digPart = GetComponentInChildren<ParticleSystem>();
        _start = start;
        _end = end;
        _rabbitSpawn = spawn;
        waypoints = nodes;
        NewDirection();
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

    private void Move()
    {
        _moving = true;

        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) > _minDist)
            transform.position += _dir * _travelDist * Time.deltaTime * 1f / 0.75f;
        else
            transform.position = waypoints[_waypointIndex].transform.position;
        _travelTime -= Time.deltaTime;
    }

    private void NewDirection()
    {
        _dir = (waypoints[_waypointIndex].transform.position - transform.position).normalized;

        _travelDist = Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position);
        _travelTime = _travelTimeMax * (_relocating ? Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) : 1);
        spriteRend.flipX = (_dir.x < 0);
        animator.SetTrigger("Jump");
    }

    public bool PathImpacted()
    {
        Debug.Log("Check Path Impacted");

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].Disabled)
            {
                Debug.Log("Path Impacted");
                return true;
            }

        }
        return false;
    }

    void UpdateWaypointIndex()
    {

        if (_relocating)
        {
            spriteRend.enabled = true;
            _relocating = false;
        }

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
            GameManager.Singleton.ChangeLives(-1);
            EndOfLife();
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
            else
            {
                _rabbitSpawn.RabbitRequestRelocate(this);
                _relocating = true;
                spriteRend.enabled = false;
                _digPart.Play();
            }
        }
    }
    private void Repath()
    {
        Debug.Log("Repath requested");
        if (PathImpacted())
        {
            Debug.Log("Repath approved");
            _start = waypoints[_waypointIndex];
            StartNewJourney();
        }
        repathPending = false;
    }


    public void TerminatePath()
    {
        Debug.Log("Path terminated");
        _end = null;
    }

    public void EndOfLife()
    {
        _rabbitSpawn.RemoveRabbit(this);
        Destroy(gameObject);
    }

}
