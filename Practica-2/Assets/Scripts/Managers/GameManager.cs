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
        LEVEL_SELECT = 1,
        SHOP = 2,
        GAME_SCENE = 3
    }

//------------------------------------------------MANAGERS------------------------------------------------------------//

    public static GameManager instance;

    [Header("Seccion de Managers")] [Tooltip("Referencia al MainMenuManager")]
    public MainMenuManager mainMenu;

    [Tooltip("Referencia al AdsManager")] public ShopManager shop;

    [Tooltip("Referencia al SelectLevelManager")]
    public SelectLevelManager selectLevelSelectLevel;

    [Tooltip("Referencia al LevelManager")]
    public LevelManager levelManager;

//-------------------------------------------ATRIBUTOS-EDITOR---------------------------------------------------------//
    [Header("Categorias y temas del juego")]
    [Tooltip("Recarga los datos del juego a sus valores por defecto")]
    [SerializeField]
    private bool reloadData;

    [Tooltip("Lista de los paquetes de las categorías del juego")] [SerializeField]
    private List<Category> categories;

    [Tooltip("Lista de los paquetes de temas del juego")] [SerializeField]
    private List<ColorPack> colorThemes;

    [Header("Atributos que se quieren iniciar por defecto")] [Tooltip("Categoría por defecto")] [SerializeField]
    private Category defaultCategory;

    [SerializeField] private bool useDefaultCat;

    [Tooltip("Paquete de niveles por defecto")] [SerializeField]
    private LevelPack defaultLevelPack;

    [SerializeField] private bool useDefaultLevelPack;

    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private int defaultLevel;

    [SerializeField] private bool useDefaultLevel;

    [Tooltip("Tema inicial por defecto")] [SerializeField]
    private ColorPack defaultTheme;

    [SerializeField] private bool useDefaultTheme;
