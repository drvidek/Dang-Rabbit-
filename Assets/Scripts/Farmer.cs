using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public enum FarmerState { move, attack }

public class Farmer : MonoBehaviour
{
    public FarmerState state;
    [SerializeField] private float _moveSpd,_minDist = 0.02f;
    [SerializeField] private float _cooldown, _cooldownMax;
    [SerializeField] private List<Rabbit> _targetList = new List<Rabbit>();
    [SerializeField] private Net _net;
    private Node _rabbitEnd;
    private Transform _targetDest;
    [SerializeField] private Node _currentNode;

    private void Start()
    {
        state = FarmerState.attack;
        Initialise();
    }

    public void Initialise()
    {
        _rabbitEnd = GameObject.Find("End").GetComponent<Node>();
        _currentNode.SetDisabled(false);
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
            if (_targetDest != null && _net.NetFree)
            {
                state = FarmerState.move;
                break;
            }

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
        _net.gameObject.SetActive(false);
        while (state == FarmerState.move)
        {
            if (_targetDest != null)
            {
                Move();
                _net.transform.localPosition = Vector3.zero;
            }
            else
                state = FarmerState.attack;
            yield return null;
        }
        RabbitSpawn.RepathRabbits();
        _net.gameObject.SetActive(true);
        _net.Initialise();
        NextState();
    }


    private void Move()
    {
        if (Vector3.Distance(transform.position, _targetDest.position) > _minDist)
        transform.position += MathExt.Direction(transform.position, _targetDest.position) * _moveSpd * Time.deltaTime;
        else
        {
            transform.position = _targetDest.position;
            _targetDest = null;
        }
    }

    public void SetDestination(Node end)
    {
        _currentNode.SetDisabled(false);
        end.SetDisabled(true);
        _targetDest = end.transform;
        _currentNode = end;
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
