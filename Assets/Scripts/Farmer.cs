using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmer : MonoBehaviour
{
    [SerializeField] private float _cooldown, _cooldownMax;
    [SerializeField] private List<Rabbit> _targetList = new List<Rabbit>();
    [SerializeField] private Net _net;
    private Node _end;

    private void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        _cooldown = _cooldownMax;
        _end = GameObject.Find("End").GetComponent<Node>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_cooldown == 0)
        {
            if (_targetList.Count > 0)
            {
                Rabbit nearest = null;
                float dist = float.PositiveInfinity;
                foreach (Rabbit rabbit in _targetList)
                {
                    float newDist = Vector3.Distance(rabbit.transform.position, _end.transform.position);
                    if (nearest == null || dist > newDist)
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
