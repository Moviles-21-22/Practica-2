using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class DataToSave
{
    [SerializeField]
    private int numHints;

    [SerializeField]
    private bool premium;

    [SerializeField]
    public List<Category> categories;
    
    [SerializeField]
    public string data;

    [SerializeField]
    private string hash;

    [SerializeField]
    public DataToSave(int _numHints, bool _premium, List<Category> _category)
    {
        numHints = _numHints;
        premium = _premium;
        categories = _category;
        data = _category.ToString();
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
}


public class DataManager : MonoBehaviour
{
    private SecureManager secureManager;
    // TODO Poner algo chulo de clave 
    private string key = "asdladawdajdw";
    private string fileName = "props.json";
    private string routeToSave;
    private string routeToPaste;

    private const int numHintsDefault = 2;
    [Tooltip("Lista de categorias por defecto")]
    [SerializeField] List<Category> categories;
    // Rutas para el guardado por defecto
    private string[] saveRoutes;

    private string saveRoute;

    public static DataManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Start()
    {
        routeToSave = Directory.GetCurrentDirectory() + "/Assets/save/";
        secureManager = new SecureManager();
        Load();
    }

    public void Save()
    {
        //  Guardamos los datos
        int numHints = GameManager.instance.GetNumHints();
        bool premium = GameManager.instance.IsPremium();
        List<Category> cat = GameManager.instance.GetCategories();

        DataToSave objToSave = new DataToSave(numHints, premium, cat);
        print(JsonUtility.ToJson(objToSave));

        objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));
        print(objToSave.GetHash());
        string json = JsonUtility.ToJson(objToSave);

        if (!Directory.Exists(routeToSave)) 
        {
            Directory.CreateDirectory(routeToSave);
            Debug.Log("El directorio " + routeToSave + " se ha creado correctamente");
        }

        if (System.IO.File.Exists(routeToSave + fileName))
        {
            System.IO.File.Delete(routeToSave + fileName);
            System.IO.File.WriteAllText(routeToSave + fileName, json);
        }
        else
        {
            System.IO.File.WriteAllText(routeToSave + fileName, json);
        }
    }

    //  Carga el json con la información necesaria para cargar un usuario
    public void Load()
    {
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
                print("Datos verificados");
                GameManager.instance.InitDataLoaded(objToLoad);
            }
            else
            {
                print("Datos corruptos");
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

    //  Crea el json props por defecto
    private DataToSave CreateDefaultJson()
    {
        print("Props por defecto");
        List<Category> categoriesCopy = new List<Category>();
        saveRoute = "Assets/save";
        saveRoutes = new string[categories.Count];
        try
        {
            //  Creación de carpetas y rutas para el guardado de packs
            for (int i = 0; i < categories.Count; i++)
            {
                saveRoutes[i] =  "Assets/save/" + categories[i].name;
                if (!AssetDatabase.IsValidFolder(saveRoute + "/" + categories[i].name))
                {
                    AssetDatabase.CreateFolder(saveRoute, categories[i].name);
                }

                //  Copiamos la categoria
                string categoryOriginalRoute = AssetDatabase.GetAssetPath(categories[i]);
                string categoryCopyRoute = saveRoutes[i] + "/" + categories[i].name + "Copy.asset";

                string statusMov = AssetDatabase.ValidateMoveAsset(categoryOriginalRoute, categoryCopyRoute);
                if (!statusMov.Equals(string.Empty))
                {
                    print("Borro " + categories[i].name);
                    AssetDatabase.DeleteAsset(categoryCopyRoute);
                }
                AssetDatabase.CopyAsset(categoryOriginalRoute, categoryCopyRoute);
                categoriesCopy.Add((Category)AssetDatabase.LoadAssetAtPath(categoryCopyRoute, typeof(Category)));

                //  Copiamos los niveles
                LevelPack[] currPack = new LevelPack[categories[i].levels.Length];
                for (int j = 0; j < categories[i].levels.Length; j++)
                {
                    string originalPackRoute = AssetDatabase.GetAssetPath(categories[i].levels[j]);
                    string copyPackRoute = saveRoutes[i] + "/" + categories[i].levels[j].name + "Copy.asset";
                    string packStatus = AssetDatabase.ValidateMoveAsset(originalPackRoute, copyPackRoute);
                    if (!packStatus.Equals(string.Empty))
                    {
                        print("Borro " + categories[i].levels[j].name);
                        AssetDatabase.DeleteAsset(copyPackRoute);
                    }
                    AssetDatabase.CopyAsset(originalPackRoute, copyPackRoute);
                    currPack[j] = (LevelPack)AssetDatabase.LoadAssetAtPath(copyPackRoute, typeof(LevelPack));
                }
                categoriesCopy[i].levels = currPack;
            }
        }
        catch (Exception e)
        {
            throw new Exception("Reinstala la app");
        }
        DataToSave currData = new DataToSave(numHintsDefault, false, categoriesCopy);
        currData.SetHash(SecureManager.Hash(JsonUtility.ToJson(currData)));
        string json = JsonUtility.ToJson(currData);
        System.IO.File.WriteAllText(routeToSave + fileName, json);
        return currData;
    }
}
