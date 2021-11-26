using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private BoardManager boardManager;

    private Map map;
    private Level currLevel;

    [SerializeField]
    private Text numLevel;

    [SerializeField]
    private Text packName;


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

    public void init(Map mp, Level level)
    {
        //  TODO : revisar esto
        try
        {
            map = mp;
            currLevel = level;
            numLevel.text = "Nivel " + GameManager.instance.GetCurrLevel();
            int currPack = GameManager.instance.GetCurrLevel() / 30;
            packName.text = GameManager.instance.GetCurrentPack().gridNames[currPack];
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

}
