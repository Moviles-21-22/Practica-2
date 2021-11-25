using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private Text categoryTitle;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private GameObject packPrefab;

    //  Numero de casillas en x
    private const int numX = 30;
    //  Numero de casillas en y
    private const int numY = 30;

    private Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    private List<Level> levels;

    //  Tamaño del contentScroll en w
    private float originalW;


    void Start()
    {
        LevelPack currLevelPack = GameManager.instance.GetCurrentLevel();
        categoryTitle.color = GameManager.instance.GetCurrentCategory().color;
        categoryTitle.text = currLevelPack.levelName;
        //Map map = new Map(currLevelPack.txt.ToString());
        //levels = map.GetAllLevels();
        int numPacks = currLevelPack.gridNames.Length;
        originalW = content.rect.width;
        float rect = content.rect.xMax;
        for (int i = 0; i < numPacks; i++)
        {
            CreateGrid();
            if (i < numPacks - 1)
            {
                rect += originalW;
                content.offsetMax = new Vector2(rect,content.offsetMax.y);
            }
        }
        //content.offsetMin = new Vector2(0,content.offsetMax.y);
    }

    private void CreateGrid()
    {
        var currPack = Instantiate(packPrefab,content.transform);
        Vector2 pos = new Vector2(1, 1);
        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                    
            }
        }
    }
}
