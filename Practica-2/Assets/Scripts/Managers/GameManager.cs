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
        INIT_SCENE = 0,
        MAIN_MENU = 1,
        SHOP = 2,
        GAME_SCENE = 3
    }

    [Serializable]
    public struct LevelPackData
    {
        public string name;
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
        public string name;
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

//------------------------------------------ATRIBUTOS-PRIVADOS--------------------------------------------------------//

    /// <summary>
    /// Lista de los datos de guardado de las categorías
    /// </summary>
    private List<CategoryData> categoriesData = new List<CategoryData>();

    /// <summary>
    /// Lista de los datos de guardado de los temas
    /// </summary>
    private List<ThemeData> themesData = new List<ThemeData>();

    /// <summary>
    /// Actual paquete de niveles
    /// </summary>
    private LevelPackData currPack;

    private int indexCurrPack;

    private CategoryData currCat;
    private int indexCurrCat;

    /// <summary>
    /// Actual tema escogido
    /// </summary>
    private ThemeData currTheme;

    /// <summary>
    /// Actual nivel del juego
    /// </summary>
    private Level currLevel;

    //  Actual pack cargado
    /// <summary>
    /// Actual pack cargado
    /// </summary>
    private Map currMap;

    /// <summary>
    /// Número de pistas disponibles
    /// </summary>
    private int numHints;

    /// <summary>
    /// Premium del jugador
    /// </summary>
    private bool isPremium;

    /// <summary>
    /// Datos cargados actuales
    /// </summary>
    private DataToSave currData;

    /// <summary>
    /// Determina si ya se han cargado los datos
    /// </summary>
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
                    instance.numHints, instance.currTheme.colors);

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
                shop.Init(themesData, currTheme, numHints, true);
            // GameScene
            if (levelManager)
                levelManager.Init(currMap, currLevel, currPack, numHints, null, true);

            DontDestroyOnLoad(gameObject);
        }
    }

//--------------------------------------------GESTION-LOAD-SAVE-------------------------------------------------------//

    /// <summary>
    /// Carga los datos del juego. Si hay datos guardados, se cargarán esos, sino
    /// se crearán los datos por defecto.
    /// </summary>
    /// <param name="categories">Lista de categorías base para crear datos por defecto</param>
    /// <param name="themes">Lista de temeas base para crear datos por defecto</param>
    /// <param name="reload">Determina si se recrgan los datos guardados o no</param>
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

        isDataLoaded = true;
    }

    /// <summary>
    /// Carga un nivel del pack de niveles escogido
    /// </summary>
    /// <param name="lvl">Nivel que se quiere cargar</param>
    /// <param name="levelPack">Paquete de niveles que se usará por defecto</param>
    /// <param name="defaultLevel">Determina si se inicia un nivel por defecto</param>
    public void LoadLevel(int lvl, LevelPack levelPack, bool defaultLevel = false)
    {
        currPack.name = levelPack.name;
        currMap = new Map(levelPack.txt.text);
        if (lvl < 0 || lvl >= currMap.NumLevels())
            lvl = 0;
        currLevel = currMap.GetLevel(lvl);
        if (defaultLevel) return;

        LoadScene((int) SceneOrder.GAME_SCENE);
    }

    /// <summary>
    /// Guarda el estado del juego
    /// </summary>
    private void SaveGame()
    {
        if (currData == null)
        {
            Debug.LogError("La escena se ha inicializado desde la propia tienda.\n" +
                           "No se pueden guardar los datos\n" +
                           "Para poder guardar los datos, vuelve al MainMenu para que se carguen" +
                           " los datos correctamente");
            return;
        }

        currData.SetNewData(numHints, isPremium, categoriesData, themesData);
        DataManager.Save(currData);
    }

    /// <summary>
    /// Carga un pack especifico de una categoría
    /// </summary>
    /// <param name="levelPack">Pack que se quiere cargar</param>
    /// <param name="catData">Categoría escogida</param>
    public void LoadPackage(int levelPack, int catData)
    {
        currCat = categoriesData[catData];
        indexCurrCat = catData;
        currPack = currCat.levels[levelPack];
        indexCurrPack = levelPack;
    }

//-------------------------------------------------GET-SET------------------------------------------------------------//
    /// <summary>
    /// Devuelve la lista de los datos de las categorías
    /// </summary>
    public List<CategoryData> GetCategories()
    {
        return categoriesData;
    }

    /// <summary>
    /// Devuelve el status de premium
    /// </summary>
    public bool IsPlayerPremium()
    {
        return isPremium;
    }

    /// <summary>
    /// Devuelve el nivel actual
    /// </summary>
    public Level GetCurrLevel()
    {
        return currLevel;
    }

    /// <summary>
    /// Devuelve los colores del actual theme usado
    /// </summary>
    public List<Color> GetCurrentColorPack()
    {
        return currTheme.colors;
    }

    /// <summary>
    /// Cambia el tema
    /// </summary>
    /// <param name="t">El tema a cambiar</param>
    public void SetTheme(int t)
    {
        // El anterior se suelta
        currTheme.isCurrTheme = false;
        // Se pone el nuevo
        currTheme = themesData[t];
        currTheme.isCurrTheme = true;
        SaveGame();
    }
    
//------------------------------------------------ACTUALIZACIÓN-DATOS-------------------------------------------------//

    /// <summary>
    /// Añade una solución de nivel del paquete actual
    /// </summary>
    /// <param name="numMovs">Número de movimientos para superar el nivel</param>
    /// <param name="numFlows">Numero de flujos del nivel</param>
    public void AddSolutionLevel(int numMovs, int numFlows)
    {
        int numCats = categoriesData.Count;

        if (numCats == 0) return;
        var currState = currPack.levelsInfo[currLevel.lvl].state;

        // Se añade un nivel completado al paquete
        if (currState == Levels.LevelState.UNCOMPLETED)
        {
            categoriesData[indexCurrCat].levels[indexCurrPack].completedLevels++;
        }

        // Se asigna el nivel como perfeto o completo
        currPack.levelsInfo[currLevel.lvl].state = numMovs == numFlows
            ? Levels.LevelState.PERFECT
            : Levels.LevelState.COMPLETED;

        // Se actualiza el record
        var record = currPack.levelsInfo[currLevel.lvl].record;
        currPack.levelsInfo[currLevel.lvl].record =
            record <= numMovs && record != 0 ? record : numMovs;

        // El nivel actual se ha completado
        SaveGame();
    }

    /// <summary>
    /// Desbloquea un tema
    /// </summary>
    /// <param name="t">El tema a desbloquear</param>
    public void UnlockTheme(int t)
    {
        if (themesData.Count == 0)
            return;

        var theme = themesData[t];
        theme.unlocked = true;
        SetTheme(t);
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
    /// Añade una/s pista, actualiza y guarda el estado del juego
    /// </summary>
    /// <param name="numOfHints"></param>
    public void AddHints(int numOfHints)
    {
        numHints += numOfHints;
        if (levelManager) levelManager.UpdateHints(numHints);
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
    
//--------------------------------------------------------------------------------------------------------------------//
    
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
    /// Cierra la app
    /// </summary>
    public void CloseApp()
    {
        DataManager.DebugLogs("App cerrada");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        Application.Quit();
#endif
    }

    /// <summary>
    /// Carga una escena
    /// </summary>
    /// <param name="scene">Escena a la que se quiere cambiar</param>
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
        DataManager.DebugLogs("Escena: " + scene + " cargada");
    }
}