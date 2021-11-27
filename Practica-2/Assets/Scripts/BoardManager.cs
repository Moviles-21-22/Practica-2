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
    private List<Tile> wallTiles = new List<Tile>();
    [SerializeField]
    private Tile tilePrefab;
    private Color[] colors = { Color.red, Color.blue, Color.green,
                               Color.magenta, Color.cyan, Color.yellow,
                               Color.grey, Color.white,
                               new Color(251.0f, 112.0f, 0.0f),
                               new Color(115.0f, 7.0f, 155.0f),
                               new Color(171.0f, 40.0f, 40.0f),
                               new Color(147.0f, 120.0f, 55.0f) };
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
                        inputTile.InitTile(0, new Color(currTileColor.r, currTileColor.g, currTileColor.b, 0.5f));
                        previousDir = new Vector2(-2,-2);
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
                    //  Creamos un rect en donde se ha tocado la pantalla
                    Vector2 touch = Input.GetTouch(0).position;
                    Rect touchRect = new Rect(touch.x, touch.y, 50, 50);
                    
                    //  Nuevo tile detectado en colisión
                    if (!touchRect.Overlaps(currTile.GetRect()))
                    {
                        //  Buscamos el tile entre todas las tiles
                        var dragedTile = GetTileOnCollision(touchRect);
                        if (dragedTile.Key != null && dragedTile.Key != currTile)
                        {
                            Vector2 tilePos = currTile.GetRect().position;
                            Vector2 dir = (dragedTile.Key.GetRect().position - tilePos).normalized;
                           
                            //  Hemos llegado al tile que le corresponde (solución)
                            if (dragedTile.Key.CircleActive() && dragedTile.Key.GetColor() == currTile.GetColor())
                            {
                                print("SOLUCIÓN");
                                //  TODO : puntos, pistas, yoquese
                                if (IsElbow(dir))
                                {
                                    currTile.ActiveElbow(currTileColor, dir, previousDir);
                                    dragedTile.Key.ActiveTail(dir * -1, currTileColor);
                                    currTile = dragedTile.Key;
                                    previousDir = dir;
                                }
                                else if(!currTile.CircleActive())
                                {
                                    currTile.ActiveBridge(dir,currTileColor);
                                    dragedTile.Key.ActiveTail(dir * -1, currTileColor);
                                    currTile = dragedTile.Key;
                                    previousDir = dir;
                                }
                            }
                            // No es un circulo
                            else if(!dragedTile.Key.CircleActive())    
                            {
                                //print("dir " +dir + " / pre " + previousDir);
                                //  El anterior es un circulo
                                if (currTile.CircleActive())
                                {
                                    currTile.ActiveTail(dir,currTileColor);
                                    dragedTile.Key.ActiveTail(dir,currTileColor);
                                    currTile = dragedTile.Key;
                                    previousDir = dir;
                                }
                                //  El anterior no es un circulo
                                else if (!currTile.CircleActive())
                                {
                                    // Es codo
                                    if(IsElbow(dir))
                                    {
                                        currTile.ActiveElbow(currTileColor,dir,previousDir);
                                        dragedTile.Key.ActiveTail(dir, currTileColor);
                                        currTile = dragedTile.Key;
                                        previousDir = dir;
                                    }
                                    else
                                    {
                                        currTile.ActiveBridge(dir, currTileColor);
                                        dragedTile.Key.ActiveTail(dir, currTileColor);
                                        currTile = dragedTile.Key;
                                        previousDir = dir;
                                    }
                                }
                            }
                            //  Es un circulo de otro color
                            else if (dragedTile.Key.CircleActive())
                            {
                                print("MovIncorrecto");
                            }
                        }
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

    public void GiveHint()
    {
        //Todo pistas iniciales
        
    }

    private bool IsElbow(Vector2 dir)
    {
        if (Math.Abs(dir.x + previousDir.y) == 2.0f || Math.Abs(dir.y + previousDir.x) == 2.0f
            || Math.Abs(dir.x - previousDir.y) == 0.0f || Math.Abs(dir.y - previousDir.x) == 0.0f)
        {
            return true;
        }
        return false;
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
        tiles = new Tile[currLevel.numBoardY, currLevel.numBoardX];
        for (int i = 0; i < currLevel.numBoardY; i++)
        {
            for (int j = 0; j < currLevel.numBoardX; j++)
            {
                tiles[i,j] = Instantiate(tilePrefab, initPos, Quaternion.identity);
                tiles[i,j].SetRect(initPos.x,initPos.y);
                initPos.x += w;
            }
            initPos.x = posX;
            initPos.y -= h;
        }
        initCircles(currLevel);
        initWalls(currLevel);
    }

    //  Inicializa los circulos del nivel
    private void initCircles(Level currLevel)
    {
        for (int i = 0; i < currLevel.solutions.Count; i++)
        {
            //Cabeza de la tuberia
            float firstElemt = currLevel.solutions[i][0];
            int filaA = (int)((firstElemt + 1) / currLevel.numBoardX);
            int colA = (int)((firstElemt + 1) % currLevel.numBoardX) - 1;
            if (colA < 0)
            {
                colA = currLevel.numBoardX - 1;
                filaA -= 1;
            }
            //print("colA: " + colA + " filaA: " + filaA);
            tiles[filaA,colA].InitTile(i, colors[i]);
            circleTiles.Add(tiles[filaA,colA]);

            //Final de la tuberia
            float secElement = currLevel.solutions[i][currLevel.solutions[i].Count - 1];
            int filaB = (int)((secElement + 1) / currLevel.numBoardX);
            int colB = (int)((secElement + 1) % currLevel.numBoardX) - 1;
            if (colB < 0)
            {
                colB = currLevel.numBoardX - 1;
                filaB -= 1;
            }
            tiles[filaB, colB].InitTile(i, colors[i]);
            circleTiles.Add(tiles[filaB, colB]);
        }
    }

    //Pone los muros del nivel
    private void initWalls(Level currLevel)
    {
        for (int i = 0; i < currLevel.walls.Count; i++)
        {
            //Cogemos los dos tiles adyacentes al muro
            int firstElemt = currLevel.walls[i][0];
            int secElemt = currLevel.walls[i][1];

            int fila = (int)((firstElemt + 1) / currLevel.numBoardX);
            int colm = (int)((firstElemt + 1) % currLevel.numBoardX) - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }
            //Encontramos en que direccion se encuentra el muro respecto a firstElemt
            if (firstElemt > secElemt + 1)          //Muro encima
                tiles[fila, colm].ActiveWall(0);
            else if (firstElemt == secElemt + 1)    //Muro a la derecha
                tiles[fila, colm].ActiveWall(1);
            else if (firstElemt < secElemt - 1)     //Muro debajo
                tiles[fila, colm].ActiveWall(2);
            else if (firstElemt == secElemt - 1)    //Muro a la izquierda
                tiles[fila, colm].ActiveWall(3);

            wallTiles.Add(tiles[fila, colm]);
        }
    }
}
