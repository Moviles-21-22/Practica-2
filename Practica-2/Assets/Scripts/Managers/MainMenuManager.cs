using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Lista de los paquetes de niveles que existen en el juego")]
    private List<Paquete> paquetes;

    public RectTransform tr;

    [SerializeField]
    private AudioClip forward;

    [SerializeField]
    private AudioSource audioSource; 

    [System.Serializable]
    private class Paquete 
    {
        //[Tooltip("Inserte la categoria empaquetada")]
        //private Category categoria;
        [Tooltip("Referencia al sprite colorido de la categoria")]
        public Image titleSprite;
        [Tooltip("Referencia al texto que indica el nombre de la categoria")]
        public Text titleText;
        [Tooltip("Referencia al RectTransform del título")]
        public RectTransform titleRect;
        [Tooltip("Lista de las propiedades de los niveles")]
        public List<LevelProperties> levels;
    }

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
    }

    private void Start()
    {
        GameManager gm = GameManager.instance;
        List<Category> cats = gm.GetCategories();

        var b = tr;
        int numElems = paquetes.Count;
        for (int i = 0; i < cats.Count; i++) 
        {
            numElems += paquetes[i].levels.Count;
        }

        float height = tr.rect.height / numElems;

        // Inicializa toda la informaci�n correspondiente a cada paquete y a cada nivel para mostrarla en el Canvas
        for (int i = 0; i < cats.Count; i++)
        {
            // Nombre y color de cada paquete
            paquetes[i].titleText.text = cats[i].categoryName;
            paquetes[i].titleSprite.color = cats[i].color;
            // Tamaño común para todos
            var aux = paquetes[i].titleRect.sizeDelta;
            aux.y = height;
            paquetes[i].titleRect.sizeDelta = aux;

            // Inicializaci�n de cada nivel dentro de la categor�a
            for (int j = 0; j < cats[i].levels.Length; j++)
            {
                // Color del nivel
                paquetes[i].levels[j].name.color = cats[i].color;
                // Nombre del nivel
                paquetes[i].levels[j].name.text = cats[i].levels[j].levelName;
                // Niveles completados
                paquetes[i].levels[j].levels.text = cats[i].levels[j].completedLevels + "/" + cats[i].levels[j].levelsInfo.Count;
                // Logica del boton
                paquetes[i].levels[j].LoadLevelCallback(cats[i], j);
                paquetes[i].levels[j].button.onClick.AddListener(() => audioSource.PlayOneShot(forward));

                // Tamaño común para todos
                aux = paquetes[i].levels[j].levelRect.sizeDelta;
                aux.y = height;
                paquetes[i].levels[j].levelRect.sizeDelta = aux;
            }
        }        
    }
}
