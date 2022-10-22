using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{
    [System.Serializable]
    public struct FencePiece
    {
        public Node leftPost;
        public Node rightPost;
        public SpriteRenderer fence;
    }

    public enum BuildDir
    {
        horizontal,
        vertical
    }

    public FencePiece fencePiece;
    [SerializeField] private BuildDir _buildDir;
    [SerializeField] private LineRenderer _drawLine;
    [SerializeField] private BoxCollider _boxCollider;
    public bool building = true;
    bool completeBuild = false;

    public bool ValidFence { get => ValidateFence(); }

    private void Start()
    {
        building = true;
        fencePiece.fence.enabled = false;
    }

    private void Update()
    {
        if (building)
        {
            _drawLine.SetPosition(0, fencePiece.leftPost.transform.position);
            _drawLine.SetPosition(1, fencePiece.rightPost.transform.position);
            _drawLine.startColor = _drawLine.endColor = ValidateFence() ? Color.green : Color.red;
        }
        else
            if (!completeBuild)
            CompleteBuild();
    }

    private bool ValidateFence()
    {
        return (fencePiece.leftPost.transform.position.x == fencePiece.rightPost.transform.position.x || fencePiece.leftPost.transform.position.y == fencePiece.rightPost.transform.position.y);
    }

    private BuildDir GetFenceDirection()
    {
        if (fencePiece.leftPost.transform.position.x != fencePiece.rightPost.transform.position.x)
            return BuildDir.horizontal;
        else
            return BuildDir.vertical;
    }

    private void CompleteBuild()
    {
        _drawLine.enabled = false;
        fencePiece.fence.enabled = true;

        _buildDir = GetFenceDirection();
        float width = _buildDir == BuildDir.horizontal ? fencePiece.leftPost.transform.position.x - fencePiece.rightPost.transform.position.x : fencePiece.leftPost.transform.position.y - fencePiece.rightPost.transform.position.y;
        float sign = Mathf.Sign(width);
        width += sign;
        Debug.Log(width);
        fencePiece.fence.drawMode = SpriteDrawMode.Tiled;
        fencePiece.fence.size = new Vector2(width, 1f);
        _boxCollider.size = new Vector3(Mathf.Abs(width)-0.5f, 0.5f,1f);
        float xOffsetVal = (width / 2f) - (0.5f * sign);
        float yOffsetVal = (width / 2f) - (0.5f * sign);
        Vector2 offset = new Vector2(_buildDir == BuildDir.horizontal ? -xOffsetVal : 0, _buildDir == BuildDir.vertical ? -yOffsetVal : 0);
        if (_buildDir == BuildDir.vertical)
            fencePiece.fence.transform.localEulerAngles = new Vector3(0, 0, 90);

        fencePiece.fence.transform.localPosition = offset;
        BlockNodes();
        completeBuild = true;
    }

    private void BlockNodes()
    {
        //AStar aStar = GameObject.Find("GameManager").GetComponent<AStar>();
        //List<Node> nodes = aStar.FindShortestPath(fencePiece.leftPost, fencePiece.rightPost);
        Vector3 size = _buildDir == BuildDir.horizontal ? _boxCollider.size / 2f : new Vector3(_boxCollider.size.y, _boxCollider.size.x, _boxCollider.size.z) / 2f;
        Collider[] hits = Physics.OverlapBox(transform.position + fencePiece.fence.transform.localPosition + _boxCollider.center, size);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Node>(out Node n))
            n.SetDisabled(true);
        }
        RabbitSpawn.RepathRabbits();
    }

}
