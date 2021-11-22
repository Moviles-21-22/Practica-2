using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;        //  Para leer un txt
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //  Tiles del tablero
    private Tile[,] tiles;
    private Vector2Int size;
    private List<Tile> circleTiles = new List<Tile>();
    [SerializeField]
    private Tile tilePrefab;
    private Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };
    private Tile currTile;
    private Color currTileColor;
    private Vector2 originPoint;
    private Vector2 previousDir;
    private Tile inputTile;

    public void Start()
    {
        GameManager.instance.levelManager.setBoardManager(this);
        inputTile = Instantiate(tilePrefab,transform);
        Vector3 scale = inputTile.transform.localScale;
        scale.x = 2;
        scale.y = 2;
        inputTile.transform.localScale = scale;
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
                    var pair = GetCircleTileOnCollision(touchRect);
                    if (pair.Key != null)
                    {
                        currTile = pair.Key;
                        int index = pair.Value;
                        originPoint = circleTiles[index].GetRect().position;
                        currTileColor = currTile.GetColor();
                        currTile.Touched();
                        inputTile.GetCircleRender().enabled = true;
                        inputTile.SetColor(0, new Color(currTileColor.r, currTileColor.g, currTileColor.b, 0.5f));
                    }
                }
            }
            //  Toque sale
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (currTile != null)
                {
                    currTile = null;
                }

            }
            //  Arrastrando el dedo por la pantalla
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                if (currTile != null)
                {
                    Vector2 touch = Input.GetTouch(0).position;
                    Rect touchRect = new Rect(touch.x, touch.y, 50, 50);

                    if (!collision(currTile.GetRect(), touchRect))// No sigue en contacto con el mismo tile
                    {
                        //  Buscamos el tile
                        var dragedTile = GetTileOnCollision(touchRect);
                        if (dragedTile.Key != null)
                        {
                            Vector2 dir = (dragedTile.Key.GetRect().position - originPoint).normalized;
                            if (!currTile.EmptyTile())
                                currTile.LeaveTouchTile(dir, currTileColor);
                            //  Codo detectado
                            print(dir);
                            if (Mathf.Abs(dir.x) < 1.0f && Mathf.Abs(dir.x) > 0.0f || Mathf.Abs(dir.y) < 1.0f && Mathf.Abs(dir.y) > 0.0f)
                            {
                                Vector2 currDir = (dragedTile.Key.GetRect().position - currTile.GetRect().position).normalized;
                                currTile.SetElbow(currTileColor, currDir, previousDir);
                                currTile = dragedTile.Key;
                                currTile.LeaveTouchTile(currDir, currTileColor);
                            }
                            else
                            {
                                //  Activamos el bridge del anterior
                                currTile.RemoveTail();
                                currTile = dragedTile.Key;
                                currTile.LeaveTouchTile(dir, currTileColor);
                            }
                            previousDir = dir;

                        }
                    }
                    else
                    {

                    }
                }
            }

            if (currTile != null)
            {
                inputTile.GetCircleRender().enabled = true;
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                inputTile.transform.position = pos;
            }
            else
            {
                inputTile.GetCircleRender().enabled = false;
            }
        }
    }

    //  Determina si dos rects colisionan
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

    //  Busca entre los circulos cual ha sido pulsado
    private KeyValuePair<Tile,int> GetCircleTileOnCollision(Rect touchRect)
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
        return new KeyValuePair<Tile, int>(tile, cont);
    }

    //  Busca entre todas las tiles cual ha sido pulsada y si es un tile en un limite
    private KeyValuePair<Tile, bool> GetTileOnCollision(Rect touchRect)
    {
        bool collisionDetected = false;
        int x = 0;
        int y = 0;
        Tile tile = null;
        Rect tileRect;
        bool limit = false;
        while (!collisionDetected && y < size.y)
        {
            tileRect = tiles[x, y].GetRect();
            if (collision(tileRect, touchRect))
            {
                collisionDetected = true;
                tile = tiles[x,y];
                limit = ((x == size.x - 1 || x == 0) && (y == size.y - 1 || y == 0));
            }
            else
            {
                x++;
                if (x >= size.x)
                {
                    x = 0;
                    y++;
                }
            }
        }
        

        return new KeyValuePair<Tile, bool>(tile, limit);
    }

    //  Inicializa el nivel actual
    public void init(Level currLevel)
    {
        size.x = currLevel.numBoardX;
        size.y = currLevel.numBoardY;
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

    //  Inicializa los circulos del nivel
    private void initCircles(Level currLevel)
    {
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
