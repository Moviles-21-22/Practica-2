using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private AudioClip forward;
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Prefab del GO del titulo de las categorias que se quiere instanciar")] [SerializeField]
    private CategoryTitle catTitlePrefab;

    [Tooltip("Prefab del GO de los paquetes de nivel que se quiere instanciar")] [SerializeField]
    private LevelPackMenu levelPackPrefab;

    [Tooltip("Referencia al SelectLevelManager")]
    public SelectLevelManager selectLevel;

    [Header("Datos del juego")]
    [Header("Categorias y temas del juego")]
    [Tooltip("Recarga los datos del juego a sus valores por defecto")]
    [SerializeField]
    private bool reloadData;

    [Tooltip("Lista de los paquetes de las categorías del juego")] [SerializeField]
    private List<Category> categories;

    [Tooltip("Lista de los paquetes de temas del juego")] [SerializeField]
    private List<ColorPack> colorThemes;

    [Header("Elementos del UI")]
    [Tooltip("RectTransform usado como base para el tamaño de las categorías")]
    [SerializeField]
    private RectTransform baseRect;

    [Tooltip("Sección de las categorías en el UI")] [SerializeField]
    private RectTransform categorySection;

    [Tooltip("Referencia al contenido del scroll")] [SerializeField]
    private RectTransform contentScroll;

    [Tooltip("Parte superior del Menu")] [SerializeField]
    private RectTransform topMenu;

    [Tooltip("Canvas del mainMenu")] [SerializeField]
    private GameObject mainMenu;

    [Tooltip("Canvas del selectLevel")] [SerializeField]
    private GameObject selecLevelMenu;

    [Tooltip("Manager de los ads")]
    [SerializeField]
    private AdsManager adsManager;

    private GameManager gm;
    private List<GameManager.CategoryData> categoryData;

    /// <summary>
    /// Inicializa el MainMenu y todos sus elementos
    /// </summary>
    public void Init()
    {
        // Inicialización de los datos del juego
        gm = GameManager.instance;
        gm.LoadData(categories, colorThemes, reloadData);

        adsManager.Init();

        InitCategories();
        var colors = gm.GetCurrTheme();

        LoadPackage(0, 0);
        ChangeCanvas();
    }

    /// <summary>
    /// Calcula la altura de la UI del MainMenu que corresponde
    /// al espacio que muestra las diferentes categorías y niveles
    /// </summary>
    /// <returns>Devuelve la altura</returns>
    private float CalculateHeightCategories()
    {
        int numElems = categoryData.Count;
        if (numElems == 0)
        {
            Debug.LogError("No se han encontrado objetos asignados al componente");
        }

        foreach (var cat in categoryData)
        {
            numElems += cat.levels.Length;
        }

        return numElems * baseRect.rect.height;
    }

    /// <summary>
    /// Inicializa la información de cada una de las categorías de forma dinámica
    /// en función de los paquetes quue haya cargados en el GameManager
    /// </summary>
    private void InitCategories()
    {
        // Número de niveles dentro del paquete
        categoryData = gm.GetCategories();
        var numCategories = categoryData.Count;

        // Transformación del ContentScroll
        var originalH = categorySection.rect.height;
        var finalH = CalculateHeightCategories() - originalH;
        contentScroll.sizeDelta = new Vector2(0, finalH);
        var pos = contentScroll.position;
        pos.y = 0;
        contentScroll.position = pos;

        for (var i = 0; i < numCategories; i++)
        {
            var titleGo = Instantiate(catTitlePrefab, categorySection.transform);

            // Nombre y color de cada paquete
            titleGo.SetText(categories[i].name);
            titleGo.SetCategoryColor(categories[i].color);

            InitLevels(i);
        }

        baseRect.gameObject.SetActive(false);

        // Transformación de la parte superior del Menu
        originalH = topMenu.rect.height;
        topMenu.SetParent(contentScroll, false);
        var anchor = topMenu.anchorMin;
        anchor.y = 1.0f - (originalH / contentScroll.rect.height);
        topMenu.anchorMin = anchor;

        // Transformación de la sección de categorías
        categorySection.SetParent(contentScroll, false);
        anchor.x = 1;
        categorySection.anchorMax = anchor;
    }

    /// <summary>
    /// Inicializa la información de los paquetes de niveles de cada una de las categorías
    /// </summary>
    /// <param name="iCat">Índice de la categoría dentro de la lista de categorías</param>
    private void InitLevels(int iCat)
    {
        int numLevels = categoryData[iCat].levels.Length;
        // Inicializaci�n de cada nivel dentro de la categor�a
        for (var j = 0; j < numLevels; j++)
        {
            var levelPack = Instantiate(levelPackPrefab, categorySection.transform);

            // Color del nivel
            levelPack.SetPackColor(categories[iCat].color);

            // Nombre del nivel
            levelPack.SetPackName(categories[iCat].levels[j].levelName);

            // Niveles completados
            var newText = categoryData[iCat].levels[j].completedLevels +
                          "/" + categoryData[iCat].levels[j].levelsInfo.Length;
            levelPack.SetCompletedLevels(newText);

            // Logica del boton
            levelPack.AddCallBack(j, iCat, audioSource, forward, this);
        }
    }

    /// <summary>
    /// Le comunica al GameManager que se quiere cambiar de escena
    /// </summary>
    /// <param name="scene"></param>
    public void ChangeScene(int scene)
    {
        gm.LoadScene(scene);
    }

    /// <summary>
    /// Le comunica al GameManager que se quiere cargar un paquete de niveles
    /// </summary>
    /// <param name="indexPack">Índice de la lista de paquetes que se quiere cargar</param>
    /// <param name="indexCat">Índice de la lista de categorías que se quiere a cargar</param>
    public void LoadPackage(int indexPack, int indexCat)
    {
        ChangeCanvas();
        var catData = categoryData[indexCat];
        var lvlPackData = catData.levels[indexPack];
        gm.LoadPackage(lvlPackData, catData);

        // SelectLevelScene
        var category = categories[indexCat];
        var catColor = category.color;
        var lvlPack = category.levels[indexPack];
        selectLevel.Init(lvlPackData, lvlPack, catColor, this);
    }

    /// <summary>
    /// Alterna entre los dos Canvas del mainMenu
    /// </summary>
    public void ChangeCanvas()
    {
        if (selecLevelMenu.activeSelf)
        {
            selecLevelMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
        else
        {
            selecLevelMenu.SetActive(true);
            mainMenu.SetActive(false);
        }
    }

    public void LoadLevel(int lvl, LevelPack lvlPack)
    {
        gm.LoadLevel(lvl, lvlPack);
    }
}