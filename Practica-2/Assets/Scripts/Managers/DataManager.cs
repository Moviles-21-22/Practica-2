using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CategoryToSave
{
    [SerializeField]
    Category cat;

    public CategoryToSave(Category _cat)
    {
        cat = _cat;
    }
}

[System.Serializable]
public class DataToSave
{
    [SerializeField]
    private int numHints;

    [SerializeField]
    private bool premium;

    [SerializeField]
    private List<ColorPack> themes;

    [SerializeField]
    private ColorPack currTheme;

    public List<Category> categories;
    
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

    public void SetHash(string _hash)
    {
        hash = _hash;
    }

    public string GetHash()
    {
        return hash;
    }

    public List<Category> GetCategories()
    {
        return categories;
    }

    public int GetNumHints()
    {
        return numHints;
    }

    public bool GetPremiumStatus()
    {
        return premium;
    }

    public List<ColorPack> GetThemes()
    {
        return themes;
    }

    public ColorPack GetCurrentTheme()
    {
        return currTheme;
    }
}


public class DataManager : MonoBehaviour
{
    // TODO Poner algo chulo de clave 
    private string key = "asdladawdajdw";
    //  Nombre del props
    private string fileName = "props.json";
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
            Load();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //  Guardamos en un json toda la información necesaria
    public void Save()
    {
        //  Serializamos los posibles datos que pueden haber cambiado
        DataToSave objToSave = new DataToSave(  GameManager.instance.GetNumHints(),
                                                GameManager.instance.IsPremium(),
                                                GameManager.instance.GetCategories(),
                                                GameManager.instance.GetThemes(),
                                                GameManager.instance.GetCurrTheme());
        //  Creamos el hash
        objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));
        //  Escribimos en el json
        string json = JsonUtility.ToJson(objToSave);
        if (!Directory.Exists(routeToSave)) 
        {
            Directory.CreateDirectory(routeToSave);
            Debug.Log("El directorio " + routeToSave + " se ha creado correctamente");
        }

        if (System.IO.File.Exists(routeToSave + fileName))
        {
            System.IO.File.Delete(routeToSave + fileName);
        }
        System.IO.File.WriteAllText(routeToSave + fileName, json);
    }

    //  Carga el json con la información necesaria para cargar un usuario
    public void Load()
    {
        // Si existe el props
        if (System.IO.File.Exists(routeToSave + fileName))
        {
            string json = string.Empty;
            DataToSave objToLoad;
            try
            {
                json = System.IO.File.ReadAllText(routeToSave + fileName);
                objToLoad = JsonUtility.FromJson<DataToSave>(json);
            }
            catch (System.Exception e)
            {
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
                print("Datos verificados...");
                GameManager.instance.InitDataLoaded(objToLoad);
            }
            else
            {
                Debug.LogWarning("Datos corruptos, creando unos por defecto...");
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

    //  Crea el json props y categorias por defecto
    private DataToSave CreateDefaultJson()
    {
        print("Props por defecto");
        try
        {
            //  Creamos el directorio
            if (!Directory.Exists(routeToSave))
            {
                Directory.CreateDirectory(routeToSave);
            }
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
            File.WriteAllText(routeToSave + "errorLog.txt",e.Message);
            throw new Exception("Reinstala la app");
        }
    }
}
