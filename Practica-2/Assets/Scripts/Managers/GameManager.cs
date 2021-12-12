using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //  Instancia est�tica del gameManager
    public static GameManager instance;
    //  Categoría acutal escogida
    private Category currCategory;
    //  Actual pack dentro de la categor�a usada
    private LevelPack currPack;
    //  Actual nivel en juego
    private Level currLevel;
    //  Array con todas las categor�as disponibles
    private List<Category> categories;
    //  Actual pack cargado
    private Map currMap;
    //  N�mero de pistas disponibles
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

    public bool IsPremium()
    {
        return isPremium;
    }

    public void UnLockPremium()
    {
        isPremium = true;
        SaveGame();
        AdsManager.instance.HideBanner();
    }

    public void AddHints(int numOfHints)
    {
        numHints += numOfHints;
        SaveGame();
    }

    public int GetNumHints()
    {
        return numHints;
    }

    //  Devuelve el actual pack en juego
    public LevelPack GetCurrentPack()
    {
        return currPack;
    }

    public Level GetCurrLevel()
    {
        return currLevel;
    }
    //  Devuelve la categor�a usada
    public Category GetCurrentCategory()
    {
        return currCategory;
    }

    /// <summary>
    /// Devuelve el mapa actual cargado
    /// </summary>
    public Map GetMap()
    {
        return currMap;
    }

    //  Carga una escena
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
    //  Carga un nivel en concreto de la categor�a y pack cargadoss
    public void LoadLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString(), 1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene(3);
    }

    public void ChangeLevel(int lvl)
    {
        currLevel = currMap.GetLevel(lvl);
        LoadScene(3);
    }

    public List<Category> GetCategories()
    {
        return categories;
    }

    //  Carga el estado del juego en funci�n del json
    public void InitDataLoaded(DataToSave objToLoad)
    {
        print("Cargamos json");
        categories = objToLoad.GetCategories();
        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetPremiumStatus();
    }

    //  Guarda el estado del juego
    public void SaveGame()
    {
        DataManager.instance.Save();
    }

    //  Carga un pack especifico de una categor�a
    public void LoadPackage(LevelPack level, Category cat)
    {
        currCategory = cat;
        currPack = level;
        LoadScene(2);
    }

    /// <summary>
    /// Añade una solución de nivel del paquete actual
    /// </summary>
    /// <param name="perfect">Determinas si el nivel es perfecto o no</param>
    public void AddSolutionLevel(bool perfect)
    {
        bool saved = false;
        int i = 0;
        int j = 0;
        // De momento así
        while (!saved && i < categories.Count)
        {
            if (categories[i].name == currCategory.name)
            {
                while (!saved && j < categories[i].levels.Length)
                {
                    if (categories[i].levels[j].name == currPack.name)
                    {
                        saved = true;
                        // Se añade un nivel completado al paquete
                        categories[i].levels[j].completedLevels++;

                        // El nivel actual se ha completado
                        categories[i].levels[j].levelsInfo[currLevel.lvl - 1].completed = true;
                        categories[i].levels[j].levelsInfo[currLevel.lvl - 1].perfect = true;

                        SaveGame();
                        print("Guardado");
                    }
                    j++;
                }
            }
            i++;
        }
    }
}
