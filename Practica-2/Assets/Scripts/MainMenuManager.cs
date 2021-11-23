using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private List<Paquete> paquetes;

    [System.Serializable]
    class Paquete 
    {
        public Color color;
        public string routePaquete;
        public Image titleSprite;
        public Text titleText;
        public List<LevelProperties> levels;
    }

    [System.Serializable]
    struct LevelProperties 
    {
        public Text name;
        public Text levels;
    }

    private void Start()
    {
        //TODO: Hacerlo por datos del fichero de texto

        for (int i = 0; i < paquetes.Count; i++) 
        {
            paquetes[i].titleText.text = "Intro";
            paquetes[i].titleSprite.color = paquetes[i].color;
            for (int j = 0; j < paquetes[i].levels.Count; j++) 
            {
                paquetes[i].levels[j].name.color = paquetes[i].color;
                paquetes[i].levels[j].name.text = "Nombre";
                paquetes[i].levels[j].levels.text= "0/150";
            }
        }
    }
}
