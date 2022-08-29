using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSpawn : MonoBehaviour
{
    [SerializeField] private GameObject _rabbitPrefab;
    [SerializeField] private float _spawnTimerMax = 1f;
    [SerializeField] private Node _startNode;
    [SerializeField] private Node _endNode;
    private float _spawnTimer;
    
    void Start()
    {
        _startNode = GetComponent<Node>();
        _endNode = GameObject.Find("End").GetComponent<Node>();
        _spawnTimer = _spawnTimerMax;
    }

    // Update is called once per frame
    void Update()
    {
        if (_spawnTimer <= 0)
        {
            Rabbit rabbit = Instantiate(_rabbitPrefab, transform.position, Quaternion.identity, null).GetComponent<Rabbit>();
            rabbit.Initialise(_startNode, _endNode);
            _spawnTimer = _spawnTimerMax;
        }
        else
        _spawnTimer -= Time.deltaTime;
    }
}
