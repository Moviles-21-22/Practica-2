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
    private bool draging = false;
    private Tile currTile;

    public void Start()
    {
        GameManager.instance.levelManager.setBoardManager(this);
    }

    public void Update()
    {
        if (Input.touchCount > 0)
        {
            //  Toque empieza            
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (currTile == null)
                {
                    Vector2 touch = Input.GetTouch(0).position;
                    Rect touchRect = new Rect(touch.x, touch.y, 50, 50);
                    currTile = GetCircleTileOnCollision(touchRect);
                    if (currTile != null)
                    {
                        draging = true;
                        currTile.touched();
                    }
                }
            }
            //  Toque sale
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                draging = false;
                if (currTile != null)
                {
                    currTile.OutTouch();
                    currTile = null;
                }
                    
            }
            //  Arrastrando el dedo por la pantalla
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                if (currTile != null)
                {
                    Vector2 touch = Input.GetTouch(0).position;
                    float radius = Input.GetTouch(0).radius;
                    Rect touchRect = new Rect(touch.x, touch.y, radius, radius);

                    if (!collision(currTile.GetRect(), touchRect))// No sigue en contacto con el mismo tile
                    {
                        Tile dragedTile = GetTileOnCollision(touchRect);
                        if (dragedTile != null)
                        {
                            currTile = dragedTile;
                            currTile.touched();
                        }
                    }
                }
                //print(Input.GetTouch(0).position);
            }
        }
    }

    private bool collision(Rect a, Rect b)
    {
        if (a.x < (b.x + b.width)
            && (a.x + a.width) > b.x
            && a.y < (b.y + b.height)
            && (a.height + a.y) > b.y)
        {
            return true;
        }
        else return false;
    }

    private Tile GetCircleTileOnCollision(Rect touchRect)
    {
        bool collisionDetected = false;
        int cont = 0;
        Tile tile = null;
        Rect tileRect;
        while (!collisionDetected && cont < circleTiles.Count)
        {
            tileRect = circleTiles[cont].GetRect();
            if (collision(tileRect, touchRect))
            {
                collisionDetected = true;
                tile = circleTiles[cont];
            }
            else cont++;
        }
        return tile;
    }

    private Tile GetTileOnCollision(Rect touchRect)
    {
        bool collisionDetected = false;
        int x = 0;
        int y = 0;
        Tile tile = null;
        Rect tileRect;
        while (!collisionDetected && y < tiles.Length)
        {
            tileRect = tiles[x, y].GetRect();
            if (collision(tileRect, touchRect))
            {
                collisionDetected = true;
                tile = tiles[x,y];
            }
            else
            {
                x++;
                if (x >= tiles.Length)
                {
                    x = 0;
                    y++;
                }
            }
        }
        return tile;
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
                tiles[i, j].SetRect(initPos.x,initPos.y);
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
