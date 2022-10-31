using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSpawn : MonoBehaviour
{
    [SerializeField] private GameObject _rabbitPrefab;//our rabbit prefab
    [SerializeField] private float _spawnTimerMax = 1f;//our max spawn timer
    [SerializeField] private Node _startNode;//our start node
    [SerializeField] private Node _endNode;//our end node
    [SerializeField] private Node[] _path;//our path to the end
    [SerializeField] private AStarGlobal _pathFinder;//our pathfinder
    private float _spawnTimer;//our current spawn timer
    private static List<Rabbit> _rabbits = new List<Rabbit>();//our list of rabbits
    [SerializeField] private GameObject _mound;//the mound object
    private float _spawnTimerMod = 1f;//the amount to modify spawn timer by
    private float _spawnTimerDecay = 1f;//the amount to reduce spawn timer by
    private int _rabbitCount = 0;//current rabbit count
    private int _rabbitCountMax = 20;//amount before spawn decay decreases

    void Start()
    {
        //establish our start, end, and our path to the exit
        _startNode = GetComponent<Node>();
        _endNode = GameObject.Find("End").GetComponent<Node>();
        _spawnTimer = _spawnTimerMax;
        _path = _pathFinder.FindShortestPath(_startNode, _endNode).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        //if our path is impacted, repath and relocate
        if (PathImpacted())
        {
            RelocateTriggered();
            return;
        }

        //if our spawn timer has reached 0
        if (_spawnTimer <= 0)
        {
            //spawn a rabbit
            Rabbit rabbit = Instantiate(_rabbitPrefab, _startNode.transform.position, Quaternion.identity, null).GetComponent<Rabbit>();
            rabbit.Initialise(_startNode, _endNode, this, _path);
            _rabbits.Add(rabbit);
            //update spawn decay if required
            ManageSpawnDecay();
            //reset the spawntimer
            _spawnTimer = _spawnTimerMax * _spawnTimerMod;
            //adjust the next spawn timer by a random value and the decay
            _spawnTimerMod = Random.Range(0.6f, 1f) * _spawnTimerDecay;
        }
        else //else count down to 0
            _spawnTimer -= Time.deltaTime;
    }

    private void ManageSpawnDecay()
    {
        //increase the rabbit count
        _rabbitCount++;
        //if we go past max
        if (_rabbitCount > _rabbitCountMax)
        {
            //decrease the decay value
            _spawnTimerDecay = Mathf.MoveTowards(_spawnTimerDecay, 0.5f, 0.05f);
            //increase the next threshold for decay
            _rabbitCountMax = Mathf.RoundToInt((float)_rabbitCountMax * 1.2f);
            //reset the count
            _rabbitCount = 0;
        }
    }

    public void RemoveRabbit(Rabbit rabbit)
    {
        //remove the rabbit if currently on your list
        if (_rabbits.Contains(rabbit))
        {
            _rabbits.Remove(rabbit);
        }
    }

    void RelocateTriggered()
    {
        //find the shortest path
        List<Node> nodes = _pathFinder.FindShortestPath(_startNode, _endNode);
        //if the path is valid, repath and exit
        if (nodes != null && nodes.Count > 1)
        {
            _path = nodes.ToArray();
            return;
        }

        //if no path is found, repath again but ignore disabled nodes
        nodes = _pathFinder.FindShortestPath(_startNode, _endNode, false);
        //loop backwards through nodes until you find a disabled node
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i].Disabled)
            {
                //set the start point to the node after this node
                Node newNode = nodes[i + 1];
                _startNode = newNode;
                //clip the list until that node
                nodes.RemoveRange(0, i);
                //update the path to this new path
                _path = nodes.ToArray();
                //place a mound in the new location
                Instantiate(_mound, _startNode.transform);
                return;
            }
        }
    }

    public void RabbitRequestRelocate(Rabbit rabbit)
    {
        //relocate the rabbit using your start node
        rabbit.Initialise(_startNode, _endNode, this, _path);
    }

    public static void RepathRabbits()
    {
        //set all rabbits to check for repath
        foreach (Rabbit rabbit in _rabbits)
        {
            rabbit.RepathPending = true;
        }
    }

    public bool PathImpacted()
    {
        //check your node list for any disabled nodes
        for (int i = 0; i < _path.Length; i++)
        {
            if (_path[i].Disabled)
            {
                return true;
            }
        }
        return false;
    }
}
