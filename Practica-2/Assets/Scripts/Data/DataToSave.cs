using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase para guardar datos serializables
/// </summary>
[Serializable]
public class DataToSave
{
    //  Numero de pistas
    [SerializeField] private int currHints;

    //  Para determinar si el jugador es isPremium
    [SerializeField] private bool isPremium;

    //  Categorias serializables
    [SerializeField] private List<GameManager.CategoryData> categoriesList;

    //  Lista de skins
    [SerializeField] private List<GameManager.ThemeData> themesList;

    //  Hash creado a partir del serializable
    [SerializeField] private string hash;

//--------------------------------------------------------------------------------------------------------------------//
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
    /// Genera los datos de las categorías del juego para que sean serializados.
    /// </summary>
    /// <param name="categories">Lista de categorías</param>
    private void CreateCategoryData(List<Category> categories)
    {
        int numCategories = categories.Count;
        categoriesList = new List<GameManager.CategoryData>();
        for (int i = 0; i < numCategories; i++)
        {
            categoriesList.Add(new GameManager.CategoryData());
            // Paquete de niveles
            int numLevelPacks = categories[i].levels.Length;
            categoriesList[i].levels = new GameManager.LevelPackData[numLevelPacks];
            var levelPack = categories[i].levels;
            CreateLevelPacksData(i, numLevelPacks, levelPack);
        }
    }

    /// <summary>
    /// Genera los datos de los paquetes de niveles de una categoría
    /// </summary>
    /// <param name="i">Índice de la lista de categoría para seleccionar una categoría</param>
    /// <param name="numLevelPacks">Número de paquetes de niveles</param>
    /// <param name="levelPack">Lista de los paquetes de niveles</param>
    private void CreateLevelPacksData(int i, int numLevelPacks, IReadOnlyList<LevelPack> levelPack)
    {
        for (int j = 0; j < numLevelPacks; j++)
        {
            categoriesList[i].levels[j] = new GameManager.LevelPackData
            {
                // 1. Nombre del paquete
                name = levelPack[j].name,
                // 2. Número de niveles completos
                completedLevels = levelPack[j].completedLevels
            };

            // 3. Información de los niveles
            int numLevels = levelPack[j].levelsInfo.Length;
            categoriesList[i].levels[j].levelsInfo = new Levels[numLevels];
            for (int k = 0; k < numLevels; k++)
            {
                categoriesList[i].levels[j].levelsInfo[k] = new Levels
                {
                    // 3.1 Records de cada uno de los niveles
                    record = levelPack[j].levelsInfo[k].record,
                    // 3.2 Información de los niveles
                    state = levelPack[j].levelsInfo[k].state
                };
            }
        }
    }

    /// <summary>
    /// Genera los datos de los temas del juego
    /// </summary>
    /// <param name="themes">Lista de los temas del juego</param>
    /// <param name="lastTheme">Último tema usado</param>
    private void CreateThemeData(IReadOnlyList<ColorPack> themes, ColorPack lastTheme)
    {
        int numThemes = themes.Count;
        themesList = new List<GameManager.ThemeData>();
        for (int i = 0; i < numThemes; i++)
        {
            themesList.Add(new GameManager.ThemeData
            {
                name = themes[i].name,
                isCurrTheme = themes[i] == lastTheme,
                // Estado del tema
                unlocked = themes[i].unlocked
            });
            int numColors = themes[i].colors.Count;
            themesList[i].colors = new List<Color>();

            for (int j = 0; j < numColors; j++)
            {
                themesList[i].colors.Add(themes[i].colors[j]);
            }
        }
    }
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Constructor por copia
    /// </summary>
    public DataToSave(DataToSave other)
    {
        if (other == null) return;

        currHints = other.GetNumHints();
        isPremium = other.GetIsPremium();
        CreateCategoryData(other.GetCategories());
        CreateThemeData(other.GetThemes());
    }

