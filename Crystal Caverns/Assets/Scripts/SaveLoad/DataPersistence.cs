using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataPersistence
{
    public string SerializeList<T, T2>(List<T> objects)
    {
        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        List<T2> savedItems = new();
        foreach (var item in objects)
        {
            var savedItem = (T2)(item as ISaveable).Save();
            savedItems.Add(savedItem);
        }
        var serializedItems = JsonConvert.SerializeObject(savedItems, Formatting.Indented, settings);
        return serializedItems;
    }

    public void LoadSerializedList<T>(string serializedObjects)
    {
        var deserializedSavable = JsonConvert.DeserializeObject<List<T>>(serializedObjects);
        foreach (var item in deserializedSavable)
        {
            (item as IUnwrappable).LoadPrefab();
        }
    }


    public bool SaveData<T>(string relativePath, T data)
    {
        var path = Application.persistentDataPath + relativePath;
        try
        {
            if (File.Exists(path))
            {
                Debug.Log($"Data at {path} exists. Deleting old file");
                File.Delete(path);
            }
            else
            {
                Debug.Log($"Saving data at {path} for the first time");
            }

            using var stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Saving to {path} failed due to {e.Message} {e.StackTrace}");
            return false;
        }
    }

    public T LoadData<T>(string relativePath)
    {
        throw new System.NotImplementedException();
    }

    public bool SaveSceneData(string sceneName, string relativePath)
    {
        var path = Application.persistentDataPath + relativePath;
        var grid = GameObject.Find("Grid");
        var serialized = JsonConvert.SerializeObject(grid);
        var deserialized = JsonConvert.DeserializeObject(serialized);
        throw new System.NotImplementedException();
    }

    public bool LoadSceneData(string sceneName, string relativePath)
    {
        throw new System.NotImplementedException(); 
    }

    public async void LoadScene(string sceneName, string relativePath)
    {
        var loadingScreen = GameObject.Find("LoadingScreen");
        var scene = SceneManager.LoadSceneAsync(sceneName.ToString());
        scene.allowSceneActivation = false;

        loadingScreen.SetActive(true);
        var loadingScreenBackground = loadingScreen.transform.GetChild(0).GetComponent<Image>();
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

        loadingScreen.SetActive(false);
    }

    public bool SaveGame(string relativePath)
    {
        var path = Application.persistentDataPath + relativePath;
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/temp/");

        try
        {
            var allFiles = Directory.GetFiles(Application.persistentDataPath + "/temp/", "*.unity");
            foreach (var newPath in allFiles)
            {
                File.Copy(newPath, newPath.Replace(Application.persistentDataPath + "/temp/", path + "/temp/"));
            }
            SaveData(relativePath, Singleton.Instance.PlayerStats);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not save game at {path} due to {e.Message} {e.StackTrace}");
            return false;
        }
    }

    public bool LoadGame(string relativePath)
    {
        var path = Application.persistentDataPath + relativePath;
        Directory.Delete(Application.persistentDataPath + "/temp/");
        Directory.CreateDirectory(Application.persistentDataPath + "/temp/");
        try
        {
            var allFiles = Directory.GetFiles(path+ "/temp/", "*.unity");
            foreach (var newPath in allFiles)
            {
                File.Copy(newPath, newPath.Replace(path + "/temp/", Application.persistentDataPath + "/temp/"));
            }
            Singleton.Instance.PlayerStats = LoadData<PlayerStats>(relativePath + "/player-stats.json");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not load save at {path} due to {e.Message} {e.StackTrace}");
            return false;
        }
    }
}

public interface IDataManipulation
{
    bool SaveData<T>(string relativePath, T data);

    T LoadData<T>(string relativePath);
}