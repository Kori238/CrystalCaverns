using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TreeEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMove : MonoBehaviour
{
    private GridBasedBehaviours _gridBasedBehaviours;

    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    [SerializeField] private int _eyesLayer, _chestLayer;
    [SerializeField] private Path _path = null;
    [SerializeField] private int _pathIndex = 1;
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private Vector2Int _position;
    [SerializeField] private int _currentLayer = 0;
    private static readonly Vector2 LayerMultiplier = new Vector2(0, PublicValues.CellHeight);


    void Awake()
    {
        _gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
    }

    void Start()
    {
        Tilemap tilemap;
        Vector3Int cell;
        var destination = Singleton.Instance.playerPortalDestination;

        if (destination != new Vector3Int(0, 0, 0))
        {
            tilemap = _gridBasedBehaviours.Grids[destination.z].GetTilemap();
            _currentLayer = destination.z;
            transform.position = tilemap.GetCellCenterWorld(new Vector3Int(destination.x, destination.y, 0));
            cell = tilemap.WorldToCell(transform.position);
        }
        else
        {
            tilemap = _gridBasedBehaviours.Grids[_currentLayer].GetTilemap();
            cell = tilemap.WorldToCell(transform.position);
        }
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        _chestLayer = Mathf.RoundToInt((_chest.position - _feet.position / PublicValues.CellHeight).y);
        _eyesLayer = Mathf.RoundToInt((_eyes.position - _feet.position / PublicValues.CellHeight).y);
        _playerSprite.sortingOrder = _currentLayer + 1;
#pragma warning disable CS4014
        TurnLoop();
#pragma warning restore CS4014
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
           UpdateCurrentPath();
        }
        
    }

    private async Task TurnLoop()
    {
        while (true)
        {
            if (_path == null)
            {
                await Task.Yield();
                continue;
            }

            await MoveNext();
            List<Task> enemiesTurns = new();
            foreach (var enemy in _gridBasedBehaviours.EnemyMoves)
            {
                enemy.PlayerPos = _position;
                enemy.PlayerLayer = _currentLayer;
                enemy.PlayerTransform = transform;
                enemy.PlayerChest = _chest;
                enemy.PlayerEyes = _eyes;
                enemy.PlayerFeet = _feet;
                enemy.PlayerEyesLayer = _eyesLayer;
                enemy.PlayerChestLayer = _chestLayer;
                enemiesTurns.Add(enemy.MoveNext());
            }
            await Task.WhenAll(enemiesTurns);
            //await Task.Delay(100);
        }
    }

    private async Task MoveNext()
    {
        await Task.Delay(100);
        if (_path == null) return;
        var node = _path.Nodes[_pathIndex];
        var previousNode = _path.Nodes[_pathIndex - 1];
        await MoveToCell(node, previousNode);
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            return;
        }
        _pathIndex++;
    }

    public async Task MoveToCell(Node node, Node previousNode)
    {
        previousNode.PlayerExitedTile?.Invoke();
        previousNode.CharacterExitedTile?.Invoke();
        var direction = node.Center - transform.position;
        _position = new Vector2Int(node.X, node.Y);
        _currentLayer = node.Z;
        _playerSprite.sortingOrder = _currentLayer + 1;
        while (Vector2.Distance(transform.position, node.Center) > 0.01)
        {
            transform.position += direction * Time.deltaTime * movementSpeed;
            await Task.Yield();
        }
        node.PlayerEnteredTile?.Invoke();
        node.CharacterEnteredTile?.Invoke();
    }


    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        NodeGrid previousLayer = null;
        Node previousNode = null;
        var currentLayer = _gridBasedBehaviours.Grids[_currentLayer];
        var currentPos = (Vector3Int)_position;
        var currentNode = currentLayer.GetNodeFromCell((int)currentPos.x, (int)currentPos.y);
        
        foreach (var layer in _gridBasedBehaviours.ReversedGrids)
        {
            Tilemap tilemap = layer.GetTilemap();
            int.TryParse(tilemap.name, out var newLayer);
            var layerVector = LayerMultiplier * newLayer;
            var cellPos = tilemap.WorldToCell(mousePointInWorld - layerVector);
            var selectedNode = layer.GetNodeFromCell(cellPos.x, cellPos.y);
            if (selectedNode == currentNode) return;
            if (selectedNode.HasTile && (previousLayer == null || !previousNode.HasTile))
            {
                var tile = selectedNode.Tile;
                if (!tile.Walkable) continue;
                //Debug.Log($"x:{currentPos.x}, y:{currentPos.y}, z:{_currentLayer}   target x:{selectedNode.X}, y:{selectedNode.Y}, z:{selectedNode.Z}");
                var path = _gridBasedBehaviours.Pathfinding.FindPath(currentPos.x, currentPos.y, _currentLayer,
                    selectedNode.X, selectedNode.Y, selectedNode.Z);
                if (path != null)
                {
                    _path = path;
                    _pathIndex = 1;
                    Node prevNode = null;
                    foreach (var node in _path.Nodes)
                    {
                        if (prevNode != null) Debug.DrawLine(node.Center + new Vector3(0, 0.3f, 0), 
                            prevNode.Center + new Vector3(0, 0.3f, 0), Color.black, _path.Nodes.Count);
                        prevNode = node;
                    }
                }
                break;
            }

            previousLayer = layer;
            previousNode = selectedNode;
        }
    }
}
