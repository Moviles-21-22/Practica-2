using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace

/// <summary>
/// Clase para guardar datos serializables
/// </summary>
[System.Serializable]
public class DataToSave
{
    //  Numero de pistas
    [SerializeField] private int numHints;

    //  Para determinar si el jugador es premium
    [SerializeField] private bool premium;

    //  Lista de skins
    [SerializeField] private List<ColorPack> themes;

    //  Skin actual usada por el jugador
    [SerializeField] private ColorPack currTheme;

    //  Categorias serializables
    [SerializeField] private List<Category> categories;

    //  Hash creado a partir del serializable
    [SerializeField] private string hash;

    public DataToSave(int numH, bool p, List<Category> c, List<ColorPack> t,
        ColorPack lastTheme)
    {
        numHints = numH;
        premium = p;
        categories = c;
        themes = t;
        currTheme = lastTheme;
    }

    /// <summary>
    /// Guarda el hash en la clase
    /// </summary>
    /// <param name="newHash"></param>
    public void SetHash(string newHash)
    {
        this.hash = newHash;
    }

    /// <summary>
    /// Devuelve el hash
    /// </summary>
    /// <returns>hash</returns>
    public string GetHash()
    {
        return hash;
    }

    /// <summary>
    /// Devuelve todas las categorias disponibles
    /// </summary>
    /// <returns></returns>
    public List<Category> GetCategories()
    {
        return categories;
    }

    /// <summary>
    /// Devuelve el número de pistas disponibles
    /// </summary>
    /// <returns></returns>
    public int GetNumHints()
    {
        return numHints;
    }

    /// <summary>
    /// Devuelve el estatus del jugador (premium)
    /// </summary>
    /// <returns></returns>
    public bool GetPremiumStatus()
    {
        return premium;
    }

    /// <summary>
    /// Devuelve todos las skins disponibles
    /// </summary>
    /// <returns></returns>
    public List<ColorPack> GetThemes()
    {
        return themes;
    }

    /// <summary>
    /// Devuelve la skin actualmente usada por el jugador
    /// </summary>
    /// <returns></returns>
    public ColorPack GetCurrentTheme()
    {
        return currTheme;
    }
}