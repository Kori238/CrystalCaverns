using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps;

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
                if (tilemap.HasTile(tilemap.WorldToCell(mousePointInWorld)))
                {
                    tallestLayer = tilemap.name + tilemap.WorldToCell(mousePointInWorld);
                    break;
                }
            }
            Debug.Log(tallestLayer);
        }
        
    }
}
