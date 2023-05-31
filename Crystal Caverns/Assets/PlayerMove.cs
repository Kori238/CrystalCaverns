using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private SpriteRenderer playerSprite;

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
            foreach (var tilemap in tilemaps)
            {
                Int32.TryParse(tilemap.name, out var newLayer);


                Vector2 layerVector = new Vector2((int)(newLayer * 0.24), (int)(newLayer * 0.24));
                if (tilemap.HasTile(tilemap.WorldToCell(mousePointInWorld - layerVector)))
                {
                    tallestLayer = tilemap.name + tilemap.WorldToCell(mousePointInWorld - layerVector);
                    transform.position = tilemap.GetCellCenterWorld(tilemap.WorldToCell(mousePointInWorld - layerVector));
                    playerSprite.sortingOrder = newLayer + 1;
                    break;
                }
            }
            Debug.Log(tallestLayer);
        }
        
    }
}