    /// <summary>
    /// Genera los datos de la categoría mediante el constructor por copia
    /// </summary>
    /// <param name="categories">Lista de las categorías del otro DataToSave</param>
    private void CreateCategoryData(List<GameManager.CategoryData> categories)
    {
        int numCategories = categories.Count;
        categoriesList = new List<GameManager.CategoryData>();
        for (int i = 0; i < numCategories; i++)
        {
            categoriesList.Add(new GameManager.CategoryData());
            // 1. Paquetes de niveles
            int numLevelPacks = categories[i].levels.Length;
            categoriesList[i].levels = new GameManager.LevelPackData[numLevelPacks];
            var levelPack = categories[i].levels;
            CreateLevelPacksData(i, numLevelPacks, levelPack);
        }
    }

    /// <summary>
    /// Genera los datos de los paquetes de niveles de una categoría mediante el constructor por copia
    /// </summary>
    /// <param name="i">Índice de la lista de categoría para seleccionar una categoría</param>
    /// <param name="numLevelPacks">Número de paquetes de niveles</param>
    /// <param name="levelPack">Lista de los paquetes de niveles del otro DataToSave</param>
    private void CreateLevelPacksData(int i, int numLevelPacks, IReadOnlyList<GameManager.LevelPackData> levelPack)
    {
        for (int j = 0; j < numLevelPacks; j++)
        {
            categoriesList[i].levels[j] = new GameManager.LevelPackData
            {
                // 1. Nombre del paquete
                name = levelPack[j].name,
                // 2. Número de niveles completos
                completedLevels = levelPack[j].completedLevels
            };
            // 3. Información de los niveles
            int numLevels = levelPack[j].levelsInfo.Length;
            categoriesList[i].levels[j].levelsInfo = new Levels[numLevels];
            for (int k = 0; k < numLevels; k++)
            {
                // 3.1 Records de cada uno de los niveles
                categoriesList[i].levels[j].levelsInfo[k] = new Levels
                {
                    record = levelPack[j].levelsInfo[k].record,
                    // 3.2 Información de los niveles
                    state = levelPack[j].levelsInfo[k].state
                };
            }
        }
    }

    /// <summary>
    /// Genera los datos de los temas del juego
    /// </summary>
    /// <param name="themes">Lista de los temas del juego</param>
    private void CreateThemeData(IList<GameManager.ThemeData> themes)
    {
        int numThemes = themes.Count;
        themesList = new List<GameManager.ThemeData>();
        for (int i = 0; i < numThemes; i++)
        {
            themesList.Add(new GameManager.ThemeData
            {
                name = themes[i].name,
                // Tema actual
                isCurrTheme = themes[i].isCurrTheme,
                // Estado del tema
                unlocked = themes[i].unlocked,
                colors = themes[i].colors
            });

            int numColors = themes[i].colors.Count;
            themesList[i].colors = new List<Color>();

            for (int j = 0; j < numColors; j++)
            {
                themesList[i].colors.Add(themes[i].colors[j]);
            }
        }
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
    public List<GameManager.CategoryData> GetCategories()
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
    public List<GameManager.ThemeData> GetThemes()
    {
        return themesList;
    }

    /// <summary>
    /// Devuelve la skin actualmente usada por el jugador
    /// </summary>
    /// <returns>Devuelve el índice de la skin usada</returns>
    public int GetCurrentTheme()
    {
        for (int i = 0; i < themesList.Count; i++)
        {
            if (themesList[i].isCurrTheme)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// Guarda el hash en la clase
    /// </summary>
    /// <param name="newHash"></param>
    public void SetHash(string newHash)
    {
        hash = newHash;
    }

    public void SetNewData(int numHints, bool premium, List<GameManager.CategoryData> categoriesData,
        List<GameManager.ThemeData> themesData)
    {
        currHints = numHints;
        isPremium = premium;
        categoriesList = categoriesData;
        themesList = themesData;
        hash = null;
    }
}