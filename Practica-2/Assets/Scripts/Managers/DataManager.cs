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
    private Category [] categories;

    [SerializeField]
    private string hash;

    [SerializeField]
    public DataToSave(int _numHints, bool _premium, Category[] _category)
    {
        numHints = _numHints;
        premium = _premium;
        categories = _category;
    }

    public void SetHash(string _hash)
    {
        hash = _hash;
    }

    public string GetHash()
    {
        return hash;
    }

    public Category[] GetCategories()
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

    private const int numHintsDefault = 2;
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
        Category[] cat = GameManager.instance.categories;

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
        DataToSave currData = new DataToSave(numHintsDefault, false, GameManager.instance.categories);
        currData.SetHash(SecureManager.Hash(JsonUtility.ToJson(currData)));
        string json = JsonUtility.ToJson(currData);
        System.IO.File.WriteAllText(routeToSave + fileName, json);
        return currData;
    }
}
