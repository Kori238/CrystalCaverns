using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class LineOfSight
{
    private List<NodeGrid> grids;
    public LineOfSight(List<NodeGrid> grids)
    {
        this.grids = grids;
    }
    public bool HasLineOfSight(int x0, int y0, int z0, int x1, int y1, int z1)
        {
        var origin = new Vector3(x0, y0, z0);
        var destination = new Vector3(x1, y1, z1);

        var direction = destination - origin;
        var distance = Mathf.CeilToInt(direction.magnitude);

        var stepIncrement = direction.normalized;
        var currentPos = origin;

        for (var i = 0; i <= distance; i++)
        {
            var currentGridPos = new Vector3Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.y),
                Mathf.RoundToInt(currentPos.z));
            if (currentGridPos.z >= grids.Count)
            {
                Debug.Log($"OB {currentGridPos}");
                currentPos += stepIncrement;
                continue;
            }
            if (grids[currentGridPos.z].GetNodeFromCell(currentGridPos.x, currentGridPos.y).HasTile)
            {
                Debug.Log($"false {currentGridPos}");
                return false;
            }
            if (i < distance)
            {
                if (HasObstructionPerpendicular(currentGridPos, direction, Vector3.right) ||
                    HasObstructionPerpendicular(currentGridPos, direction, Vector3.up) ||
                    HasObstructionPerpendicular(currentGridPos, direction, Vector3.forward))
                {
                    //Debug.Log($"double perp {currentGridPos}");
                    return false;
                }
            }
            //Debug.Log($"true {currentGridPos}");
            currentPos += stepIncrement;
        }
        return true;
    }

    private bool HasObstructionPerpendicular(Vector3Int position, Vector3 direction, Vector3 axis)
    {
        // Calculate the two perpendicular directions using 2D rotations.
        var perpendicular1 = Vector3.Cross(direction, axis);
        var perpendicular2 = -perpendicular1;

        // Sample two points on either side of the current position in each perpendicular direction.
        var sidePoint1 = position + new Vector3Int(Mathf.RoundToInt(perpendicular1.x), Mathf.RoundToInt(perpendicular1.y), Mathf.RoundToInt(perpendicular1.z));
        var sidePoint2 = position + new Vector3Int(Mathf.RoundToInt(perpendicular2.x), Mathf.RoundToInt(perpendicular2.y), Mathf.RoundToInt(perpendicular2.z));

        return grids[position.z].GetNodeFromCell(sidePoint1.x, sidePoint1.y).HasTile && grids[position.z].GetNodeFromCell(sidePoint2.x, sidePoint2.y).HasTile;
    }
}
