using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        try
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets/save","Intro");
            UnityEditor.AssetDatabase.CreateFolder("Assets/save","Mania");
            UnityEditor.AssetDatabase.CreateFolder("Assets/save","Rectangle");

            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Categories/intro.asset", "Assets/save/Intro/introCopy.asset");
            Category c = (Category)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Intro/introCopy.asset", typeof(Category));
            categoriesCopy.Add(c);
            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Categories/Mania.asset", "Assets/save/Mania/maniaCopy.asset");
            c  = (Category)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Mania/maniaCopy.asset", typeof(Category));
            categoriesCopy.Add(c);
            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Categories/rectangles.asset", "Assets/save/Rectangle/rectanglesCopy.asset");
            c = (Category)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Rectangle/rectanglesCopy.asset", typeof(Category));
            categoriesCopy.Add(c);

            LevelPack[] currPack = new LevelPack[4];
            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Levels/Intro/clasicPack.asset", "Assets/save/Intro/clasicPackCopy.asset");
            currPack[0] = (LevelPack)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Intro/clasicPackCopy.asset", typeof(LevelPack));

            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Levels/Intro/bonusPack.asset", "Assets/save/Intro/bonusPackCopy.asset");
            currPack[1] = (LevelPack)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Intro/bonusPackCopy.asset", typeof(LevelPack));

            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Levels/Intro/greenPack.asset", "Assets/save/Intro/greenPackCopy.asset");
            currPack[2] = (LevelPack)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Intro/greenPackCopy.asset", typeof(LevelPack));

            UnityEditor.AssetDatabase.CopyAsset("Assets/Data/Levels/Intro/bluePack.asset", "Assets/save/Intro/bluePackCopy.asset");
            currPack[3] = (LevelPack)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/save/Intro/bluePackCopy.asset", typeof(LevelPack));

            categoriesCopy[0].levels = currPack;
        }
        catch (Exception e)
        {
            print(e.Message);
        }
        DataToSave currData = new DataToSave(numHintsDefault, false, categoriesCopy);
        currData.SetHash(SecureManager.Hash(JsonUtility.ToJson(currData)));
        string json = JsonUtility.ToJson(currData);
        System.IO.File.WriteAllText(routeToSave + fileName, json);
        return currData;
    }
}
