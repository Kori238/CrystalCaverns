using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Node
{
    public int FCost, GCost, HCost;
    public readonly int X, Y, Z;
    public Node PreviousNode;
    public bool HasTile;
    public Vector3 Center;
    public string TileName;

    [JsonIgnore]
    public Action PlayerEnteredTile;
    [JsonIgnore]
    public Action PlayerExitedTile;
    [JsonIgnore]
    public Action CharacterEnteredTile;
    [JsonIgnore]
    public Action CharacterExitedTile;
    [JsonIgnore]
    public Action EnemyEnteredTile;
    [JsonIgnore]
    public Action EnemyExitedTile;

    public Node(int x, int y, int z, bool hasTile, Vector3 center, BaseTileRules tile)
    {
        //var dict = Singleton.Instance.TileDictionary;
        this.X = x; this.Y = y; this.Z = z;
        this.HasTile = hasTile;
        this.Center = center;
        if (tile != null)
        {
            this.TileName = tile.Name;
        }
    }

    public void UpdateFCost()
    {
        FCost = GCost + HCost;
    }
}

public class NodeGrid
{
    public readonly int Width, Height, Layer;
    public readonly Node[,] _grid;

    [JsonConstructor]
    public NodeGrid(int width, int height, int layer, Node[,] _grid)
    {
        Width = width; Height = height; Layer = layer;
        this._grid = _grid;
    }

    public NodeGrid(int width, int height, int layer, Tilemap tilemap)
    {
        Width = width;
        Height = height;
        Layer = layer;
        _grid = new Node[width, height];

        for (var x = 0; x < _grid.GetLength(0); x++)
        {
            for (var y = 0; y < _grid.GetLength(1); y++)
            {
                var position = new Vector3Int(x - _grid.GetLength(0) / 2, y - _grid.GetLength(1) / 2);
                _grid[x, y] = new Node(x - _grid.GetLength(0) / 2, y - _grid.GetLength(1) / 2, layer,
                    tilemap.HasTile(position), tilemap.GetCellCenterWorld(position), tilemap.GetTile<BaseTileRules>(position));
            }
        }
    }

    public Node[,] GetGrid() { return _grid; }

    public Node GetNodeFromCell(int x, int y)
    {
        return _grid[x + _grid.GetLength(0) / 2, y + _grid.GetLength(1) / 2];
    }
}

public class Path
{
    public int FCost, Cost;
    public List<Node> Nodes;

    public Path()
    {
        FCost = 0;
        Cost = 0;
        Nodes = new List<Node>();
    }
}

public class Adjacents
{
    public List<Node> SameLayer;
    public List<Node> LayerTraversalUp;
    public List<Node> LayerTraversalDown;

    public Adjacents()
    {
        SameLayer = new List<Node>();
        LayerTraversalUp = new List<Node>();
        LayerTraversalDown = new List<Node>();
    }
}

public class AStar
{
    private const int DIAGONAL_COST = 14;
    private const int STRAIGHT_COST = 10;
    private const int LAYER_COST = 25;
    private readonly Vector2Int _gridDimensions;
    private readonly int _layersCount;

    private readonly List<NodeGrid> _layers;
    private List<Node> _searchedNodes;
    private List<Node> _unsearchedNodes;

    public AStar(List<NodeGrid> layers)
    {
        _layers = layers; 
        _gridDimensions = new Vector2Int(_layers[0].GetGrid().GetLength(0) / 2, _layers[0].GetGrid().GetLength(1) / 2);
        _layersCount = layers.Count;
    }

    public List<NodeGrid> GetLayers() { return _layers; }

    public NodeGrid GetLayer(int z) { return _layers[z]; }

