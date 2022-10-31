using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField] private GameObject _fence; //our fence prefab
    [SerializeField] private Fence _currentFence;   //the fence we are currently building
    [SerializeField] private bool _leftClickHeld;   //if we are holding left click
    [SerializeField] private GameObject[] _builderOptions;  //our array of build options
    private int _builderIndex = -1; //our index for our build options

    // Update is called once per frame
    void Update()
    {
        //if the game is over, do nothing
        if (!GameManager.Singleton.IsPlaying)
            return;

        //if we have a valid build option selected
        if (_builderIndex > -1)
        {
            //if we're not already holding, check for an input
            if (!_leftClickHeld)
                _leftClickHeld = CheckLeftClick();
            //if we're building a fence
            if (_currentFence != null)
            {
                //if we release the fence
                if (!PlacingFence())
                {
                    //if the fence is valid, build it
                    if (_currentFence.ValidFence)
                    {
                        _currentFence.building = false;
                        _currentFence = null;
                    }
                    else    //otherwise destroy it
                    {
                        Destroy(_currentFence.gameObject);
                        _currentFence = null;
                    }
                    _builderIndex = -1;
                }
            }
            //catch in case we fail to release our left click held bool
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
        //check the mouse position and return hit success + details
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitObj);
    }

    bool Buildable(out Node node)
    {
        //establish our node
        node = null;
        //if we have a successful click
        if (CheckClickedPosition(out RaycastHit hit))
        {
            //if that click found a node
            if (hit.collider.TryGetComponent<Node>(out Node n))
            {
                //set our out node to that node
                node = n;
                //if it's not disbaled and free for building, return true
                if (!n.Disabled && n.Buildable)
                    return true;
            }
        }
        return false;
    }

    bool CheckLeftClick()
    {
        //if we left click
        if (Input.GetMouseButtonDown(0))
        {
            //if we find a buildable node
            if (Buildable(out Node n))
            {
                //if we have a fence selected
                if (_builderIndex == 0)
                {
                    //place a new fence at the node
                    _currentFence = PlaceObject(_fence, n.transform.position).GetComponent<Fence>();
                    _currentFence.fencePiece.leftPost = n;
                    _currentFence.fencePiece.rightPost = n;
                }
                else
                {
                    //repath the farmer selected
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
        //if we're still holding left click
        if (Input.GetMouseButton(0))
        {
            //if the current node we're hovering over is valid
            if (Buildable(out Node n))
            {
                //update the fence position
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
        //get the farmer component of the selected farmer
        Farmer farmer = _builderOptions[farmerIndex].GetComponent<Farmer>();
        //change its destination
        farmer.SetDestination(n);
        _builderIndex = -1;
    }

    public void SetBuilderMode(int i)
    {
        //update the builder index to the selection
        _builderIndex = i;
    }
}
