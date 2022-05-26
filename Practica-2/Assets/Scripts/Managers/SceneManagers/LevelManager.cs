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
    
    [Tooltip("Nivel del paquete por defecto")] [SerializeField]
    private int defaultLevel;

    [SerializeField] private bool useDefaultLevel;
    

    [Tooltip("Referencia al adsManager")]
    [SerializeField]
    private AdsManager adsManager;

    private GameManager gm;
    public void Init(Map currMap, Level lvl, GameManager.LevelPackData package, List<Color> theme, int numHints)
    {
        gm = GameManager.instance;
        
        adsManager.Init();

        hud.Init(currMap, lvl, package, numHints, this);
        board.Init(lvl, theme, numHints, hudRegion, this);
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