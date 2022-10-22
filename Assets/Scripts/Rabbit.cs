using System.Collections.Generic;
using UnityEngine;

public class Rabbit : AStarAgent
{
    public Node End { get => _end; }
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRend;

    public void Initialise(Node start, Node end, RabbitSpawn spawn, Node[] nodes)
    {
        if (_pathFinder == null)
            _pathFinder = GameObject.Find("GameManager").GetComponent<AStarGlobal>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRend == null)
            spriteRend = GetComponentInChildren<SpriteRenderer>();
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

    override protected void NewDirection()
    {
        base.NewDirection();
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

    new void UpdateWaypointIndex()
    {
        
        if (_relocating)
        {
            spriteRend.enabled = true;
            _relocating = false;
        }

        base.UpdateWaypointIndex();
    }

    new public void StartNewJourney()
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
            }
        }
    }
    new protected void Repath()
    {
        Debug.Log("Repath requested");
        if (PathImpacted())
        {
            Debug.Log("Repath approved");
            _start = waypoints[_waypointIndex];
            StartNewJourney();
        }
        base.Repath();
    }

    public void EndOfLife()
    {
        _rabbitSpawn.RemoveRabbit(this);
        Destroy(gameObject);
    }

}
