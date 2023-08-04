using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

    private Singleton()
    {

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