using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class NewCustomRuleTile : RuleTile<NewCustomRuleTile.Neighbor> {
    public bool CustomField;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int NULL = 3;
        public const int NOT_NULL = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.NULL: return tile == null;
            case Neighbor.NOT_NULL: return tile != null;
        }
        return base.RuleMatch(neighbor, tile);
    }
}