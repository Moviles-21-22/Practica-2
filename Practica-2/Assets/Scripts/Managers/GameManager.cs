using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Enumerado que representa las escenas del juego
    /// </summary>
    [Serializable]
    public enum SceneOrder
    {
        MAIN_MENU = 0,
        SHOP = 1,
        GAME_SCENE = 2
    }

    [Serializable]
    public struct LevelPackData
    {
        public int completedLevels;
        public Levels[] levelsInfo;
    }

    [Serializable]
    public class CategoryData
    {
        // Paquete de niveles
        public LevelPackData[] levels;
    }

    [Serializable]
    public class ThemeData
    {
        public string colorPackName;
        public List<Color> colors;
        public bool isCurrTheme;
        public bool unlocked;
    }

//------------------------------------------------MANAGERS------------------------------------------------------------//

    public static GameManager instance;

    [Header("Seccion de Managers")] [Tooltip("Referencia al MainMenuManager")]
    public MainMenuManager mainMenu;

    [Tooltip("Referencia al AdsManager")] public ShopManager shop;

    [Tooltip("Referencia al LevelManager")]
    public LevelManager levelManager;

//-------------------------------------------ATRIBUTOS-EDITOR---------------------------------------------------------//

    [Tooltip("Tema inicial por defecto")] [SerializeField]
    private ColorPack defaultTheme;

    [SerializeField] private bool useDefaultTheme;
//------------------------------------------ATRIBUTOS-PRIVADOS--------------------------------------------------------//

    /// <summary>
    /// Lista de los datos de guardado de las categorías
    /// </summary>
    private List<CategoryData> categoriesData;

    /// <summary>
    /// Lista de los datos de guardado de los temas
    /// </summary>
    private List<ThemeData> themesData;

    /// <summary>
    /// Actual paquete de niveles
    /// </summary>
    private LevelPackData currPack;

    private CategoryData currCat;
    
    /// <summary>
    /// Actual tema escogido
    /// </summary>
    private ThemeData currTheme;

    //  Actual nivel en juego
    private Level currLevel;

    //  Actual pack cargado
    private Map currMap;

    //  Numero de pistas disponibles
    private int numHints;

    //  Tiene premium el jugador
    private bool isPremium;

    private DataToSave currData;

    private bool isDataLoaded;
