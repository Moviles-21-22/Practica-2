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
    private string hash;

    [SerializeField]
    public DataToSave(int _numHints)
    {
        numHints = _numHints;
    }

    public void SetHash(string _hash)
    {
        hash = _hash;
    }

    public string GetHash()
    {
        return hash;
    }
}


public class DataManager : MonoBehaviour
{
    private SecureManager secureManager;
    //  Poner algo chulo de clave
    private string clave = "asdladawdajdw";

    public void Start()
    {
        GameManager.instance.SetDataManager(this);
        secureManager = new SecureManager();
    }

    public void Save()
    {
        int numHints = GameManager.instance.GetNumHints();
        DataToSave objToSave = new DataToSave(numHints);
        objToSave.SetHash(SecureManager.Hash(JsonUtility.ToJson(objToSave)));
        string json = JsonUtility.ToJson(objToSave);
        System.IO.File.WriteAllText(Directory.GetCurrentDirectory() + "/holaTiwardo.json", json);
    }

    public void Load()
    {
        string json = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/holaTiwardo.json");
        DataToSave objToLoad = null;
        try
        {
            objToLoad = JsonUtility.FromJson<DataToSave>(json);
        }
        catch (System.Exception e)
        {
            print(e);
        }

        string hashSaved = objToLoad.GetHash();
        string[] hashGenerated = json.Split(',');
        string chain = hashGenerated[0] + "}";
        string hashFromSec = SecureManager.Hash(chain);

        if (hashFromSec.Equals(objToLoad.GetHash()))
        {
            print("IGUALITOS");
        }
        else
        {
            print("HUELE A CACA");
        }
    }

    public void SaveHash(string Json)
    {
        string hashValue = SecureManager.Hash(Json);
    }
}
