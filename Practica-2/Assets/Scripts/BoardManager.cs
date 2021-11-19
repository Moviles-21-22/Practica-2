using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;        //  Para leer un txt
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //  Tiles del tablero
    private Tile[,] tiles;
    private List<Tile> circleTiles = new List<Tile>();
    [SerializeField]
    private Tile tilePrefab;
    private Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    public void Start()
    {
        GameManager.instance.levelManager.setBoardManager(this);
    }

    public void Update()
    {
        if (Input.touchCount > 0)
        {
            //  Toque empieza
            print("Toque detectado");
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 touch = Input.GetTouch(0).position;
                float radius = Input.GetTouch(0).radius;
                Rect touchRect = new Rect(touch.x,touch.y,radius,radius);
                bool collisionDetected = false;
                int cont = 0;
                while (!collisionDetected && cont < circleTiles.Count)
                {
                    Rect currTileRect = circleTiles[cont].GetRect();
                    if (currTileRect.x < touchRect.x + touchRect.width
                        && currTileRect.x + currTileRect.width > touchRect.x
                        && currTileRect.y < touchRect.y + touchRect.height
                        && currTileRect.height + currTileRect.y > touchRect.y)
                    {
                        collisionDetected = true;
                        circleTiles[cont].touched();
                    }
                    else cont++;
                }
            }
            //  Toque sale
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                
            }

        }
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
            initPos.x = posX;
            initPos.y -= h;
        }
        initCircles(currLevel);
    }

    private void initCircles(Level currLevel)
    {
        Vector2Int firstInd = new Vector2Int(-1,-1);
        Vector2Int secInd = new Vector2Int(-1,-1);
        for (int i = 0; i < currLevel.solutions.Count; i++)
        {
            float firstElemt = currLevel.solutions[i][0];
            int filaA = (int)((firstElemt + 1) / currLevel.numBoardX);
            int colA = (int)((firstElemt + 1) % currLevel.numBoardX) - 1;

            if (colA < 0)
            {
                colA = currLevel.numBoardX - 1;
                filaA -= 1;
            }
            tiles[filaA,colA].SetColor(i, colors[i]);
            circleTiles.Add(tiles[filaA,colA]);

            float secElement = currLevel.solutions[i][currLevel.solutions[i].Count - 1];
            int filaB = (int)((secElement + 1) / currLevel.numBoardX);
            int colB = (int)((secElement + 1) % currLevel.numBoardX) - 1;
            if (colB < 0)
            {
                colB = currLevel.numBoardY - 1;
                filaB -= 1;
            }
            tiles[filaB, colB].SetColor(i, colors[i]);
            circleTiles.Add(tiles[filaB, colB]);
        }
    }
}
