using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour, IMoveNext
{
    private Path _path;
    [SerializeField] private SpriteRenderer _sprite;
    private int _pathIndex = 1;
    private Vector2Int _position = new Vector2Int();
    private int _layer = 0;
    public Vector2Int PlayerPos;
    public int PlayerLayer;

    void Start()
    {
        Singleton.Instance.EnemyMoves.Add(this);
        var tilemap = Singleton.Instance.Grids[_layer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
    }

    public IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        GetPath();
        if (_path == null || _path.nodes.Count <= 1) yield break;
        var node = _path.nodes[_pathIndex];
        transform.position = node.Center;
        _position = new Vector2Int(node.x, node.y);
        _layer = node.z;
        _sprite.sortingOrder = _layer + 1;
        if (_pathIndex == _path.nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            yield break;
        }
        _pathIndex++;
        yield return null;
    }

    public void GetPath()
    {
        Path path = Singleton.Instance.Pathfinding.FindPath(_position.x, _position.y, _layer, PlayerPos.x, PlayerPos.y, PlayerLayer);
        if (path == null) return;
        _path = path;
        _pathIndex = 1;
    }
}

public interface IMoveNext
{
    public IEnumerator MoveNext();
}