//------------------------------------------ATRIBUTOS-PRIVADOS--------------------------------------------------------//

    private Category currCategory;

    private LevelPack currPack;

    private ColorPack currTheme;

    //  Actual nivel en juego
    private Level currLevel;

    //  Actual pack cargado
    private Map currMap;

    //  Numero de pistas disponibles
    private int numHints;

    //  Tiene premium el jugador
    private bool isPremium;
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
                instance.mainMenu.Init(instance.categories, instance.currTheme.colors);

            // ShopScene
            instance.shop = shop;
            if (instance.shop)
                instance.shop.Init(instance.isPremium, instance.colorThemes,
                    instance.currTheme, instance.numHints);

            // SelectLevelScene
            instance.selectLevelSelectLevel = selectLevelSelectLevel;
            if (instance.selectLevelSelectLevel)
                instance.selectLevelSelectLevel.Init(instance.currPack, instance.currCategory.color);

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
            // 1. Inicializacion de los datos
            var currData = DataManager.Init(categories, colorThemes, reloadData);
            LoadData(currData);

            // 2. Inicialización de datos por defecto. Pensado para el debug especialmente
            if (useDefaultCat)
                currCategory = defaultCategory;
            if (useDefaultLevelPack)
                currPack = defaultLevelPack;
            if (useDefaultTheme)
                currTheme = defaultTheme;
            if (useDefaultLevel)
                LoadLevel(defaultLevel);

            // 3. Inicializacion de los managers en caso de que existan
            // MainMenuScene
            if (mainMenu)
                mainMenu.Init(categories, currTheme.colors);
            // ShopScene
            if (shop)
                shop.Init(isPremium, colorThemes, currTheme, numHints);
            //SelectLevelScene
            if (selectLevelSelectLevel)
                selectLevelSelectLevel.Init(currPack, currCategory.color);
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
    /// Carga un nivel en concreto de la categor�a y pack cargados
    /// </summary>
    /// <param name="lvl">Nivel que se quiere cargar</param>
    public void LoadLevel(int lvl)
    {
        currMap = new Map(currPack.txt.ToString());
        if (lvl < 0 || lvl >= currMap.NumLevels())
            lvl = 0;
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int) SceneOrder.GAME_SCENE);
    }

    /// <summary>
    /// Carga el estado del juego en función del json
    /// </summary>
    /// <param name="objToLoad">Data con toda la información</param>
    private void LoadData(DataToSave objToLoad)
    {
        DataManager.DebugLogs("Empezamos a cargar los datos...");

        var categoryData = objToLoad.GetCategories();
        int numCats = categoryData.Count;
        for (int i = 0; i < numCats; i++)
        {
            // Paquetes de niveles
            int numLevelPacks = categories[i].levels.Length;
            var levelPack = categoryData[i].levels;
            LoadLevelPacksData(i, numLevelPacks, levelPack);

            DataManager.DebugLogs("Categoria " + categories[i].name + " cargada correctamente");
        }

        if (categories == null) DataManager.DebugLogs("No puedo cargar las categorias en el gameManager");

        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetIsPremium();

        var themeData = objToLoad.GetThemes();
        int numThemes = themeData.Length;
        for (int i = 0; i < numThemes; i++)
        {
            colorThemes[i].active = themeData[i].unlocked;
            DataManager.DebugLogs("Skin " + colorThemes[i].colorPackName + " cargada correctamente");
        }

        if (colorThemes == null)
            DataManager.DebugLogs("No puedo cargar las skins en el gameManager");
        else
        {
            var currThemeIndex = objToLoad.GetCurrentTheme();
            currTheme = colorThemes[currThemeIndex];
        }

        DataManager.DebugLogs("Datos cargados...");
    }

    private void LoadLevelPacksData(int i, int numLevelPacks, DataToSave.LevelPackData[] levelPack)
    {
        for (int j = 0; j < numLevelPacks; j++)
        {
            // 1. Número de niveles completos
            categories[i].levels[j].completedLevels = levelPack[j].completedLevels;

            // 2. Información de los niveles
            int numLevels = levelPack[j].levelsInfo.Length;
            for (int k = 0; k < numLevels; k++)
            {
                // 2.1 Records de cada uno de los niveles
                categories[i].levels[j].levelsInfo[k].record = levelPack[j].levelsInfo[k].record;
                // 2.2 Información de los niveles
                categories[i].levels[j].levelsInfo[k].state = levelPack[j].levelsInfo[k].state;
            }
        }
    }

    /// <summary>
    /// Guarda el estado del juego
    /// </summary>
    public void SaveGame()
    {
        DataManager.Save(numHints, isPremium, categories, colorThemes, currTheme);
    }

    /// <summary>
    /// Carga un pack especifico de una categoría
    /// </summary>
    /// <param name="level">Pack que se quiere cargar</param>
    /// <param name="cat">Categoria que se quiere cargar</param>
    public void LoadPackage(LevelPack level, Category cat)
    {
        DataManager.DebugLogs("Entro botón");
        DataManager.DebugLogs("Cargando categoria: " + cat.categoryName);
        DataManager.DebugLogs("Cargando nivel: " + level.levelName);
        currCategory = cat;
        currPack = level;
        LoadScene((int) SceneOrder.LEVEL_SELECT);
        DataManager.DebugLogs("Salgo botón");
    }

    /// <summary>
    /// Añade una solución de nivel del paquete actual
    /// </summary>
    /// <param name="perfect">Determinas si el nivel es perfecto o no</param>
    /// <param name="movs">Número de movimientos para superar el nivel</param>
    /// <param name="numFlows">Numero de flujos del nivel</param>
    public void AddSolutionLevel(bool perfect, int movs, int numFlows)
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
                        var currState = categories[i].levels[j].levelsInfo[currLevel.lvl].state;

                        // Se añade un nivel completado al paquete
                        if (currState == Levels.LevelState.UNCOMPLETED)
                        {
                            categories[i].levels[j].completedLevels++;
                        }

                        // Se asigna el nivel como perfeto o completo
                        categories[i].levels[j].levelsInfo[currLevel.lvl].state = movs == numFlows
                            ? Levels.LevelState.PERFECT
                            : Levels.LevelState.COMPLETED;

                        // Se actualiza el record
                        var record = categories[i].levels[j].levelsInfo[currLevel.lvl].record;
                        categories[i].levels[j].levelsInfo[currLevel.lvl].record =
                            record <= movs && record != 0 ? record : movs;

                        // El nivel actual se ha completado
                        SaveGame();
                    }

                    j++;
                }
            }

            i++;
        }
    }

    /// <summary>
    /// Desbloquea un tema
    /// </summary>
    /// <param name="t">El tema a desbloquear</param>
    public void UnlockTheme(ColorPack t)
    {
        var v = colorThemes[colorThemes.IndexOf(t)];
        v.active = true;
        SetTheme(v);
    }

//--------------------------------------------------------------------------------------------------------------------//

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
    public void SetTheme(ColorPack t)
    {
        currTheme = t;
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
}