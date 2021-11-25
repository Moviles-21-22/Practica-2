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
        public Category categoria;
        public Image titleSprite;
        public Text titleText;
        public List<LevelProperties> levels;
    }

    [System.Serializable]
    struct LevelProperties 
    {
        public Button button;
        public Text name;
        public Text levels;

        public void LoadLevelCallback(Category categoria, int j) 
        {
            button.onClick.AddListener(() => GameManager.instance.LoadLevel(categoria.levels[j],categoria));
        }
    }

    private void Start()
    {
        for (int i = 0; i < paquetes.Count; i++)
        {
            paquetes[i].titleText.text = paquetes[i].categoria.categoryName;
            paquetes[i].titleSprite.color = paquetes[i].categoria.color;
            for (int j = 0; j < paquetes[i].categoria.levels.Length; j++)
            {
                paquetes[i].levels[j].name.color = paquetes[i].categoria.color;
                paquetes[i].levels[j].name.text = paquetes[i].categoria.levels[j].levelName;
                // TODO: El 0 se sustituye por los niveles completos, que deberían estar guardados en GameManager
                paquetes[i].levels[j].levels.text = "0 / " + paquetes[i].categoria.levels[j].totalLevels;
                paquetes[i].levels[j].LoadLevelCallback(paquetes[i].categoria, j);
            }
        }
    }

}
