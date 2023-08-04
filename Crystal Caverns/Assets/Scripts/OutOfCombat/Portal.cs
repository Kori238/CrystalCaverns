using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    private GridBasedBehaviours _gridBasedBehaviours;
    private GameObject _loadingScreen;
    [SerializeField] private int _currentLayer = 0;
    [SerializeField] private Vector2Int pos;
    [SerializeField] private SceneNames sceneName;
    [SerializeField] private Vector3Int playerDestination;

    void Awake()
    {
        _gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
        _loadingScreen = GameObject.Find("LoadingScreen");
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
        ChangeScene(sceneName);
    }

    private async void ChangeScene(SceneNames name)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName.ToString());
        scene.allowSceneActivation = false;

        _loadingScreen.SetActive(true);
        var loadingScreenBackground = _loadingScreen.transform.GetChild(0).GetComponent<Image>();
        var alpha = 0f;

        while (alpha < 1f)
        {
            alpha += 2f * Time.deltaTime;
            loadingScreenBackground.color = new Color(0f, 0f, 0f, alpha);
            await Task.Yield();
        }

        do
        {

        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;

        while (alpha > 0f)
        {
            alpha -= 2f * Time.deltaTime;
            loadingScreenBackground.color = new Color(0f, 0f, 0f, alpha);
            await Task.Yield();
        }

        _loadingScreen.SetActive(false);
    }
}
