using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO: Buscar todos los datamanager.LogError y mirar si se pueden borrar

[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Enumerado que representa las escenas del juego
    /// </summary>
    [System.Serializable]
    public enum SceneOrder
    {
        MAIN_MENU = 0,
        LEVEL_SELECT = 1,
        ADS = 2,
        GAME_SCENE = 3
    }

//------------------------------------------------MANAGERS------------------------------------------------------------//

    public static GameManager instance;

    [Tooltip("Referencia al MainMenuManager")]
    public MainMenuManager mainMenu;

//-------------------------------------------ATRIBUTOS-EDITOR---------------------------------------------------------//
    // TODO: Mirar que esto si deba ir aquí o se pueda delegar trabajo
    [Tooltip("Lista de los paquetes de las categorías del juego")] [SerializeField]
    private List<Category> categories;

    [Tooltip("Lista de los paquetes de temas del juego")] [SerializeField]
    private List<ColorPack> colorThemes;

    // TODO: Esto va en otro lado. Category para el selector de nivel y Level para la gameScene
    // [Tooltip("Categoria que se inicializa por defecto")] [SerializeField]
    // private Category defaultCategory;
    //
    // [Tooltip("Nivel que se inicializa por defecto")] [SerializeField]
    // private Level defaultLevel;
//------------------------------------------ATRIBUTOS-PRIVADOS--------------------------------------------------------//

    //  Categoría acutal escogida
    private Category currCategory;

    //  Actual pack dentro de la categor�a usada
    private LevelPack currPack;

    //  Actual nivel en juego
    private Level currLevel;

    //  Tema actual
    private ColorPack currTheme;

    //  Actual pack cargado
    private Map currMap;

    //  N�mero de pistas disponibles
    private int numHints;

    //  Tiene premium el jugador
    private bool isPremium;
//--------------------------------------------------------------------------------------------------------------------//

    public void Awake()
    {
        if (instance)
        {
            // Delegación de los managers del GameManager de la escena
            // 1. Inicializacion mainMenu
            instance.mainMenu = mainMenu;
            if (instance.mainMenu) instance.mainMenu.Init();

            Destroy(this.gameObject);
        }
        else
        {
            // Instancia única
            instance = this;
            // 1. Inicializacion de los datos
            DataManager.Init(categories, colorThemes);

            // 2. Inicializacion mainMenu
            if (mainMenu) mainMenu.Init();

            DontDestroyOnLoad(this.gameObject);
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
        currMap = new Map(currPack.txt.ToString(), 1);
        currLevel = currMap.GetLevel(lvl);
        LoadScene((int) SceneOrder.GAME_SCENE);
    }

    /// <summary>
    /// Carga el estado del juego en función del json
    /// </summary>
    /// <param name="objToLoad">Data con toda la información</param>
    public void LoadData(DataToSave objToLoad)
    {
        DataManager.DebugLogs("Empezamos a cargar los datos...");

        // 
        categories = objToLoad.GetCategories();

        foreach (Category cat in categories)
        {
            DataManager.DebugLogs("Categoria " + cat.categoryName + " cargada correctamente");
        }

        if (categories == null) DataManager.DebugLogs("No puedo cargar las categorias en el gameManager");

        numHints = objToLoad.GetNumHints();
        isPremium = objToLoad.GetPremiumStatus();

        colorThemes = objToLoad.GetThemes();
        foreach (ColorPack cat in colorThemes)
        {
            DataManager.DebugLogs("Skin " + cat.colorPackName + " cargada correctamente");
        }

        if (colorThemes == null) DataManager.DebugLogs("No puedo cargar las skins en el gameManager");

        currTheme = objToLoad.GetCurrentTheme();
        DataManager.DebugLogs("Datos cargados...");
    }

    /// <summary>
    /// Guarda el estado del juego
    /// </summary>
    public void SaveGame()
    {
        //dataManager.Save();
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


    /// <summary>
    /// Desbloquea un tema
    /// </summary>
    /// <param name="t">El tema a desbloquear</param>
    public void UnlockTheme(ColorPack t)
    {
        //  TODO hacerlo sobre el que se guarda, no sobre la copia original
        var v = colorThemes[colorThemes.IndexOf(t)];
        v.active = true;
        SetTheme(v);
        SaveGame();
    }

//--------------------------------------------------------------------------------------------------------------------//

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
    /// Devuelve el número de pistas restantes
    /// </summary>
    /// <returns></returns>
    public int GetNumHints()
    {
        return numHints;
    }

    /// <summary>
    /// Devuelve el actual pack en juego
    /// </summary>
    /// <returns></returns>
    public LevelPack GetCurrentPack()
    {
        return currPack;
    }

    /// <summary>
    /// Devuelve el nivel actual cargado en gameManager
    /// </summary>
    /// <returns></returns>
    public Level GetCurrLevel()
    {
        return currLevel;
    }

    /// <summary>
    /// Devuelve la categoría usada
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Devuelve todos las skins de colores disponibles en el gameManager
    /// </summary>
    /// <returns></returns>
    public List<ColorPack> GetThemes()
    {
        return colorThemes;
    }

    /// <summary>
    /// Devuelve el tema usado actualmente
    /// </summary>
    /// <returns></returns>
    public ColorPack GetCurrTheme()
    {
        return currTheme;
    }

    /// <summary>
    /// Cambia el tema
    /// </summary>
    /// <param name="t">El tema a cambiar</param>
    internal void SetTheme(ColorPack t)
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

    /// <summary>
    /// Devuelve todas las categorías correspondientes
    /// </summary>
    /// <returns></returns>
    public List<Category> GetCategories()
    {
        return categories;
    }
    
}