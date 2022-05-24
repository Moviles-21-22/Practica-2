using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/// <summary>
/// Clase scripteable para las diferentes categorias
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "skinpack", menuName = "Flow/skin", order = 1)]
[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class Category : ScriptableObject
{
    [Tooltip("Nombre de la categoria")]
    public string categoryName;
    [Tooltip("Color de un tile")]
    public Color color;
    [Tooltip("fichero con los niveles")]
    public LevelPack[] levels;
    
    /// <summary>
    /// Resetea a un estado inicial
    /// </summary>
    public void Reset()
    {
        foreach (LevelPack level in levels)
        {
            level.Reset();
        }
    }
}
