using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    public int FCost, GCost, HCost;
    public readonly Vector2Int Position;
    public Node PreviousNode;
    public bool hasTile;
    public Vector3 center;
    public BaseTileRules tile;

    public Node(int x, int y, bool hasTile, Vector3 center, BaseTileRules tile)
    {
        Position = new Vector2Int(x, y);
        this.hasTile = hasTile;
        this.center = center;
        this.tile = tile;
    }

    public void UpdateFCost()
    {
        FCost = GCost + HCost;
    }
}

public class NodeGrid
{
    public readonly int Width, Height, Layer;
    public Tilemap Tilemap;
    public Node[,] Grid;

    public NodeGrid(int width, int height, int layer, Tilemap tilemap)
    {
        Width = width;
        Height = height;
        Layer = layer;
        Tilemap = tilemap;

        Grid = new Node[width, height];
        for (var x = 0; x < Grid.GetLength(0); x++)
        {
            for (var y = 0; y < Grid.GetLength(1); y++)
            {
                var position = new Vector3Int(x - Grid.GetLength(0) / 2, y - Grid.GetLength(1) / 2);
                Grid[x, y] = new Node(x - Grid.GetLength(0) / 2, y - Grid.GetLength(1) / 2,
                    tilemap.HasTile(position), tilemap.GetCellCenterWorld(position), tilemap.GetTile<BaseTileRules>(position));
            }
        }
    }

    public Node GetNodeFromCell(Vector2Int position)
    {
        return Grid[position.x + Grid.GetLength(0) / 2, position.y + Grid.GetLength(1) / 2];
    }
}
