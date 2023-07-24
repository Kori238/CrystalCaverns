using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    void Start()
    {
        var tilemap = Singleton.Instance.Grids[_currentLayer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
    }

    void Update()
    {
        
    }
}
