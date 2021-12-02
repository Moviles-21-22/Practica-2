using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private Map map;
    private Level currLevel;

    [SerializeField]
    private Text numLevel;

    [SerializeField]
    private Text gridName;


    private void Start()
    {
        Init(GameManager.instance.currMap, GameManager.instance.GetCurrLevel());
    }

    private void Init(Map mp, Level level)
    {
        //  TODO : revisar esto
        try
        {
            map = mp;
            currLevel = level;
            numLevel.text = "Nivel " +level.lvl;
            int currPack = level.lvl / 30;
            gridName.text = GameManager.instance.GetCurrentPack().gridNames[currPack];
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

}
