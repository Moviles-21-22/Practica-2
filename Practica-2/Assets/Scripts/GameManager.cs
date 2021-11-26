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
    private LevelPack currPack;
    private Level currLevel;
    public static GameManager instance;
    public Category[] categories;
    public Map currMap;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetLevelManager(LevelManager otherLevelManager)
    {
        levelManager = otherLevelManager;
        if (levelManager != null)
        {
            levelManager.init(currMap,currLevel);
        }
    }

    public LevelPack GetCurrentPack()
    {
        return currPack;
    }

    public int GetCurrLevel()
    {
        return currLevel.lvl;
    }

    public Category GetCurrentCategory()
    {
        return currCategory;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadPackLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString(),1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene(3);
    }


    public void LoadLevel(LevelPack level, Category cat) 
    {
        currCategory = cat;
        currPack = level;
        LoadScene("GridGameSlelection");
    }
}
