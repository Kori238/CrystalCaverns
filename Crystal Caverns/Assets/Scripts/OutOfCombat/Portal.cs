using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor.VersionControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

public class Portal : MonoBehaviour, ISaveable
{
    private GridBasedBehaviours _gridBasedBehaviours;
    private GameObject _loadingScreen;
    [SerializeField] private int _currentLayer = 0;
    [SerializeField] private Vector2Int pos;
    [SerializeField] public SceneNames sceneName;
    [SerializeField] public Vector3Int playerDestination;

    void Awake()
    {
        _gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
        _loadingScreen = GameObject.Find("LoadingScreen");
    }

    public enum SceneNames
    {
        None,
        ExampleRoom1,
        ExampleRoom2,
    }
    void Start()
    {
        var tilemap = _gridBasedBehaviours.Tilemaps[_currentLayer];
        var cell = tilemap.WorldToCell(transform.position);
        Debug.Log(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
        pos = (Vector2Int)cell;
        var node = _gridBasedBehaviours.Grids[_currentLayer].GetNodeFromCell(pos.x, pos.y);
        node.PlayerEnteredTile += PortalTo;
        _gridBasedBehaviours.GridChildren.portals.Add(this);
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

    private async void ChangeScene(SceneNames name, bool checkSaves = true)
    {
        AsyncOperation scene;
        if (checkSaves && File.Exists(Application.persistentDataPath + $"/saves/{name}.json"))
        {
            scene = SceneManager.LoadSceneAsync("CleanScene");
            scene.allowSceneActivation = false;
            _loadingScreen.SetActive(true);

            var dataManipulation = new DataPersistence();
            var wrappedGrid = dataManipulation.LoadData<WrappedGrid>($"/saves/{name}.json");
            
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
                await Task.Yield();
            } while (scene.progress < 0.9f);

            scene.allowSceneActivation = true;

            do
            {
                await Task.Yield();
            } while (!scene.isDone);

            var gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
            gridBasedBehaviours.Unwrap(wrappedGrid);

            while (alpha > 0f)
            {
                alpha -= 2f * Time.deltaTime;
                loadingScreenBackground.color = new Color(0f, 0f, 0f, alpha);
                await Task.Yield();
            }

            _loadingScreen.SetActive(false);
        }
        else
        {
            scene = SceneManager.LoadSceneAsync(sceneName.ToString());
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
                await Task.Yield();
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

    public object Save()
    {
        return new WrappedPortal(transform, sceneName, playerDestination);
    }
}

public class WrappedPortal : IUnwrappable
{
    public Vector3 Position;
    public Portal.SceneNames SceneName;
    public Vector3Int PlayerDestination;

    [JsonConstructor]
    public WrappedPortal(Vector3 position, Portal.SceneNames sceneName, Vector3Int playerDestination)
    {
        Position = position;
        SceneName = sceneName;
        PlayerDestination = playerDestination;
    }

    public WrappedPortal(Transform transform, Portal.SceneNames sceneName, Vector3Int playerDestination)
    {
        Position = transform.position;
        SceneName = sceneName;
        PlayerDestination = playerDestination;
    }

    public void LoadPrefab()
    {
        var portal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/portal.prefab");
        var instantiated = Object.Instantiate(portal, Position, Quaternion.identity).GetComponent<Portal>();
        instantiated.sceneName = SceneName;
        instantiated.playerDestination = PlayerDestination;
    }
}