using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    private Vector3 _homePos; //to store our home position
    [SerializeField] private float _spd;    //our movement speed
    [SerializeField] private float _minDist = 0.1f; //minimum distance before we have "reached" our target
    private Vector3 _dir;   //movement direction
    protected Rabbit _target;   //targeted rabbit
    [SerializeField] private float _maxDist = 2f;   //longest distance we can travel before returning
    private LineRenderer _line; //my line renderer
    private SpriteRenderer _sprite; //my sprite
    public bool NetFree { get => _target == null && transform.position == _homePos; } //if the net is currently home and not targeting a rabbit

    private void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        //get components if null
        _line ??= GetComponentInChildren<LineRenderer>();
        _sprite ??= GetComponentInChildren<SpriteRenderer>();
        //set the home position to our current position
        _homePos = transform.position;
        //set the line points to our current position
        _line.SetPosition(0, _homePos);
        _line.SetPosition(1, _homePos);
    }

    public void SetTarget(Rabbit rabbit, Vector3 pos)
    {
        //set the rabbit to our target
        _target = rabbit;
        //update the direction towards the rabbit
        _dir = MathExt.Direction(transform.position, pos);
        //keep things on a 2D axis
        _dir.z = 0;
    }

    void Update()
    {
        //move based on your direction and speed
        transform.position += _dir * _spd * Time.deltaTime;
        //if you have reached home and your target is null
        if (Vector3.Distance(transform.position, _homePos) < _minDist && _target == null)
        {
            //set direction to 0 and snap to home position
            _dir = Vector3.zero;
            transform.position = _homePos;
        }
        //if you exceed max distance from home
        if (Vector3.Distance(transform.position, _homePos) > _maxDist)
        {
            //remove target and move home
            SetTarget(null, _homePos);
        }
        //update the end line point to your current position
        _line.SetPosition(1, transform.position);
        //disable the sprite if we are home
        _sprite.enabled = (transform.position != _homePos);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if we collide with a rabbit
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            //if that rabbit is our target
            if (rabbit == _target)
            {
                //return home
                SetTarget(null, _homePos);
                //increase money and rabbits caught
                GameManager.Singleton.ChangeMoney(GameManager.Singleton.RabbitWorth);
                GameManager.Singleton.ChangePoints(1);
                //terminate the rabbit
                rabbit.EndOfLife();
            }
        }
    }

}
