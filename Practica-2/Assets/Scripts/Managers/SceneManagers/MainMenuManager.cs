using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable All

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Referencia al titulo de colores NIVELES")]
    public TitleColor title;
    
    [SerializeField] [Tooltip("Lista de los paquetes de niveles que existen en el juego")]
    private List<Paquete> paquetes;

    [SerializeField] [Tooltip("Referencia al Rect Transform del objeto padre de Categorías en MainMenu")]
    private RectTransform categRT;

    [SerializeField] private AudioClip forward;

    [SerializeField] private AudioSource audioSource;

    private GameManager gm;
    
    /// <summary>
    /// Lista de categorías disponibles
    /// </summary>
    private List<Category> categoriesList;
    
    /// <summary>
    /// Clase para agrupar las diferentes categorias
    /// </summary>
    [System.Serializable]
    private class Paquete
    {
        [Tooltip("Referencia al sprite colorido de la categoria")]
        public Image titleSprite;

        [Tooltip("Referencia al texto que indica el nombre de la categoria")]
        public Text titleText;

        [Tooltip("Referencia al RectTransform del título")]
        public RectTransform titleRect;

        [Tooltip("Lista de las propiedades de los niveles")]
        public List<LevelProperties> levels;
    }

    /// <summary>
    /// Elementos de cada categoria agrupados
    /// </summary>
    [System.Serializable]
    private struct LevelProperties
    {
        [Tooltip("Referencia al boton para elegir un nivel")]
        public Button button;

        [Tooltip("Referencia al texto del nombre del nivel")]
        public Text name;

        [Tooltip("Referencia al texto que muestra los niveles completados")]
        public Text levels;

        [Tooltip("Referencia al RectTransform del objeto correspondiente")]
        public RectTransform levelRect;

        public void CallBack(int lvl, int cat, AudioSource audioSource, AudioClip forward, MainMenuManager menu)
        {
            // El botón le comunica al mainMenu que se quiere cargar un nuevo paquete de niveles
            // de manera que sea el mainMenu quien se lo diga al GameManager
            Category category = menu.GetCategoriesList()[cat];
            LevelPack level = category.levels[lvl];
            button.onClick.AddListener(() => menu.LoadPackage(level, category));
            audioSource.PlayOneShot(forward);
        }
    }
    
    /// <summary>
    /// Inicializa las categorias del juego
    /// </summary>
    public void Init(List<Category> categories, List<Color> theme)
    {
        gm = GameManager.instance;
        categoriesList = categories;
        
        InitPackages();

        title.Init(theme);
    }

    /// <summary>
    /// Calcula la altura de la UI del MainMenu que corresponde
    /// al espacio que muestra las diferentes categorías y niveles
    /// </summary>
    /// <returns>Devuelve la altura</returns>
    private float CalculateHeightUI()
    {
        int numElems = paquetes.Count;
        if (numElems == 0)
        {
            Debug.LogError("No se han encontrado objetos asignados al componente");
        }

        for (int i = 0; i < categoriesList.Count; i++)
        {
            numElems += paquetes[i].levels.Count;
        }

        return categRT.rect.height / numElems;
    }

    private void InitPackages()
    {
        float canvasH = CalculateHeightUI();

        // Inicializa toda la informaci�n correspondiente a cada paquete y a cada nivel para mostrarla en el Canvas
        for (int i = 0; i < categoriesList.Count; i++)
        {
            // Nombre y color de cada paquete
            paquetes[i].titleText.text = categoriesList[i].categoryName;
            paquetes[i].titleSprite.color = categoriesList[i].color;
            
            // Tamaño común para todos
            var newSize = paquetes[i].titleRect.sizeDelta;
            newSize.y = canvasH;
            paquetes[i].titleRect.sizeDelta = newSize;

            InitLevels(i, newSize, canvasH);
        }
    }

    private void InitLevels(int i_cat, Vector2 newSize, float canvasH)
    {
        // Inicializaci�n de cada nivel dentro de la categor�a
        for (int j = 0; j < categoriesList[i_cat].levels.Length; j++)
        {
            // Color del nivel
            paquetes[i_cat].levels[j].name.color = categoriesList[i_cat].color;
            
            // Nombre del nivel
            paquetes[i_cat].levels[j].name.text = categoriesList[i_cat].levels[j].levelName;
            
            // Niveles completados
            paquetes[i_cat].levels[j].levels.text =
                categoriesList[i_cat].levels[j].completedLevels + "/" + categoriesList[i_cat].levels[j].levelsInfo.Count;
            
            // Logica del boton
            paquetes[i_cat].levels[j].CallBack(j, i_cat, audioSource, forward, this);

            // Tamaño común para todos
            newSize = paquetes[i_cat].levels[j].levelRect.sizeDelta;
            newSize.y = canvasH;
            paquetes[i_cat].levels[j].levelRect.sizeDelta = newSize;
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