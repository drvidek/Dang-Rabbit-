using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FarmerState { move, attack }

public class Farmer : MonoBehaviour
{
    #region Variables
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
    #endregion

    private void Start()
    {
        state = FarmerState.attack;
        Initialise();
    }

    public void Initialise()
    {
        //find the rabbit's end goal
        _rabbitEnd = GameObject.Find("End").GetComponent<Node>();
        //set the cooldown image
        _cooldownImage = GetComponentInChildren<Image>();
        //disable the node you're currently on
        _currentNode.SetDisabled(false);
        //get the animator
        _anim = GetComponentInChildren<Animator>();
        //start the state machine
        NextState();
    }

    void NextState()
    {
        //if the game is over, do nothing
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
        while (state == FarmerState.attack)
        {
            //if the game is over, do nothing
            if (!GameManager.Singleton.IsPlaying)
                break;

            //if we have a new destination to move towards and our net is not currently moving
            if (_targetDest != null && _net.NetFree)
            {
                //change states
                state = FarmerState.move;
                break;
            }

            //if our attack cooldown is up
            if (_cooldown == 0)
            {
                //loop our target list and remove any invalid entries
                for (int i = 0; i < _targetList.Count; i++)
                {
                    if (_targetList[i] == null)
                    {
                        _targetList.RemoveAt(i);
                        i--;
                    }
                }

                //if we have any targets
                if (_targetList.Count > 0)
                {
                    //establish some initial values
                    Rabbit nearest = null;
                    float dist = float.PositiveInfinity;
                    //loop through the target list and find the rabbit nearest to the end
                    foreach (Rabbit rabbit in _targetList)
                    {
                        float newDist = Vector3.Distance(rabbit.transform.position, _rabbitEnd.transform.position);
                        if (nearest == null || (dist > newDist && !rabbit.Moving && rabbit.End != null))
                        {
                            nearest = rabbit;
                            dist = newDist;
                        }
                    }

                    //if we found a target
                    if (nearest != null)
                    {
                        //remove the target from the list
                        _targetList.Remove(nearest);
                        //terminate the target's path
                        nearest.TerminatePath();
                        //set the net's target
                        _net.SetTarget(nearest, nearest.transform.position);
                        //reset the cooldown
                        _cooldown = _cooldownMax;
                        //trigger the animation
                        _anim.SetTrigger("Jump");
                    }
                }
            }
            //manage the cooldown
            UpdateCooldown();

            yield return null;
        }
        NextState();
    }

    IEnumerator MoveState()
    {
        //disable the net
        _net.gameObject.SetActive(false);
        //activate the walking animation
        _anim.SetBool("Walk", true);
        //while moving
        while (state == FarmerState.move)
        {
            //if the game is over, do nothing
            if (!GameManager.Singleton.IsPlaying)
                break;

            //if we still have a valid destination move towards it
            if (_targetDest != null)
            {
                Move();
            }
            else    //else start attacking
                state = FarmerState.attack;
            
            UpdateCooldown();

            yield return null;
        }
        //repath rabbits on arrival
        RabbitSpawn.RepathRabbits();
        //reposition the net
        _net.transform.localPosition = Vector3.up/2;
        //reactivate the net
        _net.gameObject.SetActive(true);
        //reinitialise the net
        _net.Initialise();
        //stop walk anim
        _anim.SetBool("Walk", false);
        NextState();
    }


    private void Move()
    {
        //if we are more than the min distance from the end goal, move towards it
        if (Vector3.Distance(transform.position, _targetDest.position) > _minDist)
            transform.position += MathExt.Direction(transform.position, _targetDest.position) * _moveSpd * Time.deltaTime;
        else    //else, snap to the goal and remove the destination
        {
            transform.position = _targetDest.position;
            _targetDest = null;
        }
    }

    public void SetDestination(Node end)
    {
        //enable the current node and disable the target node
        _currentNode.SetDisabled(false);
        end.SetDisabled(true);
        //set the new target to our destination
        _targetDest = end.transform;
        _currentNode = end;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if a rabbit enters our radius, add it to the target list
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            _targetList.Add(rabbit);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if a rabbit leaves our radius, remove it from the list
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            if (_targetList.Contains(rabbit))
                _targetList.Remove(rabbit);
        }
    }

    private void UpdateCooldown()
    {
        //move towards 0
        _cooldown = Mathf.MoveTowards(_cooldown, 0, Time.deltaTime);
        //update the cooldown image
        _cooldownImage.fillAmount = _cooldown / _cooldownMax;
    }
}
