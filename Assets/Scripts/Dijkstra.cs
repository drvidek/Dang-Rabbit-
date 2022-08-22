using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    private GameObject[] _nodes;

    public Node start;
    public Node end;

    public List<Node> FindShortestPath(Node start, Node end)
    {
        _nodes = GameObject.FindGameObjectsWithTag("Node");

        if (DijkstraAlgorithm(start, end))
        {
            List<Node> result = new List<Node>();

            Node current = end;

            do
            {
                result.Insert(0, current);
                current = current.PreviousNode;
            }
            while (current != null);

            return result;
        }
        return null;
    }

    private bool DijkstraAlgorithm(Node start, Node end)
    {
        List<Node> unexplored = new List<Node>();

        foreach (GameObject obj in _nodes)
        {
            if (obj.TryGetComponent<Node>(out Node n))
            {
                n.ResetNode();
                unexplored.Add(n);
            }
        }

        if (!unexplored.Contains(start) && !unexplored.Contains(end))
        {
            return false;
        }

        start.PathWeight = 0;
        while (unexplored.Count > 0)
        {
            //order based on path
            unexplored.Sort(
                (x, y) => x.PathWeight.CompareTo(y.PathWeight)
                );

            //current is the currently shortest path possible
            Node current = unexplored[0];

            if (current == end)
                break;

            unexplored.RemoveAt(0);

            foreach (Node neighbour in current.NeighbourNodes)
            {
                if (!unexplored.Contains(neighbour))
                    continue;

                float distance = Vector3.Distance(current.transform.position, neighbour.transform.position);
                distance += current.PathWeight;

                if (distance < neighbour.PathWeight)
                {
                    neighbour.PathWeight = distance;
                    neighbour.PreviousNode = current;
                }
            }
        }

        return true;
    }
}