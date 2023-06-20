using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _playerSprite;
    private Path _path = null;
    private int _index = 1;
    private static readonly Vector2 LayerMultiplier = new Vector2(0, 0.675f);

    void Start()
    {
        StartCoroutine(TurnTimer());
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
           UpdateCurrentPath();
        }
        
    }

    private IEnumerator TurnTimer()
    {
        MoveNext();
        yield return new WaitForSeconds(1f);
        yield return TurnTimer();
    }

    private void MoveNext()
    {
        if (_path == null) return;
        transform.position = _path.nodes[_index].Center;
        if (_index == _path.nodes.Count - 1)
        {
            _path = null;
            _index = 1;
            return;
        }
        _index++;
    }

    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("mouse up");

        NodeGrid previousLayer = null;
        Node previousNode = null;
        foreach (var layer in Singleton.Instance.Grids)
        {
            Tilemap tilemap = layer.GetTilemap();
            int.TryParse(tilemap.name, out var newLayer);
            var layerVector = LayerMultiplier * newLayer;
            var cellPos = tilemap.WorldToCell(mousePointInWorld - layerVector);
            var currentPos = tilemap.WorldToCell(transform.position);
            var selectedNode =
                layer.GetNodeFromCell(cellPos.x, cellPos.y);
            var currentNode = layer.GetNodeFromCell((int)currentPos.x, (int)currentPos.y);
            if (selectedNode.HasTile && (previousLayer == null || !previousNode.HasTile))
            {
                var tile = selectedNode.Tile;
                if (!tile.walkable) continue;
                Debug.Log(
                    $"currentNode:< x:{currentNode.x}, y:{currentNode.y}, z:{currentNode.z} > targetNode:< x:{selectedNode.x}, y:{selectedNode.y}, z:{selectedNode.z}");
                _path = Singleton.Instance.Pathfinding.FindPath(currentPos.x, currentPos.y, currentPos.z,
                    selectedNode.x, selectedNode.y, selectedNode.z);
                _index = 1;
                Node prevNode = null;
                foreach (var node in _path.nodes)
                {
                    if (prevNode != null) Debug.DrawLine(node.Center, prevNode.Center, Color.black, 10f);
                    Debug.Log($"x:{node.x} y:{node.y} z:{node.z}");
                    prevNode = node;
                }
                _playerSprite.sortingOrder = newLayer + 1;
                break;
            }

            previousLayer = layer;
            previousNode = selectedNode;
        }
    }
}
