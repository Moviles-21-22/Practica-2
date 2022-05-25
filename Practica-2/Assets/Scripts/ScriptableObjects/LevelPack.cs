using System.Collections.Generic;
using UnityEngine;

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
    public List<Levels> levelsInfo;

    [Tooltip("Pack bloqueado")] public bool lockPack;
    [Tooltip("Record de cada nivel")] public int[] records;

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
            records[i] = 0;
            levelsInfo[i].completed = false;
            levelsInfo[i].perfect = false;
        }
    }
}

[System.Serializable]
public class Levels
{
    /// <summary>
    /// Para saber si se ha superado el nivel
    /// </summary>
    public bool completed;

    /// <summary>
    /// Para saber si se ha superado con la puntuación máxima
    /// </summary>
    public bool perfect;

    public Levels()
    {
    }
}