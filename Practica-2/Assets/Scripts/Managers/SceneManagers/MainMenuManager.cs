using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Referencia al SelectLevelManager")]
    public SelectLevelManager selectLevel;

    [Tooltip("Manager de los ads")] [SerializeField]
    private AdsManager adsManager;

    [Tooltip("Canvas del mainMenu")] [SerializeField]
    private GameObject mainMenu;

    [Tooltip("Canvas del selectLevel")] [SerializeField]
    private GameObject selecLevelMenu;
    
    [Header("Efectos")] [SerializeField] private AudioClip forward;
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Texto a escribir como titulo")] [SerializeField]
    private string textTittle = "NIVELES";

    [Tooltip("Tiempo de refresco del rótulo niveles")] [SerializeField] [Min(0.2f)]
    private float nivelesTime;

    [Tooltip("Referencia al rótulo de colores de niveles")] [SerializeField]
    private TextMeshProUGUI menuTitle;
    
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
    [Tooltip("Prefab del GO del titulo de las categorias que se quiere instanciar")]
    [SerializeField]
    private CategoryTitle catTitlePrefab;

    [Tooltip("Prefab del GO de los paquetes de nivel que se quiere instanciar")] [SerializeField]
    private LevelPackMenu levelPackPrefab;

    [Tooltip("RectTransform usado como base para el tamaño de las categorías")] [SerializeField]
    private RectTransform baseRect;

    [Tooltip("Sección de las categorías en el UI")] [SerializeField]
    private RectTransform categorySection;

    [Tooltip("Referencia al contenido del scroll")] [SerializeField]
    private RectTransform contentScroll;

    [Tooltip("Parte superior del Menu")] [SerializeField]
    private RectTransform topMenu;

    [Tooltip("Parte superior del Menu")] [SerializeField]
    private VerticalLayoutGroup catLayout;
    
    /// <summary>
    /// Index para determinar en qué letra del rótulo estamos
    /// </summary>
    private uint indexNiveles;

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

        LoadPackage(0, 0);
        ChangeCanvas();

        InvokeRepeating(nameof(TitleColor), 0, nivelesTime);
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

        var height = numElems * baseRect.rect.height;
        baseRect.gameObject.SetActive(false);
        return height;
    }

    private void ResizeUI()
    {
        // 1. Calculo del tamaño que van a ocupar sólo las categorías
        var catHeight = CalculateHeightCategories();

        //2. Cálculo del offset que se la aplicará al contentScroll
        var scrollH = contentScroll.rect.height;
        var finalH = topMenu.rect.height + catHeight;
        var offset = (finalH - scrollH) / scrollH;
        var contentAnchor = Vector2.down * offset;

        // 3. Asignación nueva
        contentScroll.anchorMin = Vector2.up * contentAnchor;
        topMenu.SetParent(contentScroll);
        categorySection.SetParent(contentScroll);
        categorySection.sizeDelta = Vector2.up * catHeight;
    }

    /// <summary>
    /// Inicializa la información de cada una de las categorías de forma dinámica
    /// en función de los paquetes quue haya cargados en el GameManager
    /// </summary>
    private void InitCategories()
    {
        // Número de niveles dentro del paquete
        categoryData = gm.GetCategories();
        // 1. Redimensión dle UI
        var originalH = baseRect.rect.height;
        ResizeUI();

        var numCategories = categoryData.Count;

        // 2. Generación de los paquetes de niveles
        var catList = new List<CategoryTitle>();
        var lvlPackList = new List<LevelPackMenu>();
        for (var i = 0; i < numCategories; i++)
        {
            var titleGo = Instantiate(catTitlePrefab, categorySection.transform);
            // Nombre y color de cada paquete
            titleGo.titleText.text = categories[i].name;
            titleGo.titleSprite.color = categories[i].color;

            InitLevels(i, ref lvlPackList);
            
            catList.Add(titleGo);
        }

        // 3. Reescalado de los paquetes de niveles
        catLayout.childControlHeight = false;
        foreach (var cat in catList)
        {
            cat.titleRect.sizeDelta = Vector2.up * originalH;
        }

        foreach (var pack in lvlPackList)
        {
            pack.levelRect.sizeDelta = Vector2.up * originalH;
        }
    }

    /// <summary>
    /// Inicializa la información de los paquetes de niveles de cada una de las categorías
    /// </summary>
    /// <param name="iCat">Índice de la categoría dentro de la lista de categorías</param>
    /// <param name="packList">Lista de los paquetes del juego</param>
    private void InitLevels(int iCat, ref List<LevelPackMenu> packList)
    {
        int numLevels = categoryData[iCat].levels.Length;
        // Inicializaci�n de cada nivel dentro de la categor�a
        for (var j = 0; j < numLevels; j++)
        {
            var levelPack = Instantiate(levelPackPrefab, categorySection.transform);

            // Color del nivel
            levelPack.packName.color = categories[iCat].color;

            // Nombre del nivel
            levelPack.packName.text = categories[iCat].levels[j].levelName;

            // Niveles completados
            var newText = categoryData[iCat].levels[j].completedLevels +
                          "/" + categoryData[iCat].levels[j].levelsInfo.Length;
            levelPack.levels.text = newText;

            // Logica del boton
            levelPack.AddCallBack(j, iCat, audioSource, forward, this);
            packList.Add(levelPack);
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
        gm.LoadPackage(indexPack, indexCat);

        // SelectLevelScene
        var category = categories[indexCat];
        var catColor = category.color;
        var lvlPack = category.levels[indexPack];
        var catData = categoryData[indexCat];
        var lvlPackData = catData.levels[indexPack];
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

    /// <summary>
    /// Muestra el titulo animado
    /// </summary>
    public void TitleColor()
    {
        var currColors = gm.GetCurrentColorPack();
        menuTitle.text = "";

        for (int i = 0; i < textTittle.Length; i++)
        {
            menuTitle.text += "<color=#" + ColorUtility.ToHtmlStringRGBA(currColors[i + (int) indexNiveles]) + ">" +
                              textTittle[i] + "</color>";
        }

        indexNiveles = indexNiveles >= textTittle.Length ? 0 : indexNiveles + 1;
    }
}