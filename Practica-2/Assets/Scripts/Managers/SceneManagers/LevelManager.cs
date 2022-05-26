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

    [Tooltip("Referencia al adsManager")]
    [SerializeField]
    private AdsManager adsManager;

    private GameManager gm;
    public void Init(Map currMap, Level lvl, LevelPack package, List<Color> theme, int numHints)
    {
        gm = GameManager.instance;
        adsManager.Init();
        hud.Init(currMap, lvl, package, numHints, this);
        board.Init(lvl, theme, numHints, hudRegion, this);
    }

    public void LoadScene(GameManager.SceneOrder scene)
    {
        // TODO: Pensar en si se podría meter el SaveGame en el LoadScene. ¿Interesa guardar en cada cambio de escena?
        gm.SaveGame();
        gm.LoadScene((int)scene);
    }

    public void BackToSelectLevelScene()
    {
        gm.LoadScene((int) GameManager.SceneOrder.LEVEL_SELECT);
    }

    public void AddHints(int numOfHints)
    {
        gm.AddHints(numOfHints);
    }

    public void ChangeLevel(int level)
    {
        gm.ChangeLevel(level);
    }

    public void AddSolutionLevel(bool perfect, int movements)
    {
        gm.AddSolutionLevel(perfect, movements);
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
        hud.UpdateHintText();
    }
}