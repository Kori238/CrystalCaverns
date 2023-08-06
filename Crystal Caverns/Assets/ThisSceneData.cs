using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ThisSceneData : MonoBehaviour
{
    public static ThisSceneData Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Test();
    }

    private async void Test()
    {
        var gridBasedBehaviours = GameObject.Find("Grid").GetComponent<GridBasedBehaviours>();
        gridBasedBehaviours.SaveGrid();
    }
}
