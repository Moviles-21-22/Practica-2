using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Lista de los paquetes de niveles que existen en el juego")]
    private List<Paquete> paquetes;

    [System.Serializable]
    private class Paquete 
    {
        //[Tooltip("Inserte la categoria empaquetada")]
        //private Category categoria;
        [Tooltip("Referencia al sprite colorido de la categoria")]
        public Image titleSprite;
        [Tooltip("Referencia al texto que indica el nombre de la categoria")]
        public Text titleText;
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
            button.onClick.AddListener(() => GameManager.instance.LoadPackage(categoria.levels[level],categoria));
        }
    }

    private void Start()
    {
        GameManager gm = GameManager.instance;
        List<Category> cats = gm.GetCategories();

        for (int i = 0; i < cats.Count; i++)
        {
            paquetes[i].titleText.text = cats[i].categoryName;
            paquetes[i].titleSprite.color = cats[i].color;
            for (int j = 0; j < cats[i].levels.Length; j++)
            {
                paquetes[i].levels[j].name.color = cats[i].color;
                paquetes[i].levels[j].name.text = cats[i].levels[j].levelName;
                paquetes[i].levels[j].levels.text = cats[i].levels[j].completedLevels + "/" + cats[i].levels[j].levelsInfo.Count;
                paquetes[i].levels[j].LoadLevelCallback(cats[i], j);
            }
        }

        //// Inicializa toda la informaci�n correspondiente a cada paquete y a cada nivel para mostrarla en el Canvas
        //for (int i = 0; i < paquetes.Count; i++)
        //{
        //    paquetes[i].categoria = gm.GetCategories()[i];
        //    // Nombre y color de cada paquete
        //    paquetes[i].titleText.text = paquetes[i].categoria.categoryName;
        //    paquetes[i].titleSprite.color = paquetes[i].categoria.color;
        //    // Inicializaci�n de cada nivel dentro de la categor�a
        //    for (int j = 0; j < paquetes[i].categoria.levels.Length; j++)
        //    {
        //        // Color del nivel
        //        paquetes[i].levels[j].name.color = paquetes[i].categoria.color;
        //        // Nombre del nivel
        //        paquetes[i].levels[j].name.text = paquetes[i].categoria.levels[j].levelName;
        //        // Niveles completados
        //        paquetes[i].levels[j].levels.text = paquetes[i].categoria.levels[j].completedLevels + "/" + paquetes[i].categoria.levels[j].levelsInfo.Count;
        //        // Logica del bot�n
        //        paquetes[i].levels[j].LoadLevelCallback(paquetes[i].categoria, j);
        //    }
        //}
    }

}
