using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")] [SerializeField]
    private GameObject winInfo;

    [Tooltip("Referencia a la imagen de la estrella cuando se gana")] [SerializeField]
    private RawImage winStar;

    [Tooltip("Sprites que se usan para el icono de ganar")] [SerializeField]
    private Texture[] winSprites;

    [Header("Elementos del hud")]
    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")]
    [SerializeField]
    private HudElement levelTitleElement;

    [Tooltip("Referencia al objeto que muestra las pistas")] [SerializeField]
    private HudElement hints;

    [Header("Elementos del hud - Textos")] [Tooltip("Referencia al texto que muestra el nivel actual")] [SerializeField]
    private TextMeshProUGUI numLevelText;

    [Tooltip("Referencia al titulo del texto que se muestra al ganar")] [SerializeField]
    private TextMeshProUGUI winTitle;

    [Tooltip("Referencia al botón del próximo nivel cuando se gana")] [SerializeField]
    private TextMeshProUGUI nextLevelWinText;

    [Header("Board Info")] [Tooltip("Referencia al texto que el número de flujos conectados")] [SerializeField]
    private TextMeshProUGUI numFlowsText;

    [Tooltip("Referencia al texto que muestra el número de movimientos")] [SerializeField]
    private TextMeshProUGUI numPasosText;

    [Tooltip("Referencia al texto que muestra el récord actual")] [SerializeField]
    private TextMeshProUGUI recordText;

    [Tooltip("Referencia al texto que muestra el porcentaje completado")] [SerializeField]
    private TextMeshProUGUI percentageText;

    [Tooltip("Referencia al texto final que muestra el número de movimientos")] [SerializeField]
    private TextMeshProUGUI finalMovsText;

    [Header("Elementos del hud - Botones")] [Tooltip("Referencia al botón del proximo nivel")] [SerializeField]
    private Button undoButton;

    [Tooltip("Referencia al botón del proximo nivel")] [SerializeField]
    private Button nextLevelButton;

    [Tooltip("Referencia al botón del anterior nivel")] [SerializeField]
    private Button prevLevelButton;

    [Tooltip("Referencia al panel de ver video")] [SerializeField]
    private GameObject videoPanel;
    
//--------------------------------------------------ATRIBUTOS-PRIVADOS-------------------------------------------------//

    // Referencia al LevelManager
    private LevelManager lvlMan;

    // Numero de pistas disponibles
    private int currHints;

    //Paquete de niveles cargado
    private GameManager.LevelPackData levelPack;

    //Nivel cargado
    private Level currLevel;

    //Variable de tiempo usada para procesar las animaciones
    private float timer;

    //Numero de tuberías conectadas
    private int currentFlows;

    //Numero de pasos
    private int currentMovs;

//--------------------------------------------------------------------------------------------------------------------//

    public void Init(Level lvl, GameManager.LevelPackData package, int numHints, LevelManager lvlManager)
    {
        InitHudData(lvl, package, numHints, lvlManager);

        InitTopElements();
        InitBotElements();
    }

//-------------------------------------------------INICIALIZACION-----------------------------------------------------//

    /// <summary>
    /// Inicializa los datos generales del juego
    /// </summary>
    private void InitHudData(Level lvl, GameManager.LevelPackData package, int numHints, LevelManager lvlManager)
    {
        lvlMan = lvlManager;
        currHints = numHints;
        currLevel = lvl;
        levelPack = package;
        numLevelText.text = "Nivel " + (currLevel.lvl + 1);
    }

    /// <summary>
    /// Inicializa la parte superior del hud
    /// </summary>
    private void InitTopElements()
    {
        InitTitle();
        InitBoardInfo();
    }

    /// <summary>
    /// Inicializa la informaación del título del nivel
    /// </summary>
    private void InitTitle()
    {
        levelTitleElement.elementText.text = currLevel.numBoardX + "x" + currLevel.numBoardY;
        var levelState = levelPack.levelsInfo[currLevel.lvl].state;
        switch (levelState)
        {
            case Levels.LevelState.PERFECT:
                levelTitleElement.elementImage.enabled = true;
                levelTitleElement.elementImage.sprite = levelTitleElement.elementSprites[0];
                break;
            case Levels.LevelState.COMPLETED:
                levelTitleElement.elementImage.enabled = true;
                levelTitleElement.elementImage.sprite = levelTitleElement.elementSprites[1];
                break;
            case Levels.LevelState.UNCOMPLETED:
                break;
        }
    }

    /// <summary>
    /// Inicializa la información del tablero
    /// </summary>
    private void InitBoardInfo()
    {
        //============TABLERO-INFO===================//
        UpdateFlows(0);
        UpdateMovements(0);
        ShowPercentage(0);
        recordText.text = "récord:\n" + levelPack.levelsInfo[currLevel.lvl].record;
    }

    /// <summary>
    /// Inicializa la parte inferior del hud
    /// </summary>
    private void InitBotElements()
    {
        //============HINTS============//
        UpdateHintText();

        //============NIVEL-ANTERIOR============//
        if (currLevel.lvl == 0)
        {
            prevLevelButton.interactable = false;
        }
        //============NIVEL-POSTERIOR============//
        else if (currLevel.lvl + 1 == levelPack.levelsInfo.Length)
        {
            nextLevelButton.interactable = false;
        }
    }

