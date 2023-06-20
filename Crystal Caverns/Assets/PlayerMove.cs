using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _playerSprite;
    private Path _path = null;
    private int _pathIndex = 1;
    private Vector2Int _position;
    [SerializeField] private int _currentLayer = 0;
    private static readonly Vector2 LayerMultiplier = new(0, 0.675f);

    void Start()
    {
        var tilemap = Singleton.Instance.Grids[_currentLayer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        StartCoroutine(TurnLoop());
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
        yield return MoveNext();
        if (_path == null)
        {
            yield return new WaitForEndOfFrame();
            yield return TurnLoop();
        }
        foreach (var enemy in Singleton.Instance.EnemyMoves)
        {
            enemy.PlayerPos = _position;
            enemy.PlayerLayer = _currentLayer;
            yield return enemy.MoveNext(); 
        }
        yield return new WaitForSeconds(0.1f);
        yield return TurnLoop();
    }

    private IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        if (_path == null) yield break;
        var node = _path.nodes[_pathIndex];       
        transform.position = node.Center;
        _position = new Vector2Int(node.x, node.y);
        _currentLayer = node.z;
        _playerSprite.sortingOrder = _currentLayer + 1;
        if (_pathIndex == _path.nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            yield break;
        }
        _pathIndex++;
    }

    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("mouse up");

        NodeGrid previousLayer = null;
        Node previousNode = null;
        var currentLayer = Singleton.Instance.Grids[_currentLayer];
        var currentTilemap = currentLayer.GetTilemap();
        foreach (var layer in Singleton.Instance.Grids)
        {
            Tilemap tilemap = layer.GetTilemap();
            int.TryParse(tilemap.name, out var newLayer);
            var layerVector = LayerMultiplier * newLayer;
            var cellPos = tilemap.WorldToCell(mousePointInWorld - layerVector);
            var currentPos = currentTilemap.WorldToCell(transform.position);
            var selectedNode =
                layer.GetNodeFromCell(cellPos.x, cellPos.y);
            var currentNode = currentLayer.GetNodeFromCell((int)currentPos.x, (int)currentPos.y);
            if (selectedNode == currentNode) return;
            if (selectedNode.HasTile && (previousLayer == null || !previousNode.HasTile))
            {
                var tile = selectedNode.Tile;
                if (!tile.walkable) continue;
                Debug.Log(
                    $"currentNode:< x:{currentNode.x}, y:{currentNode.y}, z:{currentPos.z} > targetNode:< x:{selectedNode.x}, y:{selectedNode.y}, z:{selectedNode.z}");
                var path = Singleton.Instance.Pathfinding.FindPath(currentPos.x, currentPos.y, _currentLayer,
                    selectedNode.x, selectedNode.y, selectedNode.z);
                if (path != null)
                {
                    _path = path;
                    _pathIndex = 1;
                    Node prevNode = null;
                    foreach (var node in _path.nodes)
                    {
                        if (prevNode != null) Debug.DrawLine(node.Center + new Vector3(0, 0.3f, 0), prevNode.Center + new Vector3(0, 0.3f, 0), Color.black, _path.nodes.Count);
                        //Debug.Log($"x:{node.x} y:{node.y} z:{node.z}");
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
