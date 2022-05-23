using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

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

    public void Init(Map currMap, Level lvl, LevelPack package, List<Color> theme, int numHints)
    {
        hud.Init(currMap, lvl, package, numHints, this);
        board.Init(lvl, theme, numHints);
    }

    public void LoadScene(GameManager.SceneOrder scene)
    {
        // TODO: Guardar antes de cambiar de escena
        GameManager.instance.LoadScene((int)scene);
    }

    public void AddHints(int numOfHints)
    {
        GameManager.instance.AddHints(numOfHints);
    }

    public void ChangeLevel(int level)
    {
        GameManager.instance.ChangeLevel(level);
    }

    public void UseHint()
    {
        GameManager.instance.UseHint();
    }
}