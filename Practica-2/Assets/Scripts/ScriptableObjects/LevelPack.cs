using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="levelpack",menuName ="Flow/level pack",order = 1)]
public class LevelPack : ScriptableObject
{
    [Tooltip("Nombre del nivel")]
    public string levelName;
    [Tooltip("fichero del lote")]
    public TextAsset txt;
    [Tooltip("Nombre de cada grid de niveles")]
    public string[] gridNames;
    [Tooltip("Numero de niveles completados")]
    public int completedLevels;
    [Tooltip("Información de los niveles del paquete")]
    public List<Levels> levelsInfo;
    [Tooltip("Pack bloqueado")]
    public bool lockPack;
    [Tooltip("Record de cada nivel")]
    public int[] records;
    [Tooltip("Determina la forma de enumerar los niveles")]
    public bool splitLevels = true;
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
}
