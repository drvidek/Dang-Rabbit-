using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField] private GameObject _stump;
    [SerializeField] private GameObject _fence;
    private Fence _currentFence;
    private bool _rightClickHeld;

    // Update is called once per frame
    void Update()
    {
        if (!_rightClickHeld)
        {
            if (!CheckLeftClick())
                _rightClickHeld = CheckRightClick();
        }
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
            }
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
                PlaceObject(_stump, n.transform.position);
                n.SetDisabled(true);
                GameManager.RepathRabbits();
            }
            return true;
        }
        else return false;
    }

    bool CheckRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Buildable(out Node n))
            {
                _currentFence = PlaceObject(_fence, n.transform.position).GetComponent<Fence>();
                _currentFence.fencePiece.leftPost = n;
                _currentFence.fencePiece.rightPost = n;
            }
            return true;
        }
        else return false;
    }

    bool PlacingFence()
    {
        if (Input.GetMouseButton(1))
        {
            if (Buildable(out Node n))
            {
                _currentFence.fencePiece.rightPost = n;
            }
            return true;
        }
        else
        {
            _rightClickHeld = false;
            return false;
        }
    }
}
