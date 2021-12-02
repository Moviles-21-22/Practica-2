using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="levelpack",menuName ="Flow/level pack",order = 1)]
public class LevelPack : ScriptableObject
{
    [Tooltip("Niveles totales")]
    public int totalLevels;
    [Tooltip("Nombre del nivel")]
    public string levelName;
    [Tooltip("fichero del lote")]
    public TextAsset txt;
    [Tooltip("Nombre de cada grid de niveles")]
    public string[] gridNames;
    [Tooltip("Niveles superados")]
    public bool[] activeLevels;
    [Tooltip("Pack bloqueado")]
    public bool lockPack;
    [Tooltip("Record de cada nivel")]
    public int[] records;
}


