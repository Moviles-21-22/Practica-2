using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelManager levelManager;

    //  Devuelve el nivel actual
    private string currRoute;
    private int currLevel;
    public static GameManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public void SetLevelManager(LevelManager otherLevelManager)
    {
        //TESTEO
        currRoute = Directory.GetCurrentDirectory() + @"\Assets\Resources\Txt\levelpack_0.txt";
        levelManager = otherLevelManager;
        if (levelManager != null)
        {
            levelManager.init(currRoute);
        }
    }

    public int GetCurrentLevel()
    {
        return currLevel;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
