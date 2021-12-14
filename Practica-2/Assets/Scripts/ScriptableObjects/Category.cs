using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "skinpack", menuName = "Flow/skin", order = 1)]
public class Category : ScriptableObject
{
    [Tooltip("Nombre de la categoria")]
    public string categoryName;
    [Tooltip("Color de un tile")]
    public Color color;
    [Tooltip("fichero con los niveles")]
    public LevelPack[] levels;

    public void Reset()
    {
        foreach (LevelPack level in levels)
        {
            level.Reset();
        }
    }
}
