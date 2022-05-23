using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable All

public class MainMenuManager : MonoBehaviour
{
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
    private List<Category> cats;
    
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

        /// <summary>
        /// A�ade un callback para seleccionar el nivel y asociarlo al GameManager
        /// </summary>
        /// <param name="categoria">
        /// Categoria a la que est� asociado el nivel
        /// </param>
        /// <param name="level">
        /// Indice para acceder al nivel correspondiente en la categoria
        /// </param>
        public void LoadLevelCallback(Category categoria, int level)
        {
            button.onClick.AddListener(() => GameManager.instance.LoadPackage(categoria.levels[level], categoria));
        }

        public void CallBack(int lvl, int cat)
        {
            Category category = GameManager.instance.GetCategories()[cat];
            LevelPack level = category.levels[lvl];
            button.onClick.AddListener(() => GameManager.instance.LoadPackage(level, category));
        }
    }
    
    /// <summary>
    /// Inicializa las categorias del juego
    /// </summary>
    public void Init()
    {
        gm = GameManager.instance;
        cats = gm.GetCategories();
        
        InitPackages();
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

        for (int i = 0; i < cats.Count; i++)
        {
            numElems += paquetes[i].levels.Count;
        }

        return categRT.rect.height / numElems;
    }

    private void InitPackages()
    {
        float canvasH = CalculateHeightUI();

        // Inicializa toda la informaci�n correspondiente a cada paquete y a cada nivel para mostrarla en el Canvas
        for (int i = 0; i < cats.Count; i++)
        {
            // Nombre y color de cada paquete
            paquetes[i].titleText.text = cats[i].categoryName;
            paquetes[i].titleSprite.color = cats[i].color;
            
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
        for (int j = 0; j < cats[i_cat].levels.Length; j++)
        {
            // Color del nivel
            paquetes[i_cat].levels[j].name.color = cats[i_cat].color;
            
            // Nombre del nivel
            paquetes[i_cat].levels[j].name.text = cats[i_cat].levels[j].levelName;
            
            // Niveles completados
            paquetes[i_cat].levels[j].levels.text =
                cats[i_cat].levels[j].completedLevels + "/" + cats[i_cat].levels[j].levelsInfo.Count;
            
            // Logica del boton
            paquetes[i_cat].levels[j].CallBack(j, i_cat);
            paquetes[i_cat].levels[j].button.onClick.AddListener(() => audioSource.PlayOneShot(forward));

            // Tamaño común para todos
            newSize = paquetes[i_cat].levels[j].levelRect.sizeDelta;
            newSize.y = canvasH;
            paquetes[i_cat].levels[j].levelRect.sizeDelta = newSize;
        }
    }
}