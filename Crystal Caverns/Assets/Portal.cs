using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private GridBasedBehaviours _gridBasedBehaviours;
    [SerializeField] private int _currentLayer = 0;
    [SerializeField] private Vector2Int pos;
    [SerializeField] private SceneNames sceneName;
    [SerializeField] private Vector3Int playerDestination;

    void Awake()
    {
        _gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
    }

    public enum SceneNames
    {
        None,
        SampleScene,
        CoolScene,
    }
    void Start()
    {
        var tilemap = _gridBasedBehaviours.Grids[_currentLayer].GetTilemap();
        var cell = tilemap.WorldToCell(transform.position);
        Debug.Log(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        pos = (Vector2Int)cell;
        var node = _gridBasedBehaviours.Grids[_currentLayer].GetNodeFromCell(pos.x, pos.y);
        node.PlayerEnteredTile += PortalTo;
    }


    void OnDestroy()
    {
        var node = _gridBasedBehaviours.Grids[_currentLayer].GetNodeFromCell(pos.x, pos.y);
        node.PlayerEnteredTile -= PortalTo;
    }

    private void PortalTo()
    {
        Debug.Log("PORTALTIME");
        if (sceneName == SceneNames.None)
        {
            Debug.Log("Please configure the portal");
            return;
        }

        if (playerDestination == new Vector3Int(0, 0, 0))
        {
            Debug.Log("WARNING: portal destination may not be set");
        }
        Singleton.Instance.playerPortalDestination = playerDestination;
        SceneManager.LoadScene(sceneName.ToString());
    }

    
}
