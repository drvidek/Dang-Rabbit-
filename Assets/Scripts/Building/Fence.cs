using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{
    #region Variables
    [System.Serializable]
    public struct FencePiece
    {
        public Node leftPost;   //the location of the starting post
        public Node rightPost;  //the location of the end post
        public SpriteRenderer fence;    //the component which draws the fence
    }

    public enum BuildDir
    {
        horizontal,
        vertical
    }

    public FencePiece fencePiece;
    [SerializeField] private BuildDir _buildDir;    //to determine which direction the fence is facing
    [SerializeField] private LineRenderer _drawLine;    //the line renderer for building
    [SerializeField] private BoxCollider _boxCollider;  //the collider of the fence
    public bool building = true;    //are we currently building
    bool completeBuild = false; //did we complete the build
    public int cost = 30;   //how much is a fence per piece
    #endregion

    public bool ValidFence { get => ValidateFence(); }  //is the fence valid

    private void Start()
    {
        //when we get created, we are building and the renderer should be turned off
        building = true;
        fencePiece.fence.enabled = false;
    }

    private void Update()
    {
        //when we are still building
        if (building)
        {
            //draw the line between the start and end points and change the colour based on validity
            _drawLine.SetPosition(0, fencePiece.leftPost.transform.position);
            _drawLine.SetPosition(1, fencePiece.rightPost.transform.position);
            _drawLine.startColor = _drawLine.endColor = ValidateFence() ? Color.green : Color.red;
            _buildDir = GetFenceDirection();
        }
        else
            if (!completeBuild) //if we stop building and we haven't completed the build
            CompleteBuild();    //complete the build
    }

    private bool ValidateFence()
    {
        //check if the fence is aligned on the X or Y axis, and we have enough money to make it
        return (fencePiece.leftPost.transform.position.x == fencePiece.rightPost.transform.position.x || fencePiece.leftPost.transform.position.y == fencePiece.rightPost.transform.position.y) && (GameManager.Singleton.CheckMoney(cost * (int)Mathf.Abs(GetFenceWidth())));
    }

    private float GetFenceWidth()
    {
        //get the length based on the distance between the two points
        float width = _buildDir == BuildDir.horizontal ? fencePiece.leftPost.transform.position.x - fencePiece.rightPost.transform.position.x : fencePiece.leftPost.transform.position.y - fencePiece.rightPost.transform.position.y;
        float sign = Mathf.Sign(width);
        width += sign;
        return width;
    }

    private BuildDir GetFenceDirection()
    {
        //get the direction based on whether your x coordinates line up
        if (fencePiece.leftPost.transform.position.x != fencePiece.rightPost.transform.position.x || fencePiece.leftPost.transform.position == fencePiece.rightPost.transform.position)
            return BuildDir.horizontal;
        else
            return BuildDir.vertical;
    }

    private void CompleteBuild()
    {
        //stop drawing the preview line
        _drawLine.enabled = false;
        //enable the sprite renderer
        fencePiece.fence.enabled = true;
        //get the width
        float width = GetFenceWidth();
        //get the direction
        float sign = Mathf.Sign(width);
        //set up the sprite renderer based on the fence size
        fencePiece.fence.drawMode = SpriteDrawMode.Tiled;
        fencePiece.fence.size = new Vector2(width, 1f);
        //set the collider based on the fence size
        _boxCollider.size = new Vector3(Mathf.Abs(width)-0.5f, 0.5f,1f);
        //determine the centre of the fence by using the midpoint
        float xOffsetVal = (width / 2f) - (0.5f * sign);
        float yOffsetVal = (width / 2f) - (0.5f * sign);
        Vector2 offset = new Vector2(_buildDir == BuildDir.horizontal ? -xOffsetVal : 0, _buildDir == BuildDir.vertical ? -yOffsetVal : 0);
        //rotate the fence based on build direction
        if (_buildDir == BuildDir.vertical)
            fencePiece.fence.transform.localEulerAngles = new Vector3(0, 0, 90);
        //place the fence
        fencePiece.fence.transform.localPosition = offset;
        //check nodes to be blocked
        BlockNodes();
        //repath any impacted rabbits
        RabbitSpawn.RepathRabbits();
        //charge money based on length
        GameManager.Singleton.ChangeMoney(-cost * Mathf.Abs((int)width));
        //complete the build
        completeBuild = true;
    }

    private void BlockNodes()
    {
        //get the area to disable based on build length
        Vector3 size = _buildDir == BuildDir.horizontal ? _boxCollider.size / 2f : new Vector3(_boxCollider.size.y, _boxCollider.size.x, _boxCollider.size.z) / 2f;
        //get a list of the colliders within that area
        Collider[] hits = Physics.OverlapBox(transform.position + fencePiece.fence.transform.localPosition + _boxCollider.center, size);

        //if the hit colliders contain a node, disable the node
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Node>(out Node n))
            n.SetDisabled(true);
        }
    }

}
