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

    [Tooltip("Referencia al BoardManager")] [SerializeField]
    private RectTransform[] hudRegion;
    
    [Tooltip("Tema que se carga por defecto")]
    public ColorPack defaultTheme;
    
    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private LevelPack defaultPack;
    
    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private int defaultLevel;

    [Tooltip("Referencia al adsManager")]
    [SerializeField]
    private AdsManager adsManager;

    private GameManager gm;

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
            
            var defMap = gm.GetCurrMap();
            var defLevel = gm.GetCurrLevel();
            var defColors = defaultTheme.colors;
            hud.Init(defMap, defLevel, defPack, numHints, this);
            board.Init(defLevel, defColors, numHints, hudRegion, this);
        }
        else
        {

            hud.Init(currMap, lvl, package, numHints, this);
            board.Init(lvl, theme, numHints, hudRegion, this);
        }
    }

    public void LoadScene(GameManager.SceneOrder scene)
    {
        gm.LoadScene((int)scene);
    }

    public void BackToSelectLevelScene()
    {
        gm.LoadScene((int)GameManager.SceneOrder.MAIN_MENU);
    }

    public void AddHints(int numOfHints)
    {
        gm.AddHints(numOfHints);
    }

    public void ChangeLevel(int level)
    {
        gm.ChangeLevel(level);
    }

    public void AddSolutionLevel(int movements, int numFlows)
    {
        
        gm.AddSolutionLevel(movements, numFlows);
    }

//-------------------------------------------------UPDATE-HUD---------------------------------------------------------//
    
    public void UpdatePercentage(int percentage)
    {
        hud.ShowPercentage(percentage);
    }

    public void UpdateUndoButtonBehaviour(bool active, UnityAction action = null)
    {
        hud.UndoButtonBehaviour(active, action);
    }

    public void UpdateFlowsCounter(int flows)
    {
        hud.ShowFlows(flows);
    }

    public void UpdateMovements(int movements)
    {
        hud.ShowMovements(movements);
    }

    public void LevelCompleted(bool isPerfect)
    {
        hud.LevelCompleted(isPerfect);
    }

    public void UpdateHints()
    {
        gm.UseHint();
        var hints = gm.GetHints();
        hud.UpdateHintText(hints);
    }
}