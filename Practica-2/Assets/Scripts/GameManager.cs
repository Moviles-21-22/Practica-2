using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private LevelManager levelManager;
    //  Devuelve el nivel actual
    private string CurrLevel;

    public void SetLevelManager(LevelManager otherLevelManager)
    {
        levelManager = otherLevelManager;
        if (levelManager != null)
        {
            levelManager.init(CurrLevel);
        }
    }
}
