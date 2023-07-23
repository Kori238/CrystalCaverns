using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyMove : MonoBehaviour, IMoveNext
{
    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    [SerializeField] private int _eyesLayer, _chestLayer;
    public int PlayerEyesLayer, PlayerChestLayer;
    private Path _path;
    private int _pathIndex = 1;
    [SerializeField] private const float VIEW_RADIUS = 1000f;
    private Vector2Int _position = new();
    [SerializeField] private int _layer = 0;
    public Transform PlayerTransform, PlayerChest, PlayerEyes, PlayerFeet;
    public Vector2Int PlayerPos;
    public int PlayerLayer;

    void Start()
    {
        Singleton.Instance.EnemyMoves.Add(this);
        var tilemap = Singleton.Instance.Grids[_layer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        //_chestLayer = Mathf.RoundToInt((_chest.position - _feet.position / PublicValues.CellHeight).y);
        //_eyesLayer = Mathf.RoundToInt((_eyes.position - _feet.position / PublicValues.CellHeight).y);
        _sprite.sortingOrder = _layer + 1;
    }


    public IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        bool los = CheckLOS();
        if (Vector3.Distance(transform.position, PlayerTransform.position) < VIEW_RADIUS && los) GetPath();
        Debug.Log(los);
        if (_path == null || _path.Nodes.Count <= 1) yield break;
        var node = _path.Nodes[_pathIndex];
        transform.position = node.Center;
        _position = new Vector2Int(node.X, node.Y);
        _layer = node.Z;
        _sprite.sortingOrder = _layer + 1;
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            yield break;
        }
        _pathIndex++;
        yield return null;
    }

    public bool CheckLOS()
    {
        return Singleton.Instance.LOS.HasLineOfSight(_position.x, _position.y, _eyesLayer + _layer, PlayerPos.x, PlayerPos.y, PlayerLayer + 1);
        //return false;
    }

    public void GetPath()
    {
        var path = Singleton.Instance.Pathfinding.FindPath(_position.x, _position.y, _layer, PlayerPos.x, PlayerPos.y, PlayerLayer);
        if (path == null) return;
        _path = path;
        _pathIndex = 1;
    }
}

public interface IMoveNext
{
    public IEnumerator MoveNext();
}
