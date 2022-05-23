using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace

[SuppressMessage("ReSharper", "StringLiteralTypo")]
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
    /// Inicializa los datos que se usarán para cargar y guardar el resto del juego
    /// </summary>
    /// <param name="catList">Lista de categorías del juego</param>
    /// <param name="themes">Lista de temas del juego</param>
    public static void Init(List<Category> catList, List<ColorPack> themes)
    {
        if (_categories.Count > 0)
            return;

        _categories = catList;
        _colorThemes = themes;

        //  1. Inicializacion del folder. Si no existe, se crea.
        if (!Directory.Exists(Path))
        {
            Directory.CreateDirectory(Path);
        }

#if UNITY_EDITOR
        // 2. Archivo para mostrar errores. Si no existe, se crea
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
    public static void Save(int numHints, bool isPremium, List<Category> categories, List<ColorPack> themes, ColorPack currTheme)
    {
        try
        {
            DebugLogs("Empezando a guardar datos...");
            //  Serializamos los posibles datos que pueden haber cambiado
            DataToSave objToSave = new DataToSave(numHints, isPremium, categories, themes, currTheme);
            //  Creamos el hash
            objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));
            //  Escribimos en el json
            string json = JsonUtility.ToJson(objToSave);
            DebugLogs("Json guardado con exito...");

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                DebugLogs("El directorio " + Path + " se ha creado correctamente");
            }

            if (File.Exists(Path + FileName))
            {
                File.Delete(Path + FileName);
            }

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
    /// Carga el json con la información necesaria para cargar un usuario
    /// </summary>
    private static void Load()
    {
        //var a = Path + FileName;
        // 1. ¿Existe el archivo con datos guardados?
        if (File.Exists(Path + FileName))
        {
            // 1.1 Si existe se intenta recoger sus datos
            string json;
            DataToSave objToLoad;
            try
            {
                LogReset();
                DebugLogs("Estamos usando la ruta " + Path);
                json = File.ReadAllText(Path + FileName);
                objToLoad = JsonUtility.FromJson<DataToSave>(json);
            }
            catch (System.Exception e)
            {
                DebugLogs(e.Message);
                throw new System.Exception(e.Message);
            }

            //  Dividimos el contenido del json
            //  El ultimo elemento del hashGenerated va a ser el hash
            var hashGenerated = json.Split(',');
            var serializado = string.Empty;
            for (var i = 0; i < hashGenerated.Length - 1; i++)
            {
                serializado += hashGenerated[i] + ",";
            }

            serializado += "\"hash\":\"\"}";
            //  Ambos hash coinciden
            if (SecureManager.Hash(serializado).Equals(objToLoad.GetHash()))
            {
                DebugLogs("Datos verificados...");
                GameManager.instance.LoadData(objToLoad);
            }
            else
            {
                DebugLogs("Datos corruptos, creando unos por defecto...");
                // Reseteamos el json con valores por defecto
                CreateDefaultJson();
                GameManager.instance.LoadData(_currData);
            }
        }
        //1.2 Si no existe el archivo, se crea uno por defecto
        else
        {
            CreateDefaultJson();
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
            // 1. Se parte de catergorías base
            foreach (var cat in _categories)
            {
                cat.Reset();
            }

            // 2. Se parte de temas base
            foreach (var theme in _colorThemes)
            {
                theme.Reset();
            }

            // 3. Se guarda el json
            _currData = new DataToSave(NumHintsDefault, false, _categories, _colorThemes, _colorThemes[0]);
            _currData.SetHash(SecureManager.Hash(JsonUtility.ToJson(_currData)));
            string json = JsonUtility.ToJson(_currData);
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