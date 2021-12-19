using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Clase creada para guardar categorias
/// </summary>
[System.Serializable]
public class CategoryToSave
{
    //  Categoria a guardar
    [SerializeField]
    Category cat;

    public CategoryToSave(Category _cat)
    {
        cat = _cat;
    }
}

/// <summary>
/// Clase para guardar datos serializables
/// </summary>
[System.Serializable]
public class DataToSave
{
    //  Numero de pistas
    [SerializeField]
    private int numHints;

    //  Para determinar si el jugador es premium
    [SerializeField]
    private bool premium;

    //  Lista de skins
    [SerializeField]
    private List<ColorPack> themes;

    //  Skin actual usada por el jugador
    [SerializeField]
    private ColorPack currTheme;

    //  Categorias serializables
    public List<Category> categories;
    
    //  Hash creado a partir del serializable
    [SerializeField]
    private string hash;

    [SerializeField]
    public DataToSave(int _numHints, bool _premium, List<Category> _category, List<ColorPack> _theme, ColorPack _lastTheme)
    {
        numHints = _numHints;
        premium = _premium;
        categories = _category;
        themes = _theme;
        currTheme = _lastTheme;
    }
    /// <summary>
    /// Guarda el hash en la clase
    /// </summary>
    /// <param name="_hash"></param>
    public void SetHash(string _hash)
    {
        hash = _hash;
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


public class DataManager : MonoBehaviour
{
    //  Nombre del props
    private string fileName = "props.json";
    //  Nombre del log
    private string errorFileName = "errorLog.txt";
    //  Ruta abs de la carpeta save
    private string routeToSave;
    //  Numero de pistas por defecto
    private const int numHintsDefault = 2;
    [Tooltip("Lista de categorias por defecto")]
    [SerializeField] List<Category> categories;

    [Tooltip("Lista de colores por defecto")]
    [SerializeField] List<ColorPack> colorThemes;
    public static DataManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
#if UNITY_EDITOR
            routeToSave = Directory.GetCurrentDirectory() + "/Assets/save/";
#elif UNITY_ANDROID
            routeToSave = Application.persistentDataPath + "/save/";
#endif
            Init();
            Load();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Guardamos en un json toda la información necesaria
    /// </summary>    
    public void Save()
    {
        try
        {
            LogError("Empezamos a guardar los datos...");
            //  Serializamos los posibles datos que pueden haber cambiado
            DataToSave objToSave = new DataToSave(GameManager.instance.GetNumHints(),
                                                    GameManager.instance.IsPremium(),
                                                    GameManager.instance.GetCategories(),
                                                    GameManager.instance.GetThemes(),
                                                    GameManager.instance.GetCurrTheme());
            //  Creamos el hash
            objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));
            //  Escribimos en el json
            string json = JsonUtility.ToJson(objToSave);
            LogError("Json guardado con exito...");

            if (!Directory.Exists(routeToSave))
            {
                Directory.CreateDirectory(routeToSave);
                LogError("El directorio " + routeToSave + " se ha creado correctamente");
            }

            if (File.Exists(routeToSave + fileName))
            {
                File.Delete(routeToSave + fileName);
            }
            File.WriteAllText(routeToSave + fileName, json);
        }
        catch (Exception e)
        {
            LogError("Error al guardar...");
            LogError(e.Message);
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Carga el json con la información necesaria para cargar un usuario
    /// </summary>
    public void Load()
    {
        // Si existe el props
        if (System.IO.File.Exists(routeToSave + fileName))
        {
            string json = string.Empty;
            DataToSave objToLoad;
            try
            {
                LogReset();
                LogError("Estamos usando la ruta " + routeToSave);
                json = System.IO.File.ReadAllText(routeToSave + fileName);
                objToLoad = JsonUtility.FromJson<DataToSave>(json);
            }
            catch (System.Exception e)
            {
                LogError(e.Message);
                throw new System.Exception(e.Message);
            }
            //  Dividimos el contenido del json
            //  El ultimo elemento del hashGenerated va a ser el hash
            string[] hashGenerated = json.Split(',');
            string serializado = string.Empty;
            for (int i = 0; i < hashGenerated.Length - 1; i++)
            {
                serializado += hashGenerated[i] + ",";
            }
            serializado +="\"hash\":\"\"}";
            //  Ambos hash coinciden
            if (SecureManager.Hash(serializado).Equals(objToLoad.GetHash()))
            {
                LogError("Datos verificados...");
                GameManager.instance.InitDataLoaded(objToLoad);
            }
            else
            {
                LogError("Datos corruptos, creando unos por defecto...");
                // Reseteamos el json con valores por defecto
                GameManager.instance.InitDataLoaded(CreateDefaultJson());
            }
        }
        // Crear un archivo con valores por defecto
        else
        {
            GameManager.instance.InitDataLoaded(CreateDefaultJson());
        }
    }

    /// <summary>
    /// Crea el json, resetea los recursos y devuelve un DataToSave con toda esta información
    /// </summary>
    /// <returns></returns>
    private DataToSave CreateDefaultJson()
    {
        try
        {
            LogError("Creando props por defecto...");
            //  Reset a las categorías
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Reset();
            }
            //  Reset a los colores
            foreach (ColorPack lP in colorThemes)
            {
                lP.Reset();
            }
            //  Guardado de json
            DataToSave currData = new DataToSave(numHintsDefault, false, categories, colorThemes, colorThemes[0]);
            currData.SetHash(SecureManager.Hash(JsonUtility.ToJson(currData)));
            string json = JsonUtility.ToJson(currData);
            System.IO.File.WriteAllText(routeToSave + fileName, json);
            return currData;
        }
        catch (Exception e)
        {
            print(e.Message);
            LogError("No se ha podido crear el json por defecto, reinstala la app");
            LogError(e.Message);
            throw new Exception("Reinstala la app");
        }
    }

    /// <summary>
    /// Agrega una línea al log
    /// </summary>
    /// <param name="message"></param>
    public void LogError(string message)
    {
        string log = String.Empty;
        //  En init se crea el logError
        if (!File.Exists(routeToSave + errorFileName)) 
        {
            throw new Exception("No se ha creado correctamente el log");
        }

        StreamReader read = File.OpenText(routeToSave + errorFileName);
        log = read.ReadToEnd();
        read.Close();

        log += "\n" + message;
        StreamWriter writer = new StreamWriter(routeToSave + errorFileName);
        writer.Write(log);
        writer.Close();
    }

    /// <summary>
    /// Resetea el log
    /// </summary>
    private void LogReset()
    {
        if (File.Exists(routeToSave + errorFileName))
        {
            File.Delete(routeToSave + errorFileName);
        }
        StreamWriter writer = File.CreateText(routeToSave + errorFileName);
        writer.WriteLine("Reset al log");
        writer.Close();
    }

    /// <summary>
    /// Inicializa los docs
    /// </summary>
    private void Init()
    {
        //  Inicializamos el folder
        if (!Directory.Exists(routeToSave))
        {
            Directory.CreateDirectory(routeToSave);
        }

        //  Inicializamos el logError
        if (!File.Exists(routeToSave + errorFileName))
        {
            StreamWriter w = File.CreateText(routeToSave + errorFileName);
            w.WriteLine("Creamos el log");
            w.Close();
        }
    }
}
