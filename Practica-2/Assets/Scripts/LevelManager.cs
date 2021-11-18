using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BoardManager boardManager;

    private Map map;

    public void init(string file)
    {
        try
        {
            map = new Map(file);
            boardManager.init(map);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

}
