using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinding
{
    

}

public class Path
{
    public int fCost;
    public int tCost;
    public List<BaseTileRules> tiles;
    public Path()
    {
        fCost = 0;
        tCost = 0; 
        tiles = new List<BaseTileRules>();
    }
}
