using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //  Instancia estática del gameManager
    public static GameManager instance;
    //  Referencia al manager del nivel
    public LevelManager levelManager;
    //  Actual categoría usada
    private Category currCategory;
    //  Actual pack dentro de la categoría usada
    private LevelPack currPack;
    //  Actual nivel en juego
    private Level currLevel;
    //  Array con todas las categorías disponibles
    public Category[] categories;
    //  Actual pack cargado
    public Map currMap;
    //  Referencia al dataManager
    private DataManager dataManager;
    //  Número de pistas disponibles
    private int numHints = 0;
    //  Tiene premium el jugador
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
    //  Para cargar el estado del juego
    public void SetDataManager(DataManager _dataManager)
    {
        dataManager = _dataManager;
        if (dataManager != null)
        {
            dataManager.Load();
        }
    }
    //  El jugador cuenta con premium?
    public bool IsPremium()
    {
        return isPremium;
    }
    //  Set up del nivel actual
    public void SetLevelManager(LevelManager otherLevelManager)
    {
        levelManager = otherLevelManager;
        if (levelManager != null)
        {
            levelManager.init(currMap,currLevel);
        }
    }
    //  Devuelve el numero de pistas disponibles
    public int GetNumHints()
    {
        return numHints;
    }
    //  Devuelve el actual pack en juego
    public LevelPack GetCurrentPack()
    {
        return currPack;
    }
    //  Devuelve el nivel actual
    public int GetCurrLevel()
    {
        return currLevel.lvl;
    }
    //  Devuelve la categoría usada
    public Category GetCurrentCategory()
    {
        return currCategory;
    }
    //  Devuelve todas las categorías disponibles
    public Category [] GetCategories()
    {
        return categories;
    }
    //  Carga una escena
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
    //  Carga un nivel en concreto de la categoría y pack cargadoss
    public void LoadLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString(),1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene(3);
    }
    //  Carga el estado del juego en función del json
    public void InitDataLoaded(DataToSave objToLoad)
    {
        categories = objToLoad.GetCategories();
        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetPremiumStatus();
    }
    //  Guarda el estado del juego
    public void SaveGame()
    {
        dataManager.Save();
    }
    //  Carga un pack especifico de una categoría
    public void LoadPackage(LevelPack level, Category cat) 
    {
        currCategory = cat;
        currPack = level;
        LoadScene(2);
    }
}
