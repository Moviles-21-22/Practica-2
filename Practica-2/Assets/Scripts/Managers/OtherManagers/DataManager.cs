using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class DataManager
{
    /// <summary>
    /// Nombre del archivo de guardado
    /// </summary>
    private const string FileName = "props.json";

    /// <summary>
    /// Nombre del archivo que genera logs
    /// </summary>
    private const string ErrorFileName = "errorLog.txt";

    /// <summary>
    /// Numero de pistas iniciales por defeto
    /// </summary>
    private const int NumHintsDefault = 2;

    /// <summary>
    /// Nombre de la ruta donde cargar/guardar datos
    /// </summary>
    private static readonly string Path = Application.persistentDataPath + "/";

    /// <summary>
    /// Lista de los paquetes de las categorías del juego
    /// </summary>
    private static List<Category> _categories = new List<Category>();

    /// <summary>
    /// Lista de los paquetes de temas del juego
    /// </summary>
    private static List<ColorPack> _colorThemes = new List<ColorPack>();

    private static DataToSave _currData;
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Carga los datos del juego en caso de haberlos
    /// </summary>
    /// <returns></returns>
    public static void LoadData(List<Category> categories, List<ColorPack> themes, bool reloadEditorData)
    {
        _categories = categories;
        _colorThemes = themes;

        //  1. Inicializacion del folder. Si no existe, se crea.
        if (!Directory.Exists(Path))
        {
            Directory.CreateDirectory(Path);
        }

#if UNITY_EDITOR
        // 2. Si existe y se quieren usar los datos del editor, se elimin la carpeta y se vuelve a crear
        else if (reloadEditorData)
        {
            FileUtil.DeleteFileOrDirectory(Path);
            Directory.CreateDirectory(Path);
        }

        // 3. Archivo para mostrar errores. Si no existe, se crea
        // Solo interesa en el editor para no ralentizar la ejecución
        if (!File.Exists(Path + ErrorFileName))
        {
            StreamWriter w = File.CreateText(Path + ErrorFileName);
            w.WriteLine("Creamos el log");
            w.Close();
        }
#endif

        Load();
    }

    /// <summary>
    /// Guarda todos los datos del juego en un json
    /// </summary>
    /// <param name="numHints">Número de pistas</param>
    /// <param name="isPremium">Determina si el usuario es premium</param>
    /// <param name="categories">Lista de categorías</param>
    /// <param name="themes">Lista de los temas</param>
    /// <param name="currTheme">Último tema aplicado en el juego</param>
    public static void Save(int numHints, bool isPremium, List<Category> categories, List<ColorPack> themes,
        ColorPack currTheme)
    {
        try
        {
            DebugLogs("Empezando a guardar datos...");
            // 1. Se crea el objeto que se va a serializar
            DataToSave objToSave = new DataToSave(numHints, isPremium, categories, themes, currTheme);

            // 2. Se le añade el hash
            objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));

            // 3. Se transforma el objeto a formato JSON
            var json = JsonUtility.ToJson(objToSave);
            DebugLogs("Json guardado con exito...");

            // 4. Se sobreescribe el archivo 
            File.WriteAllText(Path + FileName, json);
        }
        catch (System.Exception e)
        {
            DebugLogs("Error al guardar...");
            DebugLogs(e.Message);
            throw new System.Exception(e.Message);
        }
    }

//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Genera los datos de cargado del juego
    /// </summary>
    private static void Load()
    {
        if (!File.Exists(Path + FileName))
        {
            CreateDefaultJson();
        }
        else
        {
            try
            {
                LogReset();
                DebugLogs("Estamos usando la ruta " + Path);
                // 1. Se leen los datos del json y se transforman
                var json = File.ReadAllText(Path + FileName);
                _currData = JsonUtility.FromJson<DataToSave>(json);
            }
            catch (System.Exception e)
            {
                DebugLogs(e.Message);
                throw new System.Exception(e.Message);
            }

            // 2. Se compara el hash con el original
            var originalHash = _currData.GetHash();
            // Como el hash original se construyó a partir de un hash = null, primero hay que
            // construir una copia, asignar su hash como null y generar el hash de la copia.
            // Mirar punto 4 del método CreateDefaultJson()
            var dataAux = new DataToSave(_currData);
            dataAux.SetHash(null);
            var hash = SecureManager.Hash(JsonUtility.ToJson(dataAux));

            if (originalHash.Equals(hash))
            {
                DebugLogs("Datos verificados...");
            }
            else
            {
                DebugLogs("Datos corruptos, creando unos por defecto...");
                CreateDefaultJson();
            }
        }
    }

    /// <summary>
    /// Crea un json por defecto, resetea los recursos y devuelve un DataToSave con toda esta información
    /// </summary>
    /// <returns></returns>
    private static void CreateDefaultJson()
    {
        try
        {
            DebugLogs("Creando props por defecto...");
            // 1. Se resetean las categorías del juego
            foreach (var cat in _categories)
            {
                cat.Reset();
            }

            // 2. Se resetean los temas del juego, pero se deja el primero activo
            _colorThemes[0].Reset();
            _colorThemes[0].active = true;
            for (var i = 1; i < _colorThemes.Count; i++)
            {
                _colorThemes[i].Reset();
            }

            // 3. Se generan los datos que se quieren guardar
            _currData = new DataToSave(NumHintsDefault, false, _categories, _colorThemes, _colorThemes[0]);

            // 4. Se crea el Hash
            var has = SecureManager.Hash(JsonUtility.ToJson(_currData));
            _currData.SetHash(has);

            // 5. Se vuelcan los datos al json
            var json = JsonUtility.ToJson(_currData);

            // 6. Se crea o se sobreescribe el archivo
            File.WriteAllText(Path + FileName, json);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            DebugLogs("No se ha podido crear el json por defecto, reinstala la app");
            DebugLogs(e.Message);
            throw new System.Exception("Reinstala la app");
        }
    }

    public static DataToSave GetCurrData()
    {
        return _currData;
    }

    /// <summary>
    /// Agrega una línea al archivo de logs
    /// </summary>
    /// <param name="message"></param>
    public static void DebugLogs(string message)
    {
#if UNITY_EDITOR
        string log;
        //  En init se crea el logError
        if (!File.Exists(Path + ErrorFileName))
        {
            throw new System.Exception("No se ha creado correctamente el log");
        }

        StreamReader read = File.OpenText(Path + ErrorFileName);
        log = read.ReadToEnd();
        read.Close();

        log += "\n" + message;
        StreamWriter writer = new StreamWriter(Path + ErrorFileName);
        writer.Write(log);
        writer.Close();
#endif
    }

    /// <summary>
    /// Resetea el archivo de logs
    /// </summary>
    private static void LogReset()
    {
        if (File.Exists(Path + ErrorFileName))
        {
            File.Delete(Path + ErrorFileName);
        }

        StreamWriter writer = File.CreateText(Path + ErrorFileName);
        writer.WriteLine("Reset al log");
        writer.Close();
    }
}