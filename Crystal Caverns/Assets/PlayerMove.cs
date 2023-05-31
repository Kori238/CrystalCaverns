using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private SpriteRenderer playerSprite;
    Vector2 layerMultiplier = new Vector2(0, 0.6f);

    // Start is called before the first frame update
    void Start()
    {
        tilemaps.Reverse();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("mouse up");
            
            string tallestLayer = "none";
            Tilemap previousTilemap = null;
            Vector2 previousLayerVector = Vector2.zero;
            foreach (var tilemap in tilemaps)
            {
                Int32.TryParse(tilemap.name, out var newLayer);
                Vector2 layerVector = layerMultiplier * newLayer;

                if (tilemap.HasTile(tilemap.WorldToCell(mousePointInWorld - layerVector)) 
                                    && (previousTilemap == null || !previousTilemap.HasTile(tilemap.WorldToCell(mousePointInWorld - layerVector))))
                {
                    tallestLayer = tilemap.name + tilemap.WorldToCell(mousePointInWorld - layerVector);
                    transform.position = tilemap.GetCellCenterWorld(tilemap.WorldToCell(mousePointInWorld - layerVector));
                    playerSprite.sortingOrder = newLayer + 1;
                    break;
                }
                previousTilemap = tilemap;
                previousLayerVector = layerVector;
            }
            Debug.Log(tallestLayer);
        }
        
    }
}
