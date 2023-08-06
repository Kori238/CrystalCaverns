using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TileDictionary
{
    public Dictionary<string, BaseTileRules> TileDict = new();
    public TileDictionary()
    {
        AssetDatabase.FindAssets("t:BaseTileRules");
        foreach (var asset in AssetDatabase.FindAssets("t:BaseTileRules"))
        {
            Debug.Log(asset);
            var path = AssetDatabase.GUIDToAssetPath(asset);
            var pos = path.LastIndexOf("/") + 1;
            var tileName = path.Substring(pos, path.Length - pos - 6);
            var tile = AssetDatabase.LoadAssetAtPath<BaseTileRules>(AssetDatabase.GUIDToAssetPath(asset));
            tile.Name = tileName;
            Debug.Log(tile.TileType);
            try
            {
                TileDict.Add(tile.Name, tile);
            }
            catch (ArgumentException e)
            {
                Debug.LogError($"Tile could not be added to dictionary as it's name is duplicate {e.Message}, {e.StackTrace}");
            }
            
        }

        Debug.Log(TileDict);
    }
}
