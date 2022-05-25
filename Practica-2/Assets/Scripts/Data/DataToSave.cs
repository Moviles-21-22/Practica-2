using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase para guardar datos serializables
/// </summary>
[System.Serializable]
public class DataToSave
{
    //  Numero de pistas
    [SerializeField] private int currHints;

    //  Para determinar si el jugador es isPremium
    [SerializeField] private bool isPremium;

    //  Categorias serializables
    [SerializeField] private List<Category> categoriesList;

    //  Lista de skins
    [SerializeField] private List<ColorPack> themesList;

    //  Skin actual usada por el jugador
    [SerializeField] private ColorPack currTheme;

    //  Hash creado a partir del serializable
    [SerializeField] private string hash;

    /// <summary>
    /// Genera un paquete de datos copiados de los datos del juego original para que
    /// se puedan serializar
    /// </summary>
    /// <param name="numHints">Número de pistas</param>
    /// <param name="premium">Determina si el usuario es premium</param>
    /// <param name="categories">Lista de categorías</param>
    /// <param name="themes">Lista de los temas</param>
    /// <param name="lastTheme">Último tema aplicado en el juego</param>
    public DataToSave(int numHints, bool premium, List<Category> categories,
        List<ColorPack> themes, ColorPack lastTheme)
    {
        currHints = numHints;
        isPremium = premium;
        CreateCategoryData(categories);
        CreateThemeData(themes, lastTheme);
    }

    /// <summary>
    /// Constructor por copia
    /// </summary>
    public DataToSave(DataToSave other)
    {
        if (other == null) return;

        currHints = other.GetNumHints();
        isPremium = other.GetIsPremium();
        CreateCategoryData(other.GetCategories());
        CreateThemeData(other.GetThemes(), other.GetCurrentTheme());
    }

    private void CreateCategoryData(List<Category> categories)
    {
        int numCategories = categories.Count;
        categoriesList = new List<Category>();
        for (int i = 0; i < numCategories; i++)
        {
            categoriesList.Add(ScriptableObject.CreateInstance<Category>());
            // 1. Nombre de la categoría
            categoriesList[i].categoryName = categories[i].name;
            // 2. Color de la categoría
            categoriesList[i].color = categories[i].color;
            // 3. Paquetes de niveles
            int numLevelPacks = categories[i].levels.Length;
            categoriesList[i].levels = new LevelPack[numLevelPacks];
            var levelPack = categories[i].levels;
            CreateLevelPacksData(i, numLevelPacks, levelPack);
        }
    }

    private void CreateLevelPacksData(int i, int numLevelPacks, IReadOnlyList<LevelPack> levelPack)
    {
        for (int j = 0; j < numLevelPacks; j++)
        {
            categoriesList[i].levels[j] = ScriptableObject.CreateInstance<LevelPack>();
            // 1. Nombre del paquete
            categoriesList[i].levels[j].levelName = levelPack[j].levelName;
            // 2. Fichero de niveles
            categoriesList[i].levels[j].txt = levelPack[j].txt;
            // 3. Nombre de los subpaquetes de niveles
            categoriesList[i].levels[j].gridNames = levelPack[j].gridNames;
            // 4. Número de niveles completos
            categoriesList[i].levels[j].completedLevels = levelPack[j].completedLevels;
            // 5. Candado en los niveles
            categoriesList[i].levels[j].lockPack = levelPack[j].lockPack;
            // 6. Números de niveles spliteados en la UI
            categoriesList[i].levels[j].splitLevels = levelPack[j].splitLevels;
            // 7. Información de los niveles
            int numLevels = levelPack[j].levelsInfo.Count;
            categoriesList[i].levels[j].records = new int[numLevels];
            categoriesList[i].levels[j].levelsInfo = new List<Levels>();
            for (int k = 0; k < numLevels; k++)
            {
                // 7.1 Records de cada uno de los niveles
                categoriesList[i].levels[j].records[k] = levelPack[j].records[k];
                // 7.2 Información de los niveles
                categoriesList[i].levels[j].levelsInfo.Add(new Levels());
                categoriesList[i].levels[j].levelsInfo[k].completed = levelPack[j].levelsInfo[k].completed;
                categoriesList[i].levels[j].levelsInfo[k].perfect = levelPack[j].levelsInfo[k].perfect;
            }
        }
    }

    private void CreateThemeData(IReadOnlyList<ColorPack> themes, ColorPack lastTheme)
    {
        int numThemes = themes.Count;
        themesList = new List<ColorPack>();
        for (int i = 0; i < numThemes; i++)
        {
            themesList.Add(ScriptableObject.CreateInstance<ColorPack>());
            // 1. Nombre del tema
            themesList[i].colorPackName = themes[i].colorPackName;
            // 2. Colores del tema
            themesList[i].colors = themes[i].colors;
            // 3. Estado del tema
            themesList[i].active = themes[i].active;
            // 4. Estado inicial del tema
            themesList[i].initState = themes[i].initState;
        }
        
        // Asignación de los datos del tema actual
        currTheme = ScriptableObject.CreateInstance<ColorPack>();
        currTheme.colorPackName = lastTheme.colorPackName;
        currTheme.colors = lastTheme.colors;
        currTheme.active = lastTheme.active;
        currTheme.initState = lastTheme.initState;
    }

//-----------------------------------------------------GET-SET--------------------------------------------------------//
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
        return categoriesList;
    }

    /// <summary>
    /// Devuelve el número de pistas disponibles
    /// </summary>
    /// <returns></returns>
    public int GetNumHints()
    {
        return currHints;
    }

    /// <summary>
    /// Devuelve el estatus del jugador (isPremium)
    /// </summary>
    /// <returns></returns>
    public bool GetIsPremium()
    {
        return isPremium;
    }

    /// <summary>
    /// Devuelve todos las skins disponibles
    /// </summary>
    /// <returns></returns>
    public List<ColorPack> GetThemes()
    {
        return themesList;
    }

    /// <summary>
    /// Devuelve la skin actualmente usada por el jugador
    /// </summary>
    /// <returns></returns>
    public ColorPack GetCurrentTheme()
    {
        return currTheme;
    }

    /// <summary>
    /// Guarda el hash en la clase
    /// </summary>
    /// <param name="newHash"></param>
    public void SetHash(string newHash)
    {
        hash = newHash;
    }
}