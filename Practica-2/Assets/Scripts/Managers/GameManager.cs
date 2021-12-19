using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public enum SceneOrder
    {
        MAIN_MENU = 0,
        LEVEL_SELECT = 1,
        ADS = 2,
        GAME_SCENE = 3
    }

    //  Instancia estática del gameManager
    public static GameManager instance;
    //  Categoría acutal escogida
    private Category currCategory;
    //  Actual pack dentro de la categor�a usada
    private LevelPack currPack;
    //  Actual nivel en juego
    private Level currLevel;
    //  Array con todas las categor�as disponibles
    private List<Category> categories;
    //  Lista con los todos los temas
    private List<ColorPack> themes;
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

    //  Añade una pista y guarda el estado del juego
    public void AddHints(int numOfHints)
    {
        numHints += numOfHints;
        SaveGame();
    }

    //  Usa una pista y guarda el estado
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
        SaveGame();
    }

    //  Devuelve el número de pistas restantes
    public int GetNumHints()
    {
        return numHints;
    }

    //  Devuelve el actual pack en juego
    public LevelPack GetCurrentPack()
    {
        return currPack;
    }

    //  Devuelve el nivel actual cargado en gameManager
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

    //  Devuelve todos las skins de colores disponibles
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
        DataManager.instance.LogError("Escena: " + scene + " cargada");
    }
    //  Carga un nivel en concreto de la categor�a y pack cargadoss
    public void LoadLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString(), 1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int)SceneOrder.GAME_SCENE);
    }

    public void ChangeLevel(int lvl)
    {
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int)SceneOrder.GAME_SCENE);
    }

    //  Devuelve todas las categorías
    public List<Category> GetCategories()
    {
        return categories;
    }

    //  Carga el estado del juego en funci�n del json
    public void InitDataLoaded(DataToSave objToLoad)
    {
        DataManager.instance.LogError("Empezamos a cargar los datos...");
        categories = objToLoad.GetCategories();
        if (categories != null)
        {
            foreach (Category cat in categories)
            {
                DataManager.instance.LogError("Categoria " + cat.categoryName + " cargada correctamente");
            }
        }
        else
        {
            DataManager.instance.LogError("No puedo cargar las categorias en el gameManager");
        }
        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetPremiumStatus();
        themes = objToLoad.GetThemes();
        if (themes != null)
        {
            foreach (ColorPack cat in themes)
            {
                DataManager.instance.LogError("Skin " + cat.colorPackName + " cargada correctamente");
            }
        }
        else
        {
            DataManager.instance.LogError("No puedo cargar las skins en el gameManager");
        }
        currTheme = objToLoad.GetCurrentTheme();

        DataManager.instance.LogError("Datos cargados...");
    }

    //  Guarda el estado del juego
    public void SaveGame()
    {
        DataManager.instance.Save();
    }

    //  Carga un pack especifico de una categor�a
    public void LoadPackage(LevelPack level, Category cat)
    {
        DataManager.instance.LogError("Entro botón");
        currCategory = cat;
        currPack = level;
        LoadScene((int)SceneOrder.LEVEL_SELECT);
        DataManager.instance.LogError("Salgo botón");
    }

    /// <summary>
    /// Añade una solución de nivel del paquete actual
    /// </summary>
    /// <param name="perfect">Determinas si el nivel es perfecto o no</param>
    public void AddSolutionLevel(bool perfect, int movs)
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
                        var rec = categories[i].levels[j].records[currLevel.lvl];
                        categories[i].levels[j].records[currLevel.lvl] = rec <= movs && rec != 0 ? rec : movs;

                        // El nivel actual se ha completado
                        SaveGame();
                    }
                    j++;
                }
            }
            i++;
        }
    }

    public void CloseApp()
    {
        DataManager.instance.LogError("App cerrada");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
    System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif        
    }
}
