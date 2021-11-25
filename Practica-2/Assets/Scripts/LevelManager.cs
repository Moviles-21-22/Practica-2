using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BoardManager boardManager;

    private Map map;
    private Level currLevel;

    public void Awake()
    {
        GameManager.instance.SetLevelManager(this);
    }

    public void setBoardManager(BoardManager otherBoard)
    {
        boardManager = otherBoard;
        if (boardManager != null)
        {
            boardManager.init(currLevel);
        }
    }

    public void init(string file)
    {
        try
        {
            map = new Map(file);
            //currLevel = map.GetLevel(GameManager.instance.GetCurrentLevel());
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

}
