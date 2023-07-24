using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMove : MonoBehaviour
{
    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    [SerializeField] private int _eyesLayer, _chestLayer;
    [SerializeField] private Path _path = null;
    [SerializeField] private int _pathIndex = 1;
    [SerializeField] private float movementSpeed = 1;
    private Vector2Int _position;
    [SerializeField] private int _currentLayer = 0;
    private static readonly Vector2 LayerMultiplier = new Vector2(0, PublicValues.CellHeight);

    void Start()
    {
        var tilemap = Singleton.Instance.Grids[_currentLayer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        StartCoroutine(TurnLoop());
        _chestLayer = Mathf.RoundToInt((_chest.position - _feet.position / PublicValues.CellHeight).y);
        _eyesLayer = Mathf.RoundToInt((_eyes.position - _feet.position / PublicValues.CellHeight).y);
        _playerSprite.sortingOrder = _currentLayer + 1;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
           UpdateCurrentPath();
        }
        
    }

    private IEnumerator TurnLoop()
    {

        if (_path == null)
        {
            yield return new WaitForEndOfFrame();
            yield return TurnLoop();
        }
        yield return MoveNext();
        foreach (var enemy in Singleton.Instance.EnemyMoves)
        {
            enemy.PlayerPos = _position;
            enemy.PlayerLayer = _currentLayer;
            enemy.PlayerTransform = transform;
            enemy.PlayerChest = _chest;
            enemy.PlayerEyes = _eyes;
            enemy.PlayerFeet = _feet;
            enemy.PlayerEyesLayer = _eyesLayer;
            enemy.PlayerChestLayer = _chestLayer;
            yield return enemy.MoveNext(); 
        }
        yield return new WaitForSeconds(0.1f);
        yield return TurnLoop();
    }

    private IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        if (_path == null) yield break;
        var node = _path.Nodes[_pathIndex];
        yield return MoveToCell(node);
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            yield break;
        }
        _pathIndex++;
    }

    public IEnumerator MoveToCell(Node node)
    {
        var direction = node.Center - transform.position;
        _position = new Vector2Int(node.X, node.Y);
        _currentLayer = node.Z;
        _playerSprite.sortingOrder = _currentLayer + 1;
        while (Vector2.Distance(transform.position, node.Center) > 0.01)
        {
            transform.position += direction * Time.deltaTime * movementSpeed;
            yield return new WaitForEndOfFrame();
        }

    }


    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        NodeGrid previousLayer = null;
        Node previousNode = null;
        var currentLayer = Singleton.Instance.Grids[_currentLayer];
        var currentTilemap = currentLayer.GetTilemap();
        var currentPos = currentTilemap.WorldToCell(transform.position);
        var currentNode = currentLayer.GetNodeFromCell((int)currentPos.x, (int)currentPos.y);
        
        foreach (var layer in Singleton.Instance.ReversedGrids)
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
                var path = Singleton.Instance.Pathfinding.FindPath(currentPos.x, currentPos.y, _currentLayer,
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
