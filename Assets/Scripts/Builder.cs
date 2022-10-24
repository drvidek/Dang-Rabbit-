using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField] private GameObject _stump;
    [SerializeField] private GameObject _fence;
    [SerializeField] private Fence _currentFence;
    [SerializeField] private bool _leftClickHeld;
    [SerializeField] private GameObject[] _builderOptions;
    private int _builderIndex = -1;

    // Update is called once per frame
    void Update()
    {
        if (_builderIndex > -1)
        {
            if (!_leftClickHeld)
                _leftClickHeld = CheckLeftClick();
            if (_currentFence != null)
            {
                if (!PlacingFence())
                {
                    if (_currentFence.ValidFence)
                    {
                        _currentFence.building = false;
                        _currentFence = null;
                    }
                    else
                    {
                        Destroy(_currentFence.gameObject);
                        _currentFence = null;
                    }
                    _builderIndex = -1;
                }
            }
            if (_leftClickHeld && !Input.GetMouseButton(0))
                _leftClickHeld = false;
        }
    }

    GameObject PlaceObject(GameObject prefab, Vector3 pos)
    {
        return Instantiate(prefab, pos, Quaternion.identity, null);
    }

    bool CheckClickedPosition(out RaycastHit hitObj)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitObj);
    }

    bool Buildable(out Node node)
    {
        node = null;
        if (CheckClickedPosition(out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Node>(out Node n))
            {
                node = n;
                if (!n.Disabled && n.Buildable)
                    return true;
            }
        }
        return false;
    }

    bool CheckLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Buildable(out Node n))
            {
                if (_builderIndex == 0)
                {
                    _currentFence = PlaceObject(_fence, n.transform.position).GetComponent<Fence>();
                    _currentFence.fencePiece.leftPost = n;
                    _currentFence.fencePiece.rightPost = n;
                }
                else
                {
                    RepathFarmer(_builderIndex,n);
                    _leftClickHeld = false;
                }
            }
            return true;
        }
        else return false;
    }

    bool PlacingFence()
    {
        if (Input.GetMouseButton(0))
        {
            if (Buildable(out Node n))
            {
                _currentFence.fencePiece.rightPost = n;
            }
            return true;
        }
        else
        {
            _leftClickHeld = false;
            return false;
        }
    }

    void RepathFarmer(int farmerIndex, Node n)
    {
        Farmer farmer = _builderOptions[farmerIndex].GetComponent<Farmer>();
        farmer.SetDestination(n);
        _builderIndex = -1;
    }

    public void SetBuilderMode(int i)
    {
        _builderIndex = i;
    }
}
