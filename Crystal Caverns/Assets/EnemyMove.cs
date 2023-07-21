using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyMove : MonoBehaviour, IMoveNext
{
    #pragma warning disable CS0649
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Transform _eyes, _chest, _feet;
    #pragma warning restore CS0649

    [SerializeField] private int _eyesLayer, _chestLayer;
    public int PlayerEyesLayer, PlayerChestLayer;
    private Path _path;
    private int _pathIndex = 1;
    [SerializeField] private const float VIEW_RADIUS = 100f;
    private Vector2Int _position = new();
    [SerializeField] private int _layer = 0;
    public Transform PlayerTransform, PlayerChest, PlayerEyes, PlayerFeet;
    public Vector2Int PlayerPos;
    public int PlayerLayer;

    void Start()
    {
        Singleton.Instance.EnemyMoves.Add(this);
        var tilemap = Singleton.Instance.Grids[_layer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        _chestLayer = Mathf.RoundToInt((_chest.position - _feet.position / PublicValues.CellHeight).y);
        _eyesLayer = Mathf.RoundToInt((_eyes.position - _feet.position / PublicValues.CellHeight).y);
        _sprite.sortingOrder = _layer + 1;
    }

    public IEnumerator MoveNext()
    {
        yield return new WaitForSeconds(0.1f);
        if (Vector3.Distance(transform.position, PlayerTransform.position) < VIEW_RADIUS && CheckLOS()) GetPath();
        Debug.Log(CheckLOS());
        if (_path == null || _path.Nodes.Count <= 1) yield break;
        var node = _path.Nodes[_pathIndex];
        transform.position = node.Center;
        _position = new Vector2Int(node.X, node.Y);
        _layer = node.Z;
        _sprite.sortingOrder = _layer + 1;
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            yield break;
        }
        _pathIndex++;
        yield return null;
    }

    public bool CheckLOS()
    {
        var chestHits = Physics2D.RaycastAll(_eyes.position, PlayerChest.position - _eyes.position,
            Vector2.Distance(_eyes.position, PlayerChest.position));
        var eyeHits = Physics2D.RaycastAll(_eyes.position, PlayerEyes.position - _eyes.position,
            Vector2.Distance(_eyes.position, PlayerEyes.position));
        var feetHits = Physics2D.RaycastAll(_eyes.position, PlayerFeet.position - _eyes.position,
            Vector2.Distance(_eyes.position, PlayerFeet.position));
        //Debug.DrawRay(_eyes.position, PlayerEyes.position - _eyes.position, Color.white, 10f);
        var hits = new List<RaycastHit2D[]>
        {
            chestHits,
            //eyeHits,
            //feetHits,
        };
        var playerOffsets = new List<int>
        {
            PlayerChestLayer + PlayerLayer,
            //PlayerEyesLayer + PlayerLayer,
            //PlayerLayer,
        };
        var selfOffsets = new List<int>
        {
            _chestLayer + _layer,
            //_eyesLayer + _layer,
            //_layer
        };
        var distances = new List<float>
        {
            Vector2.Distance(_eyes.position, PlayerChest.position),
            //Vector2.Distance(_eyes.position, PlayerEyes.position),
            //Vector2.Distance(_eyes.position, PlayerFeet.position)
        };
        

        for (var i = 0; i < hits.Count; i++)
        {
            var array = hits[i];
            var targetLayer = playerOffsets[i];
            var originLayer = selfOffsets[i];
            var distance = distances[i];
            var layerDifference = Mathf.Abs(targetLayer - originLayer);
            Debug.Log(layerDifference);
            var hasLOS = true;
            foreach (var hit in array)
            {
                int.TryParse(hit.transform.name, out var layerName);

                if (layerDifference == 0)
                {
                    if (layerName == targetLayer)
                    {
                        Debug.Log(hit.distance);
                        hasLOS = false;
                        continue;
                    }
                }
                var segmentSize = distance / (layerDifference + 1);
                
                var currentSegment = Mathf.FloorToInt(hit.distance / segmentSize);
                Debug.Log($"segment:{segmentSize}   distance:{distance}   hit distance:{hit.distance}   current segment:{currentSegment}   segmentLayerCheck:{originLayer - currentSegment}");

                if (targetLayer > originLayer)
                {
                    if (layerName == originLayer + currentSegment)
                    {
                        hasLOS = false;
                        continue;
                    }
                }

                if (layerName == originLayer - currentSegment)
                {
                    hasLOS = false;
                    continue;
                }

            }
            if (hasLOS)
            {
                return true;
            }
        }
        return false;
    }





    /*var layerDifference = Mathf.Abs(PlayerLayer - _layer);
    if (layerDifference == 0)
    {
        Debug.Log("Same");
        foreach (var hit in hits)
        {
            int.TryParse(hit.transform.gameObject.name, out var layerName);
            if (layerName == _layer + 1)
            {
                return false;
            }
        }
    }
    else if (PlayerLayer > _layer)
    {
        Debug.Log("Player above");
        foreach (var hit in hits)
        {
            int.TryParse(hit.transform.gameObject.name, out var layerName);
            if (layerName > _layer && layerName <= PlayerLayer)
            {
                return false;
            }
        }
    }
    else
    {
        Debug.Log("Player below");
        foreach (var hit in hits)
        {
            int.TryParse(hit.transform.gameObject.name, out var layerName);
            if (layerName <= _layer && layerName > PlayerLayer)
            {
                return false;
            }
        }
    }
    return true;
}*/

    public void GetPath()
    {
        var path = Singleton.Instance.Pathfinding.FindPath(_position.x, _position.y, _layer, PlayerPos.x, PlayerPos.y, PlayerLayer);
        if (path == null) return;
        _path = path;
        _pathIndex = 1;
    }
}

public interface IMoveNext
{
    public IEnumerator MoveNext();
}
