using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class HUDManager : MonoBehaviour
{
    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")]
    [SerializeField]
    private GameObject winInfo;

    [Tooltip("Referencia a la imagen de la estrella cuando se gana")]
    [SerializeField]
    private RawImage winStar;

    [Tooltip("Sprites que se usan para el icono de ganar")]
    [SerializeField]
    private Texture[] winSprites;

    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")]
    [SerializeField]
    private HUD_Element completedLevelIcon;

    [Tooltip("Referencia al objeto que se encarga de deshacer movimientos")]
    [SerializeField]
    private HUD_Element undoElement;

    [Tooltip("Referencia al objeto que regresa al nivel anterior")]
    [SerializeField]
    private HUD_Element prevLevel;

    [Tooltip("Referencia al objeto que pasa al nivel posterior")]
    [SerializeField]
    private HUD_Element nextLevel;

    [Tooltip("Referencia al objeto que muestra las pistas")]
    [SerializeField]
    private HUD_Element hints;

    [Tooltip("Referencia al botón de próximo nivel al ganar el juego")]
    [SerializeField]
    private HUD_Element nextLevelWin;

    [Header("Textos")]
    [Tooltip("Referencia al texto que muestra el nivel actual")]
    [SerializeField]
    private Text numLevelText;
    [Tooltip("Referencia al texto que muestra el tamaño del grid")]
    [SerializeField]
    private Text numFlowsText;
    [Tooltip("Referencia al texto que muestra el número de movimientos")]
    [SerializeField]
    private Text numPasosText;
    [Tooltip("Referencia al texto que muestra el récord actual")]
    [SerializeField]
    private Text recordText;
    [Tooltip("Referencia al texto que muestra el porcentaje completado")]
    [SerializeField]
    private Text percentageText;
    [Tooltip("Referencia al titulo del texto que se muestra al ganar")]
    [SerializeField]
    private Text winTitle;
    [Tooltip("Referencia al texto final que muestra el número de movimientos")]
    [SerializeField]
    private Text finalMovsText;

    [Header("Botones")]
    [Tooltip("Referencia al botón de volver atrás")]
    [SerializeField]
    private Button backButton;
    [Tooltip("Referencia al botón de reiniciar nivel")]
    [SerializeField]
    private Button restartButton;
    [Tooltip("Referencia al botón para agregar pistas")]
    [SerializeField]
    private Button addHintsButton;

    // Referencia al LevelManager
    private LevelManager lvlMan;
    
    
    //Mapa del nivel cargado
    private Map map;
    // Numero de pistas disponibles
    private int currHints;
    //Paquete de niveles cargado
    private LevelPack levelPack;
    //Nivel cargado
    private Level currLevel;
    //Variable de tiempo usada para procesar las animaciones
    private float timer = 0.0f;
    //Numero de tuberías conectadas
    private int currentFlows = 0;
    //Numero de pasos
    private int currentMovs = 0;

    [System.Serializable]
    class HUD_Element
    {
        [Tooltip("Diferentes sprites del elemento de HUD")]
        public Sprite[] elementSprites;

        [Tooltip("Referencia al componente Image del elemento del HUD")]
        public Image elementImage;

        [Tooltip("Referencia al componente Text del elemento del HUD")]
        public Text elementText;

        [Tooltip("Referencia al componente Button del elemento del HUD")]
        public Button elementButton;
    }

    public void Init(Map currMap, Level lvl, LevelPack package,  int numHints, LevelManager lvlManager)
    {
        lvlMan = lvlManager;
        currHints = numHints;
        InitHudData(currMap, lvl, package);

        InitTopElements();
        InitMidElements();
        InitBotElements();
    }

    /// <summary>
    /// Inicializa los datos generales del juego
    /// </summary>
    private void InitHudData(Map currMap, Level lvl, LevelPack package)
    {
        map = currMap;
        currLevel = lvl;
        levelPack = package;
        numLevelText.text = "Nivel " + (currLevel.lvl + 1);
    }

    /// <summary>
    /// Inicializa la parte superior del hud
    /// </summary>
    private void InitTopElements()
    {
        //============TITULO-DEL-NIVEL===================//
        completedLevelIcon.elementText.text = currLevel.numBoardX + "x" + currLevel.numBoardY;
        if (levelPack.levelsInfo[currLevel.lvl].perfect)
        {
            completedLevelIcon.elementImage.enabled = true;
            completedLevelIcon.elementImage.sprite = completedLevelIcon.elementSprites[0];
        }
        else if (levelPack.levelsInfo[currLevel.lvl].completed)
        {
            completedLevelIcon.elementImage.enabled = true;
            completedLevelIcon.elementImage.sprite = completedLevelIcon.elementSprites[1];
        }

        //============VOLVER===================//
        backButton.onClick.AddListener(() => lvlMan.LoadScene(GameManager.SceneOrder.LEVEL_SELECT));

        //============TABLERO-INFO===================//
        numFlowsText.text = "flujos: " + 0 + "/" + currLevel.numFlow;
        numPasosText.text = "pasos: " + 0;
        recordText.text = "récord: " + levelPack.records[currLevel.lvl];
    }

    /// <summary>
    /// Inicializa la parte del medio del hud
    /// </summary>
    private void InitMidElements()
    {
        // Proximo nivel
        nextLevelWin.elementButton.onClick.AddListener(() => lvlMan.ChangeLevel(currLevel.lvl + 1));
        addHintsButton.onClick.AddListener(() => lvlMan.AddHints(1));
        addHintsButton.onClick.AddListener(UpdateHintText);
    }

    /// <summary>
    /// Inicializa la parte inferior del hud
    /// </summary>
    private void InitBotElements()
    {
        // Reinicia el nivel
        restartButton.onClick.AddListener(() => lvlMan.LoadScene(GameManager.SceneOrder.GAME_SCENE));

        //============PISTAS===================//
        hints.elementText.text = currHints + "x";
        if (currHints == 0)
        {
            hints.elementImage.sprite = hints.elementSprites[0];
        }
        else
        {
            hints.elementImage.sprite = hints.elementSprites[1];
        }

        //============NIVEL-ANTERIOR===================//
        if (currLevel.lvl == 0)
        {
            prevLevel.elementImage.sprite = prevLevel.elementSprites[0];
        }
        else
        {
            prevLevel.elementImage.sprite = prevLevel.elementSprites[1];
            prevLevel.elementButton.onClick.AddListener(() => lvlMan.ChangeLevel(currLevel.lvl - 1));
        }

        //============NIVEL-POSTERIOR===================//
        if (currLevel.lvl + 1 == levelPack.levelsInfo.Count)
        {
            nextLevel.elementImage.sprite = nextLevel.elementSprites[0];
        }
        else
        {
            nextLevel.elementImage.sprite = nextLevel.elementSprites[1];
            nextLevel.elementButton.onClick.AddListener(() => lvlMan.ChangeLevel(currLevel.lvl + 1));
        }
    }

    /// <summary>
    /// Cambia el texto que muestra las pistas en función de las pistas que queden
    /// </summary>
    public void UpdateHintText()
    {
        hints.elementText.text = currHints.ToString() + "x";
        if (currHints == 0)
        {
            hints.elementImage.sprite = hints.elementSprites[0];
        }
        else
        {
            hints.elementImage.sprite = hints.elementSprites[1];
        }
    }

    /// <summary>
    /// Lógica que procesa el comportamiento al completar un nivel
    /// </summary>
    /// <param name="perfect">Determina si el nivel es perfecto o no</param>
    public void LevelCompleted(bool perfect)
    {
        //winPerfect = perfect;
        winStar.enabled = true;
        // Si es una solución con movimientos perfectos sale una estrella grande
        if (perfect)
        {
            winStar.texture = winSprites[0];
            winTitle.text = "¡Perfecto!";
        }
        else
        {
            // Si es una solución normal aparece un tic
            winStar.texture = winSprites[1];
            winTitle.text = "¡Completado!";
        }
        finalMovsText.text = "Completaste el nivel\n" + "con " + currentMovs + " pasos";

        // El último nivel
        if (currLevel.lvl + 1 == levelPack.levelsInfo.Count)
        {
            winTitle.text = "¡Felicitaciones!";
            finalMovsText.text = "Has llegado al final del\n" + levelPack.name;

            // Nos lleva al mainMenu
            nextLevelWin.elementButton.onClick.RemoveAllListeners();
            nextLevelWin.elementButton.onClick.AddListener(() => lvlMan.LoadScene((int)GameManager.SceneOrder.MAIN_MENU));
            nextLevelWin.elementText.text = "elige el próximo paquete";
        }

        InvokeRepeating(nameof(WinAnimation), 0.0f, 0.05f);
    }

    /// <summary>
    /// Animación de aparición de la estrella al ganar
    /// </summary>
    private void WinAnimation()
    {
        timer += 0.05f;
        if (timer < 0.5f)
        {
            var color = winStar.color;
            color.a += 0.1f;
            winStar.color = color;
        }
        else if (timer < 1.0f)
        {
            var color = winStar.color;
            color.a -= 0.1f;
            winStar.color = color;
        }
        else if (timer >= 1.0f)
        {
            winStar.enabled = false;
            winInfo.SetActive(true);
            CancelInvoke();
        }
    }

    /// <summary>
    /// Muestra la parte entera del porcentaje que hay resuelto del tablero
    /// </summary>
    public void ShowPercentage(int percentage)
    {
        percentageText.text = "tubería: " + (percentage).ToString() + "%";
    }

    /// <summary>
    /// Añade o quita un flujo al contador de flujos completos
    /// </summary>
    public void ShowFlows(int flows)
    {
        currentFlows = flows;
        numFlowsText.text = "flujos: " + currentFlows + "/" + currLevel.numFlow;
    }

    /// <summary>
    /// Añade un nuevo movimiento al contador de pasos del canvas
    /// </summary>
    public void ShowMovements(int movs)
    {
        currentMovs = movs;
        numPasosText.text = "pasos: " + currentMovs;
    }

    /// <summary>
    /// Aplica el comportamiento del botón de deshacer en función de si está activo o no
    /// </summary>
    /// <param name="active">Determina si el elemento está activo o no</param>
    /// <param name="action">Callback del comportamiento del boton undo</param>
    public void UndoButtonBehaviour(bool active, UnityAction action = null)
    {
        undoElement.elementButton.onClick.RemoveAllListeners();

        // Si está activo se ve blanco, sino, gris
        var color = active ? Color.white : Color.grey;
        undoElement.elementImage.color = color;

        if (active)
        {
            undoElement.elementButton.onClick.AddListener(action);
        }
    }
}