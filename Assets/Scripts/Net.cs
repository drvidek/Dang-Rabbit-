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
    [SerializeField] private float _maxDist = 2f;
    private LineRenderer _line;
    private SpriteRenderer _sprite;
    public bool NetFree { get => _target == null && transform.position == _homePos; }

    private void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        _line ??= GetComponentInChildren<LineRenderer>();
        _sprite ??= GetComponentInChildren<SpriteRenderer>();
        _homePos = transform.position;
        _line.SetPosition(0, _homePos);
        _line.SetPosition(1, _homePos);
    }

    public void SetTarget(Rabbit rabbit, Vector3 pos)
    {
        _target = rabbit;
        _dir = MathExt.Direction(transform.position, pos);
        _dir.z = 0;
    }

    void Update()
    {
        transform.position += _dir * _spd * Time.deltaTime;
        if (Vector3.Distance(transform.position, _homePos) < _minDist && _target == null)
        {
            _dir = Vector3.zero;
            transform.position = _homePos;
        }
        if (Vector3.Distance(transform.position, _homePos) > _maxDist)
        {
            SetTarget(null, _homePos);
        }
        _line.SetPosition(1, transform.position);

        _sprite.enabled = (transform.position != _homePos);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Rabbit>(out Rabbit rabbit))
        {
            if (rabbit == _target)
            {
                SetTarget(null, _homePos);
                GameManager.Singleton.ChangeScore(5);
                rabbit.EndOfLife();
            }
        }
    }

}