//------------------------------------------------ACTUALIZACION-DATOS-------------------------------------------------//
    /// <summary>
    /// Actualiza el número de pistas locales y actualiza el text correspondiente a las pistas en el hud
    /// </summary>
    public void UpdateHint(int numHint)
    {
        currHints = numHint;
        UpdateHintText();
    }

    /// <summary>
    /// Cambia el texto que muestra las pistas en función de las pistas que queden
    /// </summary>
    private void UpdateHintText()
    {
        hints.elementText.text = currHints + "x";
    }

    /// <summary>
    /// Comportamiento del hud al completar el nivel
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
        if (currLevel.lvl + 1 == levelPack.levelsInfo.Length)
        {
            winTitle.text = "¡Felicitaciones!";
            finalMovsText.text = "Has llegado al final del\n" + levelPack.name;

            // Nos lleva al mainMenu
            nextLevelWinText.text = "elige el próximo paquete";
        }

        DisableElements();
        InvokeRepeating(nameof(WinAnimation), 0.0f, 0.05f);
    }

    /// <summary>
    /// Muestra la parte entera del porcentaje que hay resuelto del tablero
    /// </summary>
    public void ShowPercentage(int percentage)
    {
        percentageText.text = "tubería:\n" + percentage + "%";
    }

    /// <summary>
    /// Añade o quita un flujo al contador de flujos completos
    /// </summary>
    public void UpdateFlows(int flows)
    {
        currentFlows = flows;
        numFlowsText.text = "flujos:\n" + currentFlows + "/" + currLevel.numFlow;
    }

    /// <summary>
    /// Añade un nuevo movimiento al contador de pasos del canvas
    /// </summary>
    public void UpdateMovements(int movs)
    {
        currentMovs = movs;
        numPasosText.text = "pasos:\n" + currentMovs;
    }

    /// <summary>
    /// Activa el botón de deshacer movimientos
    /// </summary>
    public void ActivateUndoButton()
    {
        undoButton.interactable = true;
    }

    private void DisableElements()
    {
        undoButton.interactable = false;
        hints.elementButton.interactable = false;
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
//---------------------------------------------BUTTON-CALLBACKS-------------------------------------------------------//

    /// <summary>
    /// Callback para añadir Pistas mediante el vídeo de los anuncios
    /// </summary>
    /// <param name="numHints"></param>
    public void PlayVideo(int numHints)
    {
        lvlMan.AddHints(numHints);
        currHints += numHints;
        UpdateHintText();
    }

    /// <summary>
    /// Callback para los botones de cambiar de nivel
    /// </summary>
    /// <param name="next">Determina si es el siguiente nivel o el anterior</param>
    public void ChangeLevel(bool next)
    {
        var n = 1;

        // El botón de próximo nivel del pop-up cuando se gana
        // no se desactiva y puede seguir llamando a este método. 
        // Si es el último nivel, entonces vuelve al menú principal
        if (next && currLevel.lvl + 1 == levelPack.levelsInfo.Length)
            lvlMan.LoadScene(GameManager.SceneOrder.MAIN_MENU);
        else if (!next) n = -1;

        lvlMan.ChangeLevel(currLevel.lvl + n);
    }

    /// <summary>
    /// Callback para el botón de reiniciar el nivel
    /// </summary>
    public void RestartLevel()
    {
        lvlMan.LoadScene(GameManager.SceneOrder.GAME_SCENE);
    }

    /// <summary>
    /// Callback para el botón de deshacer movimientos
    /// </summary>
    public void UndoMovement()
    {
        lvlMan.UndoMovement();
        undoButton.interactable = false;
    }

    /// <summary>
    /// Callback para el botón de usar pistas
    /// </summary>
    public void UseHint()
    {
        if (currHints >= 1)
        {
            currHints--;
            UpdateHintText();
            lvlMan.UseHint();
        }
        else
        {
            videoPanel.SetActive(true);
        }
    }
}