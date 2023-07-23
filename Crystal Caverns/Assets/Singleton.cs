using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public struct PublicValues
{
    public static float CellHeight = 0.625f;
}

public sealed class Singleton
{
    //public float CELL_HEIGHT = 0.625f;
    public AStar Pathfinding;
    public List<NodeGrid> Grids;
    public List<NodeGrid> ReversedGrids;
    public List<Tilemap> Tilemaps;
    public List<EnemyMove> EnemyMoves = new();
    public LineOfSight LOS;

    private Singleton()
    {
        Grids = new List<NodeGrid>();
        Tilemaps = new List<Tilemap>();
        var grid = GameObject.Find("Grid");
        var tilemaps = grid.GetComponentsInChildren<Tilemap>();
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

    public static Singleton Instance => Nested.Instance;

    private class Nested
    {
        static Nested()
        {
        }

        internal static readonly Singleton Instance = new();
    }
}