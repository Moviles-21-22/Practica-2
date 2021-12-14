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
    //  TODO(cambiarlo por datos) Lista con los todos los temas
    public List<ColorPack> themes;
    //  Tema actual
    private ColorPack currTheme;
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

    /// <summary>
    /// Devuelve si el usuario es premium o no
    /// </summary>
    public bool IsPremium()
    {
        return isPremium;
    }

    /// <summary>
    /// Desbloquea el acceso premium del usuario
    /// </summary>
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

    public void UseHint()
    {
        if (numHints > 0)
        {
            numHints -= 1;
        }
        else
        {
            throw new Exception("No se puede restar una pista pistas = 0");
        }
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

    public List<ColorPack> GetThemes()
    {
        return themes;
    }

    //  Devuelve el tema usado actualmente
    public ColorPack GetCurrTheme()
    {
        return currTheme;
    }

    //  Desbloquea un tema
    public void UnlockTheme(ColorPack t)
    {
        //  TODO hacerlo sobre el que se guarda, no sobre la copia original
        var v = themes[themes.IndexOf(t)];
        v.active = true;
        SetTheme(v);
        SaveGame();
    }

    //  Cambia el tema
    internal void SetTheme(ColorPack t)
    {
        currTheme = t;
        SaveGame();
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
        // De momento así, pero es mejorable
        while (!saved && i < categories.Count)
        {
            if (categories[i].name == currCategory.name)
            {
                while (!saved && j < categories[i].levels.Length)
                {
                    if (categories[i].levels[j].name == currPack.name)
                    {
                        saved = true;

                        // Cuando ya se ha completado el nivel no hace falta esto
                        if (!categories[i].levels[j].levelsInfo[currLevel.lvl].completed)
                        {
                            // Se añade un nivel completado al paquete
                            categories[i].levels[j].completedLevels++;
                            categories[i].levels[j].levelsInfo[currLevel.lvl].completed = true;
                        }

                        categories[i].levels[j].levelsInfo[currLevel.lvl].perfect = perfect;
                        // El nivel actual se ha completado
                        SaveGame();
                    }
                    j++;
                }
            }
            i++;
        }
    }
}
