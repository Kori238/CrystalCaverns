using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;
public sealed class Singleton
{
    public AStar Pathfinding;
    public List<NodeGrid> Grids;
    public List<Tilemap> Tilemaps;
    public List<EnemyMove> EnemyMoves = new();
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
        //Grids.Reverse();
        Pathfinding = new AStar(Grids);
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