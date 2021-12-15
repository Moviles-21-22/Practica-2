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

    private LevelPack currLevelPack;

    private Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    private bool splitLevels = false;
    private bool lockPack = false;
    private int completedLevels = 0;

    private void Start()
    {
        // Paquete de niveles que se va a cargar
        currLevelPack = GameManager.instance.GetCurrentPack();
        packTitle.color = GameManager.instance.GetCurrentCategory().color;
        packTitle.text = currLevelPack.levelName;
        splitLevels = currLevelPack.splitLevels;
        lockPack = currLevelPack.lockPack;
        completedLevels = currLevelPack.completedLevels;

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

    private void CreateGrid(LevelPack pack, int index, Color color)
    {
        GridPack currPack = Instantiate<GridPack>(packPrefab, contentScroll.transform);
        currPack.SetText(pack.gridNames[index]);
        Box[] boxes = currPack.GetAllBoxes();

        for (int i = 0; i < boxes.Length; i++)
        {
            if (splitLevels)
            {
                boxes[i].SetLevelNum(i + 1);
            }
            else
            {
                boxes[i].SetLevelNum((index * boxes.Length) + i + 1);
            }

            bool perfect = currLevelPack.levelsInfo[(index * boxes.Length) + i].perfect;
            bool completed = currLevelPack.levelsInfo[(index * boxes.Length) + i].completed;

            boxes[i].InitBox(color, perfect, completed);

            if (!lockPack || lockPack && (index * boxes.Length) + i <= completedLevels)   // Desbloquea los niveles hasta dejar el primero sin hacer desbloqueado
            {
                boxes[i].SetCallBack((index * boxes.Length) + i);
                if ((index * boxes.Length) + i == completedLevels)
                {
                    boxes[i].CurrentLevel();
                }
            }
            else
            {
                boxes[i].ActiveLockImage();
            }
        }
    }
}
