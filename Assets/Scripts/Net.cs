using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    private Vector3 _homePos;
    [SerializeField] private float _spd;
    [SerializeField] private float _minDist = 0.1f;
    private Vector3 _dir;
    protected Rabbit _target;

    private void Start()
    {
        Initialise();
    }

    private void Initialise()
    {
        _homePos = transform.position;
    }

    public void SetTarget(Rabbit rabbit, Vector3 pos)
    {
        _target = rabbit;
        _dir = MathExt.Direction(transform.position, pos);
    }

    void Update()
    {
        transform.position += _dir * _spd * Time.deltaTime;
        if (Vector3.Distance(transform.position, _homePos) < _minDist && _target == null)
        {
            _dir = Vector3.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            if (rabbit == _target)
            {
                SetTarget(null, _homePos);
                Destroy(rabbit.gameObject);
            }
        }
    }
}
