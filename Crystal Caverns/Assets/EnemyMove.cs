using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Mono.Cecil;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMove : MonoBehaviour, IMoveNext
{
    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    [SerializeField] private int _eyesLayer, _chestLayer;
    public int PlayerEyesLayer, PlayerChestLayer;
    [SerializeField] private const int CONFUSION_MAX = 3;
    [SerializeField] private int _confusion;
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

    public IEnumerator MoveRandom()
    {
        List<Adjacents> adjacentsList;
        Node currentNode = Singleton.Instance.Grids[_layer].GetNodeFromCell(_position.x, _position.y);
        if (currentNode.Tile.LayerTraversable)
        {
            adjacentsList =
                Singleton.Instance.Pathfinding.FindAdjacentsOnLayerTraversalTile(currentNode.X, currentNode.Y,
                    currentNode.Z);
        }
        else
        {
            adjacentsList = new List<Adjacents>
                { Singleton.Instance.Pathfinding.FindAdjacents(currentNode.X, currentNode.Y, currentNode.Z) };
        }
        List<Node> possibleNodes = new();
        foreach (var adjacents in adjacentsList)
        {
            var allAdjacent = adjacents.SameLayer.Concat(adjacents.LayerTraversalUp)
                .Concat(adjacents.LayerTraversalDown).ToList();
            foreach (var adjacentNode in allAdjacent)
            {
                if (adjacentNode.HasTile && adjacentNode.Tile.Walkable && !possibleNodes.Contains(adjacentNode))
                {
                    possibleNodes.Add(adjacentNode);
                }
            }
            var nodeIndex = Random.Range(0, possibleNodes.Count);
            var targetNode = possibleNodes[nodeIndex];
            transform.position = targetNode.Center;
            _position = new Vector2Int(targetNode.X, targetNode.Y);
            _layer = targetNode.Z;
            _sprite.sortingOrder = _layer + 1;
        }
        yield return null;
    }

    public IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        bool los = CheckLOS();
        if (Vector3.Distance(transform.position, PlayerTransform.position) < VIEW_RADIUS && los) GetPath();
        Debug.Log(los);
        if (_path == null || _path.Nodes.Count <= 1)
        {
            if (_confusion > 0)
            {
                _confusion--;
                yield break;
            }
            yield return MoveRandom();
            yield break;
        }
        var node = _path.Nodes[_pathIndex];
        transform.position = node.Center;
        _position = new Vector2Int(node.X, node.Y);
        _layer = node.Z;
        _sprite.sortingOrder = _layer + 1;
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _confusion = CONFUSION_MAX;
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
