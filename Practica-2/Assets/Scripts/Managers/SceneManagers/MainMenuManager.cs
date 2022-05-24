using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Referencia al titulo de colores NIVELES")]
    public TitleColor title;

    [SerializeField] private AudioClip forward;

    [SerializeField] private AudioSource audioSource;

    [Tooltip("Prefab del GO del titulo de las categorias que se quiere instanciar")]
    [SerializeField]
    private CategoryTitle catTitlePrefab;

    [Tooltip("Prefab del GO de los paquetes de nivel que se quiere instanciar")]
    [SerializeField]
    private LevelPackMenu levelPackPrefab;

    [Tooltip("RectTransform usado como base para el tamaño de las categorías")]
    [SerializeField]
    private RectTransform baseRect;

    [Tooltip("Sección de las categorías")]
    [SerializeField]
    private RectTransform categorySection;

    [Tooltip("Referencia al contenido del scroll")]
    [SerializeField]
    private RectTransform contentScroll;


    [Tooltip("Parte superior del Menu")]
    [SerializeField]
    private RectTransform topMenu;

    private GameManager gm;

    /// <summary>
    /// Lista de categorías disponibles
    /// </summary>
    private List<Category> categoriesList;

    /// <summary>
    /// Inicializa las categorias del juego
    /// </summary>
    public void Init(List<Category> categories, List<Color> theme)
    {
        gm = GameManager.instance;
        categoriesList = categories;

        InitCategories();

        title.Init(theme);
    }

    /// <summary>
    /// Calcula la altura de la UI del MainMenu que corresponde
    /// al espacio que muestra las diferentes categorías y niveles
    /// </summary>
    /// <returns>Devuelve la altura</returns>
    private float CalculateHeightCategories()
    {
        int numElems = categoriesList.Count;
        if (numElems == 0)
        {
            Debug.LogError("No se han encontrado objetos asignados al componente");
        }

        for (int i = 0; i < categoriesList.Count; i++)
        {
            numElems += categoriesList[i].levels.Length;
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
        int numCategories = categoriesList.Count;

        // Transformación del ContentScroll
        float originalH = categorySection.rect.height;
        var finalH = CalculateHeightCategories() - originalH;
        contentScroll.sizeDelta = new Vector2(0, finalH);
        Vector3 pos = contentScroll.position;
        pos.y = 0;
        contentScroll.position = pos;

        for (int i = 0; i < numCategories; i++)
        {
            var title = Instantiate(catTitlePrefab, categorySection.transform);

            // Nombre y color de cada paquete
            title.SetText(categoriesList[i].categoryName);
            title.SetCategoryColor(categoriesList[i].color);

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
    /// <param name="i_cat">Índice de la categoría dentro de la lista de categorías</param>
    private void InitLevels(int i_cat)
    {
        // Inicializaci�n de cada nivel dentro de la categor�a
        for (int j = 0; j < categoriesList[i_cat].levels.Length; j++)
        {
            var levelPack = Instantiate(levelPackPrefab, categorySection.transform);

            // Color del nivel
            levelPack.SetPackColor(categoriesList[i_cat].color);

            // Nombre del nivel
            levelPack.SetPackName(categoriesList[i_cat].levels[j].levelName);

            // Niveles completados
            var newText = categoriesList[i_cat].levels[j].completedLevels +
                "/" + categoriesList[i_cat].levels[j].levelsInfo.Count;
            levelPack.SetCompletedLevels(newText);

            // Logica del boton
            levelPack.AddCallBack(j, i_cat, audioSource, forward, this);
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
    /// Le comunica al GameManager que se quiere cargar un paquete
    /// </summary>
    /// <param name="level"></param>
    /// <param name="category"></param>
    public void LoadPackage(LevelPack level, Category category)
    {
        gm.LoadPackage(level, category);
    }

    public List<Category> GetCategoriesList()
    {
        return categoriesList;
    }
}