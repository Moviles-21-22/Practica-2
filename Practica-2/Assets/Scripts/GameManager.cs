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
    private Category currCategory;
    private LevelPack currLevel;
    public static GameManager instance;

    public Category[] categories;

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
        //TESTEO Segun peblo habrá que arrastrar los txts al gameManager
        //currRoute = Directory.GetCurrentDirectory() + @"\Assets\Data\Levels\Intro\levelpack_0.txt";
        //levelManager = otherLevelManager;
        //if (levelManager != null)
        //{
        //    levelManager.init(currRoute);
        //}
    }

    public LevelPack GetCurrentLevel()
    {
        return currLevel;
    }

    public Category GetCurrentCategory()
    {
        return currCategory;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadLevel(LevelPack level, Category cat) 
    {
        currCategory = cat;
        currLevel = level;
        LoadScene("GridGameSlelection");
    }
}