//--------------------------------------------------------------------------------------------------------------------//

    public void Awake()
    {
        if (instance)
        {
            // Delegación de los managers del GameManager de la escena
            // Inicializacion en caso de que existan

            // MainMenuScene
            instance.mainMenu = mainMenu;
            if (instance.mainMenu)
                instance.mainMenu.Init();

            // ShopScene
            instance.shop = shop;
            if (instance.shop)
                instance.shop.Init(instance.themesData, instance.currTheme, instance.numHints);

            // GameScene
            instance.levelManager = levelManager;
            if (instance.levelManager)
                instance.levelManager.Init(instance.currMap, instance.currLevel, instance.currPack,
                    instance.currTheme.colors, instance.numHints);

            Destroy(gameObject);
        }
        else
        {
            // Instancia única
            instance = this;

            // Inicializacion de los managers de cada escena
            // MainMenuScene
            if (mainMenu)
                mainMenu.Init();
            // ShopScene
            if (shop)
                shop.Init(themesData, currTheme, numHints);
            // GameScene
            if (levelManager)
                levelManager.Init(currMap, currLevel, currPack, currTheme.colors, numHints);

            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Cierra la app
    /// </summary>
    public void CloseApp()
    {
        DataManager.DebugLogs("App cerrada");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
    System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
        Application.Quit();
#endif
    }

//--------------------------------------------GESTION-LOAD-SAVE-------------------------------------------------------//

    public void LoadData(List<Category> categories, List<ColorPack> themes, bool reload)
    {
        if (isDataLoaded)
            return;

        // Se cargan los datos que haya guardados. 
        // Si no hay datos, se crearán unos por defecto
        DataManager.LoadData(categories, themes, reload);
        currData = DataManager.GetCurrData();

        numHints = currData.GetNumHints();
        isPremium = currData.GetIsPremium();
        categoriesData = currData.GetCategories();
        themesData = currData.GetThemes();

        foreach (var theme in themesData)
        {
            if (!theme.isCurrTheme) continue;

            currTheme = theme;
            break;
        }

        //LoadCategoryData(defaultPack);
        //LoadThemesData();
        isDataLoaded = true;
    }

    private void LoadThemesData()
    {
        //TODO
        // var themeData = objToLoad.GetThemes();
        // int numThemes = themeData.Length;
        //
        // for (int i = 0; i < numThemes; i++)
        // {
        //     colorThemes[i].active = themeData[i].unlocked;
        //     DataManager.DebugLogs("Skin " + colorThemes[i].colorPackName + " cargada correctamente");
        // }
        //
        // if (colorThemes == null)
        //     DataManager.DebugLogs("No puedo cargar las skins en el gameManager");
        // else
        // {
        //     var currThemeIndex = objToLoad.GetCurrentTheme();
        //     currTheme = colorThemes[currThemeIndex];
        // }
        //
        // isDataLoaded = true;
    }

    /// <summary>
    /// Carga los datos de las categorías
    /// </summary>
    /// <param name="defaultPack"></param>
    private void LoadCategoryData(bool defaultPack)
    {
        //TODO
        // DataManager.DebugLogs("Empezamos a cargar los datos...");
        //
        // categories = currData.GetCategories();
        // int numCats = categories.Count;
        // for (int i = 0; i < numCats; i++)
        // {
        //     // Paquetes de niveles
        //     int numLevelPacks = categories[i].levels.Length;
        //     var levelPack = categories[i].levels;
        //     LoadLevelPacksData(categories, i, numLevelPacks, levelPack, defaultPack);
        // }
        //
        // DataManager.DebugLogs("Datos cargados...");
        // isDataLoaded = true;
    }

    private void LoadLevelPacksData(List<CategoryData> categories, int iCat,
        int numLevelPacks, LevelPackData[] levelPack, bool defaultPack)
    {
        //TODO
        // for (int j = 0; j < numLevelPacks; j++)
        // {
        //     // 1. Número de niveles completos
        //     categories[iCat].levels[j].completedLevels = levelPack[j].completedLevels;
        //
        //     // 2. Información de los niveles
        //     int numLevels = levelPack[j].levelsInfo.Length;
        //     for (int k = 0; k < numLevels; k++)
        //     {
        //         // 2.1 Records de cada uno de los niveles
        //         categories[iCat].levels[j].levelsInfo[k].record = levelPack[j].levelsInfo[k].record;
        //         // 2.2 Información de los niveles
        //         categories[iCat].levels[j].levelsInfo[k].state = levelPack[j].levelsInfo[k].state;
        //     }
        // }
    }

    private void LoadDefaultLevelPack(LevelPack levelPack)
    {
        if (!levelPack)
        {
            Debug.LogError("No se ha asignado ningún paquete por defecto");
        }
        else
        {
            currPack = new LevelPackData
            {
                completedLevels = levelPack.completedLevels
            };

            int numLevels = levelPack.levelsInfo.Length;
            currPack.levelsInfo = new Levels[numLevels];
            for (int i = 0; i < numLevels; i++)
            {
                currPack.levelsInfo[i] = new Levels
                {
                    record = levelPack.levelsInfo[i].record,
                    state = levelPack.levelsInfo[i].state
                };
            }
        }
    }

    /// <summary>
    /// Carga un nivel del pack de niveles escogido
    /// </summary>
    /// <param name="lvl">Nivel que se quiere cargar</param>
    /// <param name="levelPack">Paquete de niveles que se usará por defecto</param>
    public void LoadLevel(int lvl, LevelPack levelPack)
    {
        currPack = currCat.levels[lvl];
        currMap = new Map(levelPack.txt.text);
        if (lvl < 0 || lvl >= currMap.NumLevels())
            lvl = 0;
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int) SceneOrder.GAME_SCENE);
    }

    /// <summary>
    /// Guarda el estado del juego
    /// </summary>
    public void SaveGame()
    {
        //DataManager.Save(numHints, isPremium, categories, colorThemes, currTheme);
    }

    /// <summary>
    /// Carga un pack especifico de una categoría
    /// </summary>
    /// <param name="levelPack">Pack que se quiere cargar</param>
    /// <param name="catData">Categoría escogida</param>
    public void LoadPackage(LevelPackData levelPack, CategoryData catData)
    {
        currPack = levelPack;
        currCat = catData;
    }

