using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Check Amaro

//  Se encarga de gestionar la selección de niveles
public class SelectLevelManager : MonoBehaviour
{
    [Tooltip("Referencia al título del paquete")] [SerializeField]
    private TextMeshProUGUI packTitle;

    [Tooltip("Referencia al contenido del scroll")] [SerializeField]
    private RectTransform contentScroll;

    [Tooltip("Paquete que contiene el grid de los niveles")] [SerializeField]
    private GridPack packPrefab;

    // Actual pack cargado
    private GameManager.LevelPackData currLevelPack;

    //  Colores para los bloques
    private readonly Color[] colors = {Color.red, Color.blue, Color.green, Color.cyan, Color.magenta};

    //  Para enumerar el grid
    private bool splitLevels;

    //  Para determinar si el pack está bloqueado
    private bool lockPack;

    //  Niveles completados del bloque
    private int completedLevels;

    //  Offset original del content scroll
    private Vector2 originalOffset;

    //  Referencia al mainMenu
    private MainMenuManager mainMenu;

    //  Para determinar si se ha inicializado la menu
    private bool initialized;

    //  Lista de gridPacks
    private List<GridPack> gridList;

    //  Actual pack en uso
    private LevelPack currPack;

    /// <summary>
    /// Inicializa el gestor del SelectLevel
    /// </summary>
    /// <param name="lvlPackData">Datos del paquete de niveles que se ha cargado</param>
    /// <param name="lvlPack">Paquete de niveles ScriptableObject</param>
    /// <param name="catColor">Color del texto que muestra la categoría</param>
    /// <param name="man">Referencia al manager de la escena MainMenu</param>
    public void Init(GameManager.LevelPackData lvlPackData, LevelPack lvlPack, Color catColor, MainMenuManager man)
    {
        mainMenu = man;
        currLevelPack = lvlPackData;
        packTitle.color = catColor;
        currPack = lvlPack;
        InitGridData();
        if (!initialized)
        {
            gridList = new List<GridPack>();
            GeneratePackageLevels();
            initialized = true;
        }
        else
        {
            ChangeGridInfo();
        }
    }
    
//-------------------------------------------------INICIALIZACION-----------------------------------------------------//

    /// <summary>
    /// Inicializa los datos del grid
    /// </summary>
    private void InitGridData()
    {
        packTitle.text = currPack.levelName;
        splitLevels = currPack.splitLevels;
        lockPack = currPack.lockPack;
        completedLevels = currLevelPack.completedLevels;
    }

    /// <summary>
    /// Genera los packs para los niveles
    /// </summary>
    private void GeneratePackageLevels()
    {
        // Número de niveles dentro del paquete
        int numPacks = currPack.gridNames.Length;

        // Ancho original del contentScroll
        var originalW = contentScroll.rect.width;
        originalOffset = contentScroll.offsetMax;
        var offset = originalOffset;

        for (int i = 0; i < numPacks; i++)
        {
            gridList.Add(CreateGrid(currPack, i, colors[i]));
            if (i < numPacks - 1)
            {
                offset.x += originalW;
                contentScroll.offsetMax = offset;
            }
        }
    }

    /// <summary>
    /// Crea el grid de forma dinámica en función del pack cargado
    /// </summary>
    /// <param name="lvlPack">Paquete de niveles que se va a usar</param>
    /// <param name="index">Indice del pack</param>
    /// <param name="color">Color del bloque</param>
    private GridPack CreateGrid(LevelPack lvlPack, int index, Color color)
    {
        GridPack gridPack = Instantiate(packPrefab, contentScroll.transform);
        gridPack.SetText(lvlPack.gridNames[index]);
        CellLevel[] boxes = gridPack.GetAllBoxes();

        for (int j = 0; j < boxes.Length; j++)
        {
            if (splitLevels)
            {
                boxes[j].SetLevelNum(j + 1);
            }
            else
            {
                boxes[j].SetLevelNum((index * boxes.Length) + j + 1);
            }

            Levels.LevelState levelState = currLevelPack.levelsInfo[(index * boxes.Length) + j].state;
            boxes[j].InitBox(color, levelState, this);

            if (!lockPack ||
                lockPack && (index * boxes.Length) + j <= completedLevels)
                // Desbloquea los niveles hasta dejar el primero sin desbloquear
            {
                boxes[j].SetCallBack((index * boxes.Length) + j);
                if ((index * boxes.Length) + j == completedLevels)
                {
                    boxes[j].CurrentLevel();
                }
            }
            else
            {
                boxes[j].ActiveLockImage();
            }
        }

        return gridPack;
    }

    /// <summary>
    /// Modifica la información del menú
    /// </summary>
    private void ChangeGridInfo()
    {
        int index = 0;
        foreach (var pack in gridList)
        {
            pack.SetText(currPack.gridNames[index]);
            CellLevel[] boxes = pack.GetAllBoxes();
            for (int j = 0; j < boxes.Length; j++)
            {
                boxes[j].Reset();
                if (splitLevels)
                {
                    boxes[j].SetLevelNum(j + 1);
                }
                else
                {
                    boxes[j].SetLevelNum((index * boxes.Length) + j + 1);
                }

                var levelState = currLevelPack.levelsInfo[(index * boxes.Length) + j].state;
                boxes[j].InitBox(colors[index], levelState, this);

                if (!lockPack || (lockPack && (index * boxes.Length) + j <= completedLevels))
                    // Desbloquea los niveles hasta dejar el primero sin desbloquear
                {
                    boxes[j].SetCallBack((index * boxes.Length) + j);
                    if ((index * boxes.Length) + j == completedLevels)
                    {
                        boxes[j].CurrentLevel();
                    }
                }
                else
                {
                    boxes[j].ActiveLockImage();
                }
            }

            index++;
        }
    }

//----------------------------------------------------CALLBACKS-------------------------------------------------------//
    /// <summary>
    /// Callback para volver al mainMenu
    /// </summary>
    public void BackToMainMenu()
    {
        mainMenu.ChangeCanvas();
    }

    /// <summary>
    /// Carga una nivel
    /// </summary>
    /// <param name="lvl">nivel a cargar</param>
    public void LoadLevel(int lvl)
    {
        mainMenu.LoadLevel(lvl, currPack);
    }
}