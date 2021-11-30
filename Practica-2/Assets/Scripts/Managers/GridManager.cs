using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Tooltip("Referencia al título del paquete")]
    [SerializeField]
    private Text packTitle;

    [Tooltip("Referencia al contenido del scroll")]
    [SerializeField]
    private RectTransform contentScroll;

    [Tooltip("Paquete que contiene el grid de los niveles")]
    [SerializeField]
    private GridPack packPrefab;

    private Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    private void Start()
    {
        // Paquete de niveles que se va a cargar
        LevelPack currLevelPack = GameManager.instance.GetCurrentPack();
        packTitle.color = GameManager.instance.GetCurrentCategory().color;
        packTitle.text = currLevelPack.levelName;

        // Número de niveles dentro del paquete
        int numPacks = currLevelPack.gridNames.Length;

        // Ancho original del contentScroll
        var originalW = contentScroll.rect.width;
        Vector2 offset = contentScroll.offsetMax;

        for (int i = 0; i < numPacks; i++)
        {
            CreateGrid(currLevelPack, i, colors[i]);
            if (i < numPacks - 1)
            {
                offset.x += originalW;
                contentScroll.offsetMax = offset;
            }
        }
    }

    private void CreateGrid(LevelPack pack,int index,Color color)
    {
        GridPack currPack = Instantiate<GridPack>(packPrefab,contentScroll.transform);
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
