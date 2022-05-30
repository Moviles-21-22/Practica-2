using System.Collections.Generic;
using UnityEngine;

//  Check Amaro

/// <summary>
/// Clase scripteable usada para representar los packs con los niveles
/// </summary>
[System.Serializable]
//[CreateAssetMenu(fileName = "levelpack", menuName = "Flow/level pack", order = 1)]
public class LevelPack : ScriptableObject
{
    [Tooltip("Nombre del nivel")] public string levelName;

    [Tooltip("fichero del lote")] public TextAsset txt;

    [Tooltip("Nombre de cada grid de niveles")]
    public string[] gridNames;

    [Tooltip("Numero de niveles completados")]
    public int completedLevels;

    [Tooltip("Información de los niveles del paquete")]
    public Levels[] levelsInfo;

    [Tooltip("Pack bloqueado")] public bool lockPack;

    [Tooltip("Determina la forma de enumerar los niveles")]
    public bool splitLevels = true;

    /// <summary>
    /// Resetea el pack a un estado inicial
    /// </summary>
    public void Reset()
    {
        completedLevels = 0;
        for (int i = 0; i < gridNames.Length; i++)
        {
            levelsInfo[i].state = Levels.LevelState.UNCOMPLETED;
            levelsInfo[i].record = 0;
        }
    }
}

/// <summary>
/// Clase para representar los diferentes estados de un nivel
/// </summary>
[System.Serializable]
public class Levels
{
    public enum LevelState
    {
        UNCOMPLETED,
        COMPLETED,
        PERFECT
    }
    
    /// <summary>
    /// Estado del nivel
    /// </summary>
    public LevelState state;

    /// <summary>
    /// Record del nivel
    /// </summary>
    public int record;
}