//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Carga una escena
    /// </summary>
    /// <param name="scene">Escena a la que se quiere cambiar</param>
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
        DataManager.DebugLogs("Escena: " + scene + " cargada");
    }

    /// <summary>
    /// Añade una solución de nivel del paquete actual
    /// </summary>
    /// <param name="perfect">Determinas si el nivel es perfecto o no</param>
    /// <param name="movs">Número de movimientos para superar el nivel</param>
    /// <param name="numFlows">Numero de flujos del nivel</param>
    public void AddSolutionLevel(bool perfect, int movs, int numFlows)
    {
        //TODO
        // bool saved = false;
        // int i = 0;
        // int j = 0;
        // // De momento así, pero es mejorable
        // while (!saved && i < categories.Count)
        // {
        //     if (categories[i].name == currCategory.name)
        //     {
        //         while (!saved && j < categories[i].levels.Length)
        //         {
        //             if (categories[i].levels[j].name == currPack.name)
        //             {
        //                 saved = true;
        //
        //                 // Cuando ya se ha completado el nivel no hace falta esto
        //                 var currState = categories[i].levels[j].levelsInfo[currLevel.lvl].state;
        //
        //                 // Se añade un nivel completado al paquete
        //                 if (currState == Levels.LevelState.UNCOMPLETED)
        //                 {
        //                     categories[i].levels[j].completedLevels++;
        //                 }
        //
        //                 // Se asigna el nivel como perfeto o completo
        //                 categories[i].levels[j].levelsInfo[currLevel.lvl].state = movs == numFlows
        //                     ? Levels.LevelState.PERFECT
        //                     : Levels.LevelState.COMPLETED;
        //
        //                 // Se actualiza el record
        //                 var record = categories[i].levels[j].levelsInfo[currLevel.lvl].record;
        //                 categories[i].levels[j].levelsInfo[currLevel.lvl].record =
        //                     record <= movs && record != 0 ? record : movs;
        //
        //                 // El nivel actual se ha completado
        //                 SaveGame();
        //             }
        //
        //             j++;
        //         }
        //     }
        //
        //     i++;
        // }
    }

    /// <summary>
    /// Desbloquea un tema
    /// </summary>
    /// <param name="t">El tema a desbloquear</param>
    public void UnlockTheme(ThemeData t)
    {
        var theme = themesData[themesData.IndexOf(t)];
        theme.unlocked = true;
        SetTheme(theme);
    }

    /// <summary>
    /// Desbloquea el acceso premium del usuario
    /// </summary>
    public void UnLockPremium()
    {
        isPremium = true;
        SaveGame();
        shop.HideBanner();
    }

    /// <summary>
    /// Añade una/s pista y guarda el estado del juego
    /// </summary>
    /// <param name="numOfHints"></param>
    public void AddHints(int numOfHints)
    {
        numHints += numOfHints;
        SaveGame();
    }

    /// <summary>
    /// Usa una pista y guarda el estado
    /// </summary>
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

    /// <summary>
    /// Cambia el tema
    /// </summary>
    /// <param name="t">El tema a cambiar</param>
    public void SetTheme(ThemeData t)
    {
        currTheme = t;
        currTheme.isCurrTheme = true;
        SaveGame();
    }

    /// <summary>
    /// Cambia a un nivel en concreto
    /// </summary>
    /// <param name="lvl">Nivel al que se quiere cambiar</param>
    public void ChangeLevel(int lvl)
    {
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int) SceneOrder.GAME_SCENE);
    }
    
    /// <summary>
    /// Devuelve los colores del tema actual
    /// </summary>
    public List<Color> GetCurrTheme()
    {
        return currTheme.colors;
    }

    public List<CategoryData> GetCategories()
    {
        return categoriesData;
    }

    /// <summary>
    /// Devuelve el status de premium
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerPremium() { return isPremium; }
}