using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public enum FarmerState { move, attack }

public class Farmer : AStarAgent
{
    public FarmerState state;
    [SerializeField] private float _cooldown, _cooldownMax;
    [SerializeField] private List<Rabbit> _targetList = new List<Rabbit>();
    [SerializeField] private Net _net;
    private Node _rabbitEnd;

    private void Start()
    {
        state = FarmerState.attack;
        Initialise();
    }

    public void Initialise()
    {
        _rabbitEnd = GameObject.Find("End").GetComponent<Node>();
        NextState();
    }

    void NextState()
    {
        switch (state)
        {
            case FarmerState.move:
                StartCoroutine("MoveState");
                break;
            case FarmerState.attack:
                StartCoroutine("AttackState");
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    IEnumerator AttackState()
    {
        _cooldown = _cooldownMax;
        while (state == FarmerState.attack)
        {
            if (_cooldown == 0)
            {
                if (_targetList.Count > 0)
                {
                    Rabbit nearest = null;
                    float dist = float.PositiveInfinity;
                    foreach (Rabbit rabbit in _targetList)
                    {
                        float newDist = Vector3.Distance(rabbit.transform.position, _rabbitEnd.transform.position);
                        if (nearest == null || (dist > newDist && !rabbit.Moving && rabbit.End != null))
                        {
                            nearest = rabbit;
                            dist = newDist;
                        }
                    }

                    if (nearest != null)
                    {
                        _targetList.Remove(nearest);
                        nearest.TerminatePath();
                        _net.SetTarget(nearest, nearest.transform.position);
                        //Destroy(nearest.gameObject);
                        _cooldown = _cooldownMax;
                    }
                }
            }
            _cooldown = Mathf.MoveTowards(_cooldown, 0, Time.deltaTime);
            yield return null;
        }
        NextState();
    }

    IEnumerator MoveState()
    {
        while (state == FarmerState.move)
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
            else
                state = FarmerState.attack;
            yield return null;
        }
        NextState();
    }

    public void SetDestination(Node end)
    {
        _end = end;
    }

    override protected void Repath()
    {
        _start = waypoints[_waypointIndex];
        base.Repath();
        StartNewJourney();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            _targetList.Add(rabbit);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            if (_targetList.Contains(rabbit))
                _targetList.Remove(rabbit);
        }
    }
}
