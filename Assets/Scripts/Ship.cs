using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] private GameObject _pathManager;
    public Transform[] waypoints;
    private int _waypointIndex = 1;
    [SerializeField] private float _minDist;
    [SerializeField] private float _spd;
    [SerializeField] private float _maxSpd = 5f;
    private Vector3 _dir;
    [SerializeField] private Node _start;
    [SerializeField] private Node _end;
    [SerializeField] bool _aStar;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = _start.transform.position;
        StartNewJourney();
    }

    // Update is called once per frame
    void Update()
    {
        if (_end != null)
        {
            if (Vector3.Distance(transform.position, waypoints[_waypointIndex].position) > _minDist)
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
        _dir = (waypoints[_waypointIndex].position - transform.position).normalized;
        //transform.forward = _dir;
    }

    void UpdateWaypointIndex()
    {
        transform.position = waypoints[_waypointIndex].position;

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

    public void StartNewJourney()
    {
        if (_start != null && _end != null)
        {
            List<Node> nodes;
            if (_aStar)
                nodes = _pathManager.GetComponent<AStar>().FindShortestPath(_start, _end);
            else
                nodes = _pathManager.GetComponent<Dijkstra>().FindShortestPath(_start, _end);
            Transform[] tempArray = new Transform[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                tempArray[i] = nodes[i].transform;
            }
            waypoints = tempArray;
            _spd = _maxSpd;
            NewDirection();
        }
    }

    //public void GetNodeOnClick()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;
    //        if (Physics.Raycast(ray, out hit))
    //        {
    //            if (hit.collider.TryGetComponent<Node>(out Node n))
    //            {
    //                _end = n;
    //                StartNewJourney();
    //            }
    //        }

    //    }
    //}

}
