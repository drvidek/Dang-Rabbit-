using System.Collections.Generic;
using UnityEngine;

public class Rabbit : MonoBehaviour
{
    #region Variables
    protected RabbitSpawn _rabbitSpawn;//our home spawn
    [SerializeField] protected AStarGlobal _pathFinder;//our pathfinder
    public Node[] waypoints;//our list of waypoints
    protected int _waypointIndex = 0;//our current waypoint
    [SerializeField] protected float _travelDist;//the distance to travel
    [SerializeField] protected float _minDist;//the min distance threshold
    protected Vector3 _dir;//our current direction
    [SerializeField] protected Node _start;//our start node
    [SerializeField] protected Node _end;//our end node
    protected bool repathPending;//if we need to repath
    public bool RepathPending { set { repathPending = true; } }
    [SerializeField] protected bool _relocating;//if we are relocating
    protected bool _moving;//if we are moving
    public bool Moving { get => _moving; }
    protected float _travelTime;//how long we have to travel
    [SerializeField] protected float _travelTimeMax = 1.5f;

    public Node End { get => _end; }
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRend;
    private ParticleSystem _digPart; 
    #endregion

    public void Initialise(Node start, Node end, RabbitSpawn spawn, Node[] nodes)
    {
        //establish components
        if (_pathFinder == null)
            _pathFinder = GameObject.Find("GameManager").GetComponent<AStarGlobal>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRend == null)
            spriteRend = GetComponentInChildren<SpriteRenderer>();
        _digPart = GetComponentInChildren<ParticleSystem>();
        //set start and end points
        _start = start;
        _end = end;
        //establish your spawn
        _rabbitSpawn = spawn;
        //establish waypoints
        waypoints = nodes;
        //choose a direction
        NewDirection();
    }

    // Update is called once per frame
    void Update()
    {
        //if you have an end goal
        if (_end != null)
        {
            //move if you have travel time left, otherwise change waypoints
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

        //if we are more than the min distance from the end goal, move towards it
        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) > _minDist)
            transform.position += _dir * _travelDist * Time.deltaTime * 1f / 0.75f;
        else //else, snap to the goal and remove the destination
            transform.position = waypoints[_waypointIndex].transform.position;
        //reduce travel time towards 0
        _travelTime -= Time.deltaTime;
    }

    private void NewDirection()
    {
        //update the direction to the current waypoint
        _dir = (waypoints[_waypointIndex].transform.position - transform.position).normalized;
        //determine the distance to that point
        _travelDist = Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position);
        //set the travel time based on the distance
        _travelTime = _travelTimeMax * (_relocating ? Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) : 1);
        //animate
        spriteRend.flipX = (_dir.x < 0);
        animator.SetTrigger("Jump");
    }

    public bool PathImpacted()
    {
        Debug.Log("Check Path Impacted");
        //check all waypoints in your list to determine if any have been made disabled
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
        //if we are relocating, reset the sprite
        if (_relocating)
        {
            spriteRend.enabled = true;
            _relocating = false;
        }

        _moving = false;

        if (repathPending)  //if we have a repath pending, repath
        {
            Repath();
            return;
        }

        //increase the waypoint index
        _waypointIndex++;
        //if this is beyond the final waypoint
        if (_waypointIndex >= waypoints.Length)
        {
            //reset your index to 0
            _waypointIndex = 0;
            //change the start and end
            _start = _end;
            _end = null;
            //reduce lives by one
            GameManager.Singleton.ChangeLives(-1);
            //end the rabbit's life
            EndOfLife();
        }
        else //else update your direction
            NewDirection();
    }

    public void StartNewJourney()
    {
        //if you have a valid journey
        if (_start != null && _end != null)
        {
            //set the waypoint index to 0
            _waypointIndex = 0;

            //get the shortest path
            List<Node> nodes = _pathFinder.FindShortestPath(_start, _end);

            //if this path is valid
            if (nodes != null && nodes.Count > 1)
            {
                //get the waypoints and update your direction
                waypoints = nodes.ToArray();
                NewDirection();
            }
            else
            {
                //request relocate from your home spawn & prepare for relocate
                _rabbitSpawn.RabbitRequestRelocate(this);
                _relocating = true;
                spriteRend.enabled = false;
                _digPart.Play();
            }
        }
    }
    private void Repath()
    {
        //if your path is impacted
        if (PathImpacted())
        {
            //start a new path from your current node
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
        //remove the rabbit from the spawn's list
        _rabbitSpawn.RemoveRabbit(this);
        Destroy(gameObject);
    }

}
