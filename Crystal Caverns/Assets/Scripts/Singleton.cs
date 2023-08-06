using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEditor.Experimental.RestService;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public struct PublicValues
{
    public static float CellHeight = 0.625f;
}

public sealed class Singleton
{
    public Vector3Int playerPortalDestination;
    public bool exploring = true;
    public DataPersistence Saving = new();
    public PlayerStats PlayerStats = new();

    public Dictionary<string, BaseTileRules> TileDictionary = new();
    public void CreateDictionary()
    {
        foreach (var asset in AssetDatabase.FindAssets("t:BaseTileRules"))
        {
            var path = AssetDatabase.GUIDToAssetPath(asset);
            var pos = path.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var tileName = path.Substring(pos, path.Length - pos - 6);
            var tile = AssetDatabase.LoadAssetAtPath<BaseTileRules>(AssetDatabase.GUIDToAssetPath(asset));
            tile.Name = tileName;
            try
            {
                TileDictionary.Add(tile.Name, tile);
            }
            catch (ArgumentException e)
            {
                Debug.LogError($"Tile could not be added to dictionary as it's name is duplicate {e.Message}, {e.StackTrace}");
            }

        }
    }
    private Singleton()
    {
        CreateDictionary();
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