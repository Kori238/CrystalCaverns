using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GridChildren
{
    public List<Portal> portals = new();
    public List<EnemyMove> enemyMoves = new();
}

public sealed class GridBasedBehaviours : MonoBehaviour
{
    public AStar Pathfinding;
    public List<NodeGrid> Grids;
    public List<NodeGrid> ReversedGrids;
    public List<Tilemap> Tilemaps;
    public List<ISaveable> SavableItems = new();
    public List<EnemyMove> EnemyMoves = new();
    public GridChildren GridChildren = new();
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
        var DataManipulation = new DataPersistence();
        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var serializedPortals = DataManipulation.SerializeList<Portal, WrappedPortal>(GridChildren.portals);
        var serializedEnemies = DataManipulation.SerializeList<EnemyMove, WrappedEnemy>(GridChildren.enemyMoves);

        DataManipulation.LoadSerializedList<WrappedPortal>(serializedPortals);
        DataManipulation.LoadSerializedList<WrappedEnemy>(serializedEnemies);

        var serialized = JsonConvert.SerializeObject(Grids, Formatting.Indented, settings);
        Debug.Log(serialized);
        Grids = null;
        Grids = JsonConvert.DeserializeObject<List<NodeGrid>>(serialized);
        Debug.Log(JsonConvert.SerializeObject(Grids, Formatting.Indented, settings));
        Debug.Log(Grids);
        //Debug.Log(deserialized._grid[20, 20].HasTile);
    }

    public void UnwrapTilemapFromGrid(List<NodeGrid> wrappedGrids)
    {

    }
}

public interface ISaveable
{
    public object Save();
}

public interface IUnwrappable
{
    public void LoadPrefab();
}