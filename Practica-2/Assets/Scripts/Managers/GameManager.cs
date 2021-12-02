using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Category currCategory;
    private LevelPack currPack;
    private Level currLevel;
    public Category[] categories;
    public Map currMap;

    private DataManager dataManager;

    private int numHints = 0;

    private bool isPremium = false;

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

    public bool IsPremium()
    {
        return isPremium;
    }

    public int GetNumHints()
    {
        return numHints;
    }

    public LevelPack GetCurrentPack()
    {
        return currPack;
    }

    public Level GetCurrLevel()
    {
        return currLevel;
    }

    public Category GetCurrentCategory()
    {
        return currCategory;
    }

    public Category [] GetCategories()
    {
        return categories;
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString(),1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene(3);
    }

    public void InitDataLoaded(DataToSave objToLoad)
    {
        categories = objToLoad.GetCategories();
        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetPremiumStatus();
    }

    public void LoadPackage(LevelPack level, Category cat) 
    {
        currCategory = cat;
        currPack = level;
        LoadScene(2);
    }
}
