using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridInitializer : MonoBehaviour
{
    public List<NodeGrid> grids = new();
    public List<Tilemap> tilemaps = new();

    void Start()
    {
        var i = 0;
        foreach (var tilemap in transform.GetComponentsInChildren<Tilemap>())
        {
            tilemaps.Add(tilemap);
            grids.Add(new NodeGrid(51, 51, i, tilemap));
            Debug.Log(i);
            i++;
        }
        grids.Reverse();
        Debug.Log(grids);
    }
}
