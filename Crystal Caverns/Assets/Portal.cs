using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private int _currentLayer = 0;
    [SerializeField] private Vector2Int pos;
    
    void Start()
    {
        var tilemap = Singleton.Instance.Grids[_currentLayer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        Debug.Log(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        pos = (Vector2Int)cell;
        var node = Singleton.Instance.Grids[_currentLayer].GetNodeFromCell(pos.x, pos.y);
        node.PlayerEnteredTile += PortalTo;
    }

    void OnDestroy()
    {
        var node = Singleton.Instance.Grids[_currentLayer].GetNodeFromCell(pos.x, pos.y);
        node.PlayerEnteredTile -= PortalTo;
    }

    private void PortalTo()
    {
        Debug.Log("PORTALTIME");
    }

    
}