    public Path FindPath(int x0, int y0, int z0, int x1, int y1, int z1)
    {
        var startNode = GetLayer(z0).GetNodeFromCell(x0, y0);
        var endNode = GetLayer(z1).GetNodeFromCell(x1, y1);

        _unsearchedNodes = new List<Node> { startNode };
        _searchedNodes = new List<Node>();

        for (var z = 0; z < _layersCount; z++)
        {
            var currentLayer = GetLayer(z);
            for (var x = -25; x < currentLayer.GetGrid().GetLength(0)/2; x++)
            {
                for (var y = -25; y < currentLayer.GetGrid().GetLength(1)/2; y++)
                {
                    var node = currentLayer.GetNodeFromCell(x, y);
                    node.GCost = int.MaxValue;
                    node.UpdateFCost();
                    node.PreviousNode = null;
                }
            }
        }

        var i = 0;
        while (_unsearchedNodes.Count > 0 && i < 1000)
        {
            i++;
            var currentNode = FindLowestFCostNode(_unsearchedNodes);
            //Debug.Log($"x{currentNode.x}, y:{currentNode.y}, z:{currentNode.z} " + currentNode.HasTile);
            
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            _unsearchedNodes.Remove(currentNode);
            _searchedNodes.Add(currentNode);

            if (!currentNode.HasTile) continue;

            List<Adjacents> adjacentsList;

            var dict = Singleton.Instance.TileDictionary;
            if (dict.GetValueOrDefault(currentNode.TileName).LayerTraversable)
            {
                adjacentsList = 
                    FindAdjacentsOnLayerTraversalTile(currentNode.X, currentNode.Y, currentNode.Z);
            }
            else
            {
                adjacentsList = new List<Adjacents>
                    { FindAdjacents(currentNode.X, currentNode.Y, currentNode.Z) };
            }

            foreach (var adjacents in adjacentsList)
            {
                var allAdjacent = adjacents.SameLayer.Concat(adjacents.LayerTraversalUp).Concat(adjacents.LayerTraversalDown).ToList();
                foreach (var adjacentNode in allAdjacent)
                {
                    if (_searchedNodes.Contains(adjacentNode) || !adjacentNode.HasTile) continue;
                    if (!dict.GetValueOrDefault(adjacentNode.TileName).Walkable)
                    {
                        _searchedNodes.Add(adjacentNode);
                        continue;
                    }

                    var tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, adjacentNode);
                    if (tentativeGCost < adjacentNode.GCost)
                    {
                        adjacentNode.PreviousNode = currentNode;
                        adjacentNode.GCost = tentativeGCost;
                        adjacentNode.HCost = CalculateDistanceCost(adjacentNode, endNode);
                        adjacentNode.UpdateFCost();

                        if (!_unsearchedNodes.Contains(adjacentNode))
                        {
                            _unsearchedNodes.Add(adjacentNode);
                        }
                    }
                }
            }
        }
        return null;
    }

    private static Path CalculatePath(Node endNode)
    {
        var path = new Path { FCost = endNode.FCost };
        path.Nodes.Add(endNode);
        var currentNode = endNode;
        while (currentNode.PreviousNode != null)
        {
            path.Nodes.Add(currentNode.PreviousNode);
            currentNode = currentNode.PreviousNode;
        }
        path.Nodes.Reverse();
        return path;
    }

    private static Node FindLowestFCostNode(List<Node> nodeList)
    {
        var lowestFCostNode = nodeList[0];
        foreach (var node in nodeList)
        {
            if (node.FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = node;
            }
        }

        return lowestFCostNode;
    }

    public List<Adjacents> FindAdjacentsOnLayerTraversalTile(int x, int y, int z)
    {
        var adjs = new List<Adjacents>
        {
            FindAdjacents(x, y, z - 1),
            FindAdjacents(x, y, z)
        };
        return adjs;
    }

    public Adjacents FindAdjacents(int x, int y, int z)
    {
        var adj = new Adjacents();
        var currentLayer = GetLayer(z);
        NodeGrid layerAbove = null;
        if (z + 2 <= _layersCount) layerAbove = _layers[z + 1];
        var cardinals = new List<Vector2Int>
        {
            new Vector2Int(x - 1, y),
            new Vector2Int(x + 1, y),
            new Vector2Int(x, y - 1),
            new Vector2Int(x, y + 1)
        };
        var diagonals = new List<Vector2Int>
        {
            new Vector2Int(x - 1, y - 1),
            new Vector2Int(x - 1, y + 1),
            new Vector2Int(x + 1, y - 1),
            new Vector2Int(x + 1, y + 1)
        };
        var dict = Singleton.Instance.TileDictionary;
        foreach (var direction in cardinals)
        {
            if (QueryValidTile(direction.x, direction.y, z, currentLayer))
            {
                if (dict.GetValueOrDefault(currentLayer.GetNodeFromCell(direction.x, direction.y).TileName).LayerTraversable)
                    adj.LayerTraversalDown.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
                else adj.SameLayer.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
            }

            if (QueryValidTile(direction.x, direction.y, z + 1, layerAbove))
            {
                if (dict.GetValueOrDefault(layerAbove.GetNodeFromCell(direction.x, direction.y).TileName).LayerTraversable) 
                    adj.LayerTraversalUp.Add(layerAbove.GetNodeFromCell(direction.x, direction.y));
            }
        }
        foreach (var direction in diagonals)
        {
            if (!QueryValidTile(direction.x, direction.y, z, currentLayer)) continue;
            if (!dict.GetValueOrDefault(currentLayer.GetNodeFromCell(direction.x, direction.y).TileName).LayerTraversable)
                adj.SameLayer.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
        }
        return adj;
    }

    public bool HasTileAbove(int x, int y, int z)
    {
        return z + 2 <= _layersCount && GetLayer(z + 1).GetNodeFromCell(x, y).HasTile;
    }

    public bool QueryValidTile(int x, int y, int z, NodeGrid layer)
    {
        if (x > _gridDimensions.x || x < -_gridDimensions.x || y > _gridDimensions.y || y < -_gridDimensions.y ||
            z > _layersCount || z < 0 || layer == null) return false;
        var currentNode = layer.GetNodeFromCell(x, y);
        return currentNode.HasTile && (z > _layersCount || !HasTileAbove(x, y, z));
    }

    public int CalculateDistanceCost(Node a, Node b)
    {
        var xDistance = Mathf.Abs(a.X - b.X);
        var yDistance = Mathf.Abs(a.Y - b.Y);
        var zDistance = Mathf.Abs(a.Z - b.Z);
        var remaining = Mathf.Abs(xDistance - yDistance);

        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining + LAYER_COST * zDistance;
    }


}
