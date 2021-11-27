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
    private GridPack packPrefab;

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
        LevelPack currLevelPack = GameManager.instance.GetCurrentPack();
        categoryTitle.color = GameManager.instance.GetCurrentCategory().color;
        categoryTitle.text = currLevelPack.levelName;
        int numPacks = currLevelPack.gridNames.Length;
        originalW = content.rect.width;
        float rect = content.rect.xMax;
        for (int i = 0; i < numPacks; i++)
        {
            CreateGrid(currLevelPack,i,colors[i]);
            if (i < numPacks - 1)
            {
                rect += originalW;
                content.offsetMax = new Vector2(rect,content.offsetMax.y);
            }
        }
    }

    private void CreateGrid(LevelPack pack,int index,Color color)
    {
        GridPack currPack = Instantiate<GridPack>(packPrefab,content.transform);
        currPack.SetText(pack.gridNames[index]);
        Box[] boxes = currPack.GetAllBoxes();

        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].SetCallBack((index * boxes.Length) + i);
            boxes[i].SetLevelNum(i + 1);
            boxes[i].initBox(color);
        }
    }
}
