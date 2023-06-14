using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private List<Tilemap> _tilemaps;
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private GridInitializer _gridInitializer;
    Vector2 layerMultiplier = new Vector2(0, 0.675f);

    // Start is called before the first frame update
    void Start()
    {
        _tilemaps.Reverse();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("mouse up");
            
            NodeGrid previousLayer = null;
            Node previousNode = null;
            foreach (var layer in _gridInitializer.grids)
            {
                Int32.TryParse(layer.Tilemap.name, out var newLayer);
                Debug.Log(newLayer);
                Vector2 layerVector = layerMultiplier * newLayer;
                Debug.Log((Vector2Int)layer.Tilemap.WorldToCell(mousePointInWorld - layerVector));
                Node selectedNode =
                    layer.GetNodeFromCell((Vector2Int)layer.Tilemap.WorldToCell(mousePointInWorld - layerVector));
                Debug.Log(selectedNode.hasTile);


                if (selectedNode.hasTile && (previousLayer == null || !previousNode.hasTile))
                {
                    BaseTileRules tile = selectedNode.tile;
                    if (!tile.walkable) continue;
                    transform.position = selectedNode.center;
                    _playerSprite.sortingOrder = newLayer + 1;
                    break;
                }
                previousLayer = layer;
                previousNode = selectedNode;
            }
        }
        
    }
}
