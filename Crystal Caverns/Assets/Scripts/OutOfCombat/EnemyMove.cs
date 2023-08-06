using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Mono.Cecil;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    private GridBasedBehaviours _gridBasedBehaviours;

    [SerializeField] private int _eyesLayer, _chestLayer;
    public int PlayerEyesLayer, PlayerChestLayer;
    [SerializeField] private const int CONFUSION_MAX = 3;
    [SerializeField] private int _confusion;
    [SerializeField] private float _movementSpeed = 1;
    private Path _path;
    private int _pathIndex = 1;
    [SerializeField] private const float VIEW_RADIUS = 1000f;
    private Vector2Int _position;
    [SerializeField] private int _layer = 0;
    public Transform PlayerTransform, PlayerChest, PlayerEyes, PlayerFeet;
    public Vector2Int PlayerPos;
    public int PlayerLayer;

    void Awake()
    {
        _gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
    }

    void Start()
    {
        Invoke("AddSelfToSingleton", Time.fixedDeltaTime);
        var tilemap = _gridBasedBehaviours.Tilemaps[_layer];
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        _sprite.sortingOrder = _layer + 1;
    }

    public void AddSelfToSingleton()
    {
        _gridBasedBehaviours.EnemyMoves.Add(this);
    }

    public async Task MoveRandom()
    {
        List<Adjacents> adjacentsList;
        Node currentNode = _gridBasedBehaviours.Grids[_layer].GetNodeFromCell(_position.x, _position.y);
        var dict = Singleton.Instance.TileDictionary;
        if (dict.GetValueOrDefault(currentNode.TileName).LayerTraversable)
        {
            adjacentsList =
                _gridBasedBehaviours.Pathfinding.FindAdjacentsOnLayerTraversalTile(currentNode.X, currentNode.Y,
                    currentNode.Z);
        }
        else
        {
            adjacentsList = new List<Adjacents>
                { _gridBasedBehaviours.Pathfinding.FindAdjacents(currentNode.X, currentNode.Y, currentNode.Z) };
        }
        List<Node> possibleNodes = new();
        foreach (var adjacents in adjacentsList)
        {
            var allAdjacent = adjacents.SameLayer.Concat(adjacents.LayerTraversalUp)
                .Concat(adjacents.LayerTraversalDown).ToList();
            foreach (var adjacentNode in allAdjacent)
            {
                if (adjacentNode.HasTile && dict.GetValueOrDefault(adjacentNode.TileName).Walkable && !possibleNodes.Contains(adjacentNode))
                {
                    possibleNodes.Add(adjacentNode);
                }
            }
            var nodeIndex = Random.Range(0, possibleNodes.Count);
            var targetNode = possibleNodes[nodeIndex];
            await MoveToCell(targetNode);
        }
    }

    public async Task MoveNext()
    {
        await Task.Delay(100);
        var los = CheckLOS();
        if (Vector3.Distance(transform.position, PlayerTransform.position) < VIEW_RADIUS && los) GetPath();
        if (_path == null || _path.Nodes.Count <= 1)
        {
            if (_confusion > 0)
            {
                _confusion--;
                return;
            } 
            await MoveRandom();
            return;
        }
        var node = _path.Nodes[_pathIndex];
        await MoveToCell(node);
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _confusion = CONFUSION_MAX;
            _path = null;
            _pathIndex = 1;
            return;
        }
        _pathIndex++;
    }

    public async Task MoveToCell(Node node)
    {
        var direction = node.Center - transform.position;
        _position = new Vector2Int(node.X, node.Y);
        _layer = node.Z;
        _sprite.sortingOrder = _layer + 1;
        while (Vector2.Distance(transform.position, node.Center) > 0.01)
        {
            transform.position += direction * Time.deltaTime * _movementSpeed;
            await Task.Yield();
        }
    }

    public bool CheckLOS()
    {
        return _gridBasedBehaviours.LOS.HasLineOfSight(_position.x, _position.y, _eyesLayer + _layer, PlayerPos.x, PlayerPos.y, PlayerLayer + 1);
    }

    public void GetPath()
    {
        var path = _gridBasedBehaviours.Pathfinding.FindPath(_position.x, _position.y, _layer, PlayerPos.x, PlayerPos.y, PlayerLayer);
        if (path == null) return;
        _path = path;
        _pathIndex = 1;
    }
}

public interface IMoveNext
{
    public IEnumerator MoveNext();
}
