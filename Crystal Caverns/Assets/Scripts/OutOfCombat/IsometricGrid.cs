using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IsometricGrid
{
    private Vector2 _size;

    public IsometricGrid(int width, int height)
    {
        _size = new Vector2(width, height);
    }
}
