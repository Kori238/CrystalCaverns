using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
}
