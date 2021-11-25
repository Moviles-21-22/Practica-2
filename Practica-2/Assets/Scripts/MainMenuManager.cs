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
        [Tooltip("Inserte la categoria empaquetada")]
        public Category categoria;
        [Tooltip("Referencia al sprite colorigo de la categoria")]
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
        /// Añade un callback para seleccionar el nivel y asociarlo al GameManager
        /// </summary>
        /// <param name="categoria">
        /// Categoria a la que está asociado el nivel
        /// </param>
        /// <param name="level">
        /// Indice para acceder al nivel correspondiente en la categoria
        /// </param>
        public void LoadLevelCallback(Category categoria, int level) 
        {
            button.onClick.AddListener(() => GameManager.instance.LoadLevel(categoria.levels[level]));
        }
    }

    private void Start()
    {
        // Inicializa toda la información correspondiente a cada paquete y a cada nivel para mostrarla en el Canvas
        for (int i = 0; i < paquetes.Count; i++)
        {
            // Nombre y color de cada paquete
            paquetes[i].titleText.text = paquetes[i].categoria.categoryName;
            paquetes[i].titleSprite.color = paquetes[i].categoria.color;
            // Inicialización de cada nivel dentro de la categoría
            for (int j = 0; j < paquetes[i].categoria.levels.Length; j++)
            {
                // Color del nivel
                paquetes[i].levels[j].name.color = paquetes[i].categoria.color;
                // Nombre del nivel
                paquetes[i].levels[j].name.text = paquetes[i].categoria.levels[j].levelName;
                // TODO: El 0 se sustituye por los niveles completos, que deberían estar guardados en GameManager
                // Niveles completados
                paquetes[i].levels[j].levels.text = "0 / " + paquetes[i].categoria.levels[j].totalLevels;
                // Logica del botón
                paquetes[i].levels[j].LoadLevelCallback(paquetes[i].categoria, j);
            }
        }
    }

}
