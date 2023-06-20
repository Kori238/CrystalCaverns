using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Node
{
    public int FCost, GCost, HCost;
    public readonly int x, y, z;
    public Node PreviousNode;
    public bool HasTile;
    public Vector3 Center;
    public BaseTileRules Tile;

    public Node(int x, int y, int z, bool hasTile, Vector3 center, BaseTileRules tile)
    {
        this.x = x; this.y = y; this.z = z;
        this.HasTile = hasTile;
        this.Center = center;
        this.Tile = tile;
    }

    public void UpdateFCost()
    {
        FCost = GCost + HCost;
    }
}

public class NodeGrid
{
    public readonly int Width, Height, Layer;
    private readonly Tilemap _tilemap;
    private readonly Node[,] _grid;

    public NodeGrid(int width, int height, int layer, Tilemap tilemap)
    {
        Width = width;
        Height = height;
        Layer = layer;
        _tilemap = tilemap;

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

    public Tilemap GetTilemap() { return _tilemap; }
    public Node[,] GetGrid() { return _grid; }

    public Node GetNodeFromCell(int x, int y)
    {
        return _grid[x + _grid.GetLength(0) / 2, y + _grid.GetLength(1) / 2];
    }
}

public class Path
{
    public int FCost, TCost;
    public List<Node> nodes;

    public Path()
    {
        FCost = 0;
        TCost = 0;
        nodes = new List<Node>();
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
    private readonly int layersCount;

    private readonly List<NodeGrid> _layers;
    private List<Node> _searchedNodes;
    private List<Node> _unsearchedNodes;

    public AStar(List<NodeGrid> layers)
    {
        _layers = layers; 
        _gridDimensions = new Vector2Int(_layers[0].GetGrid().GetLength(0) / 2, _layers[0].GetGrid().GetLength(1) / 2);
        layersCount = layers.Count;
    }

    public List<NodeGrid> GetLayers() { return _layers; }

    public NodeGrid GetLayer(int z) { return _layers[z]; }

    public Path FindPath(int x0, int y0, int z0, int x1, int y1, int z1)
    {
        Debug.Log(z0 + " " + z1);
        var startNode = GetLayer(z0).GetNodeFromCell(x0, y0);
        var endNode = GetLayer(z1).GetNodeFromCell(x1, y1);

        _unsearchedNodes = new List<Node> { startNode };
        _searchedNodes = new List<Node>();

        for (var z = 0; z < layersCount; z++)
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
            Debug.Log($"x{currentNode.x}, y:{currentNode.y}, z:{currentNode.z} " + currentNode.HasTile);
            
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            _unsearchedNodes.Remove(currentNode);
            _searchedNodes.Add(currentNode);

            if (!currentNode.HasTile) continue;

            List<Adjacents> adjacentsList;

            if (currentNode.Tile.layerTraversable)
            {
                adjacentsList = 
                    FindAdjacentsOnLayerTraversalTile(currentNode.x, currentNode.y, currentNode.z);
            }
            else
            {
                adjacentsList = new List<Adjacents>
                    { FindAdjacents(currentNode.x, currentNode.y, currentNode.z) };
            }

            foreach (var adjacents in adjacentsList)
            {
                foreach (var adjacentNode in adjacents.SameLayer)
                {
                    if (_searchedNodes.Contains(adjacentNode) || !adjacentNode.HasTile) continue;
                    if (!adjacentNode.Tile.walkable)
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

    private Path CalculatePath(Node endNode)
    {
        var path = new Path { FCost = endNode.FCost };
        path.nodes.Add(endNode);
        var currentNode = endNode;
        while (currentNode.PreviousNode != null)
        {
            path.nodes.Add(currentNode.PreviousNode);
            currentNode = currentNode.PreviousNode;
        }
        path.nodes.Reverse();
        return path;
    }

    private Node FindLowestFCostNode(List<Node> nodeList)
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
        NodeGrid currentLayer = GetLayer(z);
        NodeGrid layerAbove = null;
        if (z + 1 <= layersCount) layerAbove = _layers[z + 1];
        Tilemap currentTilemap = currentLayer.GetTilemap();
        var cardinals = new List<Vector2Int>
        {
            new Vector2Int(x - 1, y),
            new Vector2Int(x + 1, y),
            new Vector2Int(x, y - 1),
            new Vector2Int(x, y + 1)
        };
        var diagonals = new List<Vector2Int>()
        {
            new Vector2Int(x - 1, y - 1),
            new Vector2Int(x - 1, y + 1),
            new Vector2Int(x + 1, y - 1),
            new Vector2Int(x + 1, y + 1),
        };
        foreach (var direction in cardinals)
        {
            if (QueryValidTile(direction.x, direction.y, z, currentLayer))
            {
                if (currentLayer.GetNodeFromCell(direction.x, direction.y).Tile.layerTraversable)
                    adj.LayerTraversalDown.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
                else adj.SameLayer.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
            }

            if (QueryValidTile(direction.x, direction.y, z + 1, layerAbove))
            {
                if (layerAbove.GetNodeFromCell(direction.x, direction.y).Tile.layerTraversable) 
                    adj.LayerTraversalUp.Add(layerAbove.GetNodeFromCell(direction.x, direction.y));
            }
        }
        foreach (var direction in diagonals)
        {
            if (QueryValidTile(direction.x, direction.y, z, currentLayer))
            {
                if (!currentLayer.GetNodeFromCell(direction.x, direction.y).Tile.layerTraversable)
                    adj.SameLayer.Add(currentLayer.GetNodeFromCell(direction.x, direction.y));
            }
        }
        return adj;
    }

    public bool HasTileAbove(int x, int y, int z)
    {
        if (z + 1 > layersCount) return false;
        if (GetLayer(z + 1).GetTilemap().HasTile(new Vector3Int(x, y))) return true;
        return false;
    }

    public bool QueryValidTile(int x, int y, int z, NodeGrid layer)
    {
        if (x > _gridDimensions.x || x < -_gridDimensions.x || y > _gridDimensions.y || y < -_gridDimensions.y ||
            z > layersCount || z < 0 || layer == null) return false;
        Node currentNode = layer.GetNodeFromCell(x, y);
        if (currentNode.HasTile && !HasTileAbove(x, y, z)) return true;
        return false;
    }

    public int CalculateDistanceCost(Node a, Node b)
    {
        var xDistance = Mathf.Abs(a.x - b.x);
        var yDistance = Mathf.Abs(a.y - b.y);
        var zDistance = Mathf.Abs(a.z - b.z);
        var remaining = Mathf.Abs(xDistance - yDistance);

        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining + LAYER_COST * zDistance;
    }


}
