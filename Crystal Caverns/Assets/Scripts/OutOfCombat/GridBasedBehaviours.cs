using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public sealed class GridBasedBehaviours : MonoBehaviour
{
    public AStar Pathfinding;
    public List<NodeGrid> Grids;
    public List<NodeGrid> ReversedGrids;
    public List<Tilemap> Tilemaps;
    public List<EnemyMove> EnemyMoves = new();
    public LineOfSight LOS;

    public void Awake()
    {
        Grids = new List<NodeGrid>();
        Tilemaps = new List<Tilemap>();
        var grid = this;
        var tilemaps = GetComponentsInChildren<Tilemap>();
        var i = 0;
        foreach (var tilemap in tilemaps)
        {
            Tilemaps.Add(tilemap);
            Grids.Add(new NodeGrid(51, 51, i, tilemap));
            i++;
        }

        ReversedGrids = new List<NodeGrid>(Grids);
        ReversedGrids.Reverse();
        Pathfinding = new AStar(Grids);
        LOS = new LineOfSight(Grids);
    }

    public void SaveGrid()
    {
        var serialized = JsonConvert.SerializeObject(Grids[0], Formatting.Indented,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        Debug.Log(serialized);
        Grids[0] = JsonConvert.DeserializeObject<NodeGrid>(serialized);
        Debug.Log(JsonConvert.SerializeObject(Grids[0], Formatting.Indented, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
        Debug.Log(Grids[0]);
        //Debug.Log(deserialized._grid[20, 20].HasTile);
    }

    public void UnwrapTilemapFromGrid(List<NodeGrid> wrappedGrids)
    {

    }
}

