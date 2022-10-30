using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FarmerState { move, attack }

public class Farmer : MonoBehaviour
{
    public FarmerState state;
    [SerializeField] private float _moveSpd, _minDist = 0.02f;
    [SerializeField] private float _cooldown, _cooldownMax;
    [SerializeField] private List<Rabbit> _targetList = new List<Rabbit>();
    [SerializeField] private Net _net;
    private Node _rabbitEnd;
    private Transform _targetDest;
    [SerializeField] private Node _currentNode;
    private Image _cooldownImage;
    private Animator _anim;

    private void Start()
    {
        state = FarmerState.attack;
        Initialise();
    }

    public void Initialise()
    {
        _rabbitEnd = GameObject.Find("End").GetComponent<Node>();
        _cooldownImage = GetComponentInChildren<Image>();
        _currentNode.SetDisabled(false);
        _anim = GetComponentInChildren<Animator>();
        NextState();
    }

    void NextState()
    {
        if (!GameManager.Singleton.IsPlaying)
            return; 
        
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
        //_cooldown = _cooldownMax;

        while (state == FarmerState.attack)
        {
            if (!GameManager.Singleton.IsPlaying)
                break;

            if (_targetDest != null && _net.NetFree)
            {
                state = FarmerState.move;
                break;
            }

            if (_cooldown == 0)
            {
                for (int i = 0; i < _targetList.Count; i++)
                {
                    if (_targetList[i] == null)
                    {
                        _targetList.RemoveAt(i);
                        i--;
                    }
                }
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
                        _anim.SetTrigger("Jump");
                    }
                }
            }
            UpdateCooldown();

            yield return null;
        }
        NextState();
    }

    IEnumerator MoveState()
    {
        _net.gameObject.SetActive(false);
        _anim.SetBool("Walk", true);
        while (state == FarmerState.move)
        {
            if (!GameManager.Singleton.IsPlaying)
                break;

            if (_targetDest != null)
            {
                Move();
            }
            else
                state = FarmerState.attack;
            
            UpdateCooldown();

            yield return null;
        }
        RabbitSpawn.RepathRabbits();
        _net.transform.localPosition = Vector3.up/2;
        _net.gameObject.SetActive(true);
        _net.Initialise();
        _anim.SetBool("Walk", false);
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

    private void UpdateCooldown()
    {
        _cooldown = Mathf.MoveTowards(_cooldown, 0, Time.deltaTime);

        _cooldownImage.fillAmount = _cooldown / _cooldownMax;
    }
}
