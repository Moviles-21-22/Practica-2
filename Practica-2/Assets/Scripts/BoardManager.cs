using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;        //  Para leer un txt
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //  Tiles del tablero
    private Tile[,] tiles;
    [SerializeField]
    private Tile tilePrefab;

    public void Start()
    {
        GameManager.instance.levelManager.setBoardManager(this);
    }

    public void init(Level currLevel)
    {
        float posX = -2;
        float posY = -2;
        float w = 1;
        float h = 1;
        Vector2 initPos = new Vector2(posX,posY);
        tiles = new Tile[currLevel.numBoardX, currLevel.numBoardY];
        for (int i = 0; i < currLevel.numBoardX; i++)
        {
            for (int j = 0; j < currLevel.numBoardY; j++)
            {
                tiles[i,j] = Instantiate(tilePrefab, initPos, Quaternion.identity);
                initPos.x += w;
            }
            initPos.x = -2;
            initPos.y += h;
        }
    }
}
