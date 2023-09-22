using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor;
using Scene = UnityEditor.SearchService.Scene;

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
    public Portal.SceneNames loadedScene = Portal.SceneNames.None;

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
        var dataManipulation = new DataPersistence();
        if (!Directory.Exists(Application.persistentDataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        string sceneName;
        if (loadedScene == Portal.SceneNames.None)
        {
            Enum.TryParse(SceneManager.GetActiveScene().name, true, out loadedScene);
        } 
        if (loadedScene == Portal.SceneNames.None) sceneName = SceneManager.GetActiveScene().name;
        else sceneName = loadedScene.ToString();
        
        dataManipulation.SaveData($"/saves/{sceneName}.json", new WrappedGrid(this));
    }

    public void Unwrap(WrappedGrid wrappedObject)
    {
        var dataManipulation = new DataPersistence();
        Grids = wrappedObject.grids;
        UnwrapTilemapFromGrid();
        ReversedGrids = new List<NodeGrid>(Grids);
        ReversedGrids.Reverse();
        Pathfinding = new AStar(Grids);
        LOS = new LineOfSight(Grids);
        loadedScene = wrappedObject.sceneName;
        dataManipulation.LoadWrappedList<WrappedEnemy, EnemyMove>(wrappedObject.wrappedEnemies);
        dataManipulation.LoadWrappedList<WrappedPortal, Portal>(wrappedObject.wrappedPortals);
    }

    public void UnwrapTilemapFromGrid()
    {
        var z = 0;
        foreach (var layer in Grids)
        {
            var tilemap = Tilemaps[z];
            for (var x = 0; x < layer.Width; x++)
            {
                for (var y = 0; y < layer.Height; y++)
                {
                    var position = new Vector3Int(x - layer.Width / 2, y - layer.Height / 2);
                    var tileName = layer._grid[x, y].TileName;
                    if (tileName == null) continue;
                    var tile = Singleton.Instance.TileDictionary.GetValueOrDefault(tileName);
                    tilemap.SetTile(position, tile);
                }
            }
            z++;
        }
    }
}

public class WrappedGrid
{
    public List<NodeGrid> grids;
    public List<WrappedPortal> wrappedPortals;
    public List<WrappedEnemy> wrappedEnemies;
    public Portal.SceneNames sceneName;

    [JsonConstructor]
    public WrappedGrid(List<NodeGrid> grids, List<WrappedPortal> wrappedPortals, List<WrappedEnemy> wrappedEnemies, Portal.SceneNames sceneName)
    {
        this.grids = grids;
        this.wrappedPortals = wrappedPortals;
        this.wrappedEnemies = wrappedEnemies;
        this.sceneName = sceneName;
    }

    public WrappedGrid(GridBasedBehaviours gbb)
    {
        var dataManipulation = new DataPersistence();

        grids = gbb.Grids;
        wrappedPortals = dataManipulation.WrapList<Portal, WrappedPortal>(gbb.GridChildren.portals);
        wrappedEnemies = dataManipulation.WrapList<EnemyMove, WrappedEnemy>(gbb.GridChildren.enemyMoves);
        sceneName = gbb.loadedScene;
        //Enum.TryParse(SceneManager.GetActiveScene().name, true, out sceneName);
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