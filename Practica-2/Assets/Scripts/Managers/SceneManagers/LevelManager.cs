using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clase encargada de inicializar y gestionar el nivel en la GameScene
/// </summary>
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "CheckNamespace")]
public class LevelManager : MonoBehaviour
{
    [Header("Seccion de Managers")] [Tooltip("Referencia al HUDManager")] [SerializeField]
    private HUDManager hud;

    [Tooltip("Referencia al BoardManager")] [SerializeField]
    private BoardManager board;

    [Tooltip("Referencia al adsManager")] [SerializeField]
    private AdsManager adsManager;

    [Tooltip("Referencia al BoardManager")] [SerializeField]
    private RectTransform[] hudRegion;

    [Tooltip("Tema que se carga por defecto")]
    public ColorPack defaultTheme;

    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private LevelPack defaultPack;

    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private int defaultLevel;

    private GameManager gm;

    private int lastTileColor;
    
    public void Init(Map currMap, Level lvl, GameManager.LevelPackData package,
        int numHints, List<Color> theme = null, bool useDefaultLevel = false)
    {
        gm = GameManager.instance;

        adsManager.Init();

        if (useDefaultLevel)
        {
            gm.LoadLevel(defaultLevel, defaultPack, true);
            GameManager.LevelPackData defPack = new GameManager.LevelPackData
            {
                name = defaultPack.levelName,
                completedLevels = defaultPack.completedLevels,
                levelsInfo = defaultPack.levelsInfo
            };

            var defLevel = gm.GetCurrLevel();
            var defColors = defaultTheme.colors;
            hud.Init(defLevel, defPack, numHints, this);
            board.Init(defLevel, defColors, numHints, hudRegion, this);
        }
        else
        {
            hud.Init(lvl, package, numHints, this);
            board.Init(lvl, theme, numHints, hudRegion, this);
        }
    }

    public void LoadScene(GameManager.SceneOrder scene)
    {
        gm.LoadScene((int) scene);
    }

    public void BackToSelectLevelScene()
    {
        gm.LoadScene((int) GameManager.SceneOrder.MAIN_MENU);
    }

    public void AddHints(int numOfHints)
    {
        AdsManager.PlayAd();
        gm.AddHints(numOfHints);
    }

    public void ChangeLevel(int level)
    {
        gm.ChangeLevel(level);
    }

    public void AddSolutionLevel(int movements, int numFlows)
    {
        gm.AddSolutionLevel(movements, numFlows);
        bool perfect = movements == numFlows;
        hud.LevelCompleted(perfect);
    }

//-------------------------------------------------UPDATE-HUD---------------------------------------------------------//

    /// <summary>
    /// Actualiza los datos del porcentaje en el hud
    /// </summary>
    /// <param name="percentage"></param>
    public void UpdatePercentage(int percentage)
    {
        hud.ShowPercentage(percentage);
    }

    /// <summary>
    /// Deshace el último movimiento del tablero
    /// </summary>
    public void UndoMovement()
    {
        board.UndoMovement(lastTileColor);
    }

    /// <summary>
    /// Activa el botón de deshacer movimientos
    /// </summary>
    public void ActivateUndoButton(int tileColor)
    {
        lastTileColor = tileColor;
        hud.ActivateUndoButton();
    }

    /// <summary>
    /// Actualiza el número de flujos conectados en el hud
    /// </summary>
    /// <param name="flows">Número de flujos conectados</param>
    public void UpdateFlowsCounter(int flows)
    {
        hud.UpdateFlows(flows);
    }

    /// <summary>
    /// Actualiza el número de movimientos en el hud
    /// </summary>
    /// <param name="movements">Número de movimientos</param>
    public void UpdateMovements(int movements)
    {
        hud.UpdateMovements(movements);
    }

    /// <summary>
    /// Le comunica al boardManager que utilice una pista
    /// </summary>
    public void UseHint()
    {
        gm.UseHint();
        board.ApplyHint();
    }
}