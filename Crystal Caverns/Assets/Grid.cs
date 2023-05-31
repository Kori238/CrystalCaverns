using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<Tilemap> tilemaps;
    [SerializeField] private TileBase tileref;
    void Start()
    {
        
        tilemaps[0].SetTile(Vector3Int.zero, tileref);
        tilemaps[0].SetTile(new Vector3Int(0, -1), tileref);
        TileBase tile = tilemaps[0].GetTile(new Vector3Int(0,0));
        Debug.Log(tile.name);
        DestroyImmediate(((Tile)tile).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
