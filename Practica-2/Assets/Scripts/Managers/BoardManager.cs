using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //  Pool para los tiles creados
    public RectTransform pool;
    //  Tiles del tablero
    private Tile[,] tiles;
    private Vector2Int size;
    //  Circulos instanciados en el tablero
    private List<Tile> circleTiles = new List<Tile>();
    private List<Tile> wallTiles = new List<Tile>();
    [SerializeField]
    private Tile tilePrefab;
    //  Deben tener el mismo orden que los tilesColor de Tile
    private Color[] colors = { Color.red, Color.blue, Color.green,
                               Color.magenta, Color.cyan, Color.yellow,
                               Color.grey, Color.white,
                               new Color(251.0f, 112.0f, 0.0f),
                               new Color(115.0f, 7.0f, 155.0f),
                               new Color(171.0f, 40.0f, 40.0f),
                               new Color(147.0f, 120.0f, 55.0f) };
    //  Ultimo tile que han tocado, hasta que el dedo se levante
    private Tile currTile;
    //  Color del tile tocado
    private Color currTileColor;
    private Vector2 originPoint;
    //  Dirección del movimiento anterior
    private Vector2 previousDir;
    //  Tile del puntero 
    private Tile inputTile;

    //private List<Tile> currMovement = new List<Tile>();
    //private List<List<Tile>> movements = new List<List<Tile>>();

    //No hace falta hacerlo así, se puede hacer con una lista de listas ordenada por color
    private struct ColorMovements
    {
        //Tile tiene los colores asociados a su numero
        int color;
        List<Tile> movements;

        public ColorMovements(int c)
        {
            color = c;
            movements = new List<Tile>();
        }

        public void AddMov(Tile t)
        {
            movements.Add(t);
        }

        public List<Tile> GetMovements()
        {
            return movements;
        }

        public void ClearUntilTile(Tile t)
        {
            bool rem = true;
            while (rem && movements.Count > 0)
            {
                Tile ultTile = movements[movements.Count - 1];
                if (ultTile == t)
                {
                    //Hemos encontrado el tile desde el que queremos mantener el camino
                    rem = false;
                }
                else
                {
                    movements.Remove(ultTile);
                }
            }
        }
    }

    private List<ColorMovements> cMovements = new List<ColorMovements>();

    private bool inputDown = false;
    private Vector3 initPosInput = new Vector2(-1, -1);
    private Level currLevel;

    //  Setea al board y activa el puntero
    public void Start()
    {
        currLevel = GameManager.instance.GetCurrLevel();
        Init();
        inputTile = Instantiate(tilePrefab, transform);
        Vector3 scale = inputTile.transform.localScale;
        scale.x = 2;
        scale.y = 2;
        inputTile.transform.localScale = scale;
    }

    public void Update()
    {
#if UNITY_EDITOR
        //  Ratón se pulsa
        if (Input.GetMouseButtonDown(0))
        {
            inputDown = true;
            initPosInput = Input.mousePosition;

            InputDown(Input.mousePosition);
        }
        //  El ratón se ha levantado
        else if (Input.GetMouseButtonUp(0))
        {
            inputDown = false;
            InputUp();
        }
        //  El ratón se está moviendo
        else if (inputDown && initPosInput != Input.mousePosition)
        {
            InputMoving(Input.mousePosition);
            ProcessPointer(Input.mousePosition);
        }
#endif

#if UNITY_ANDROID

        for (int i = 0; i < Input.touchCount; i++)
        {
            //  Toque empieza            
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                InputDown(Input.GetTouch(i).position);
            }
            //  Toque sale
            else if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                InputUp();
            }
            //  Arrastrando el dedo por la pantalla
            else if (Input.GetTouch(i).phase == TouchPhase.Moved)
            {
                InputMoving(Input.GetTouch(i).position);
                ProcessPointer(Input.GetTouch(i).position);
            }
        }
#endif
    }

    //  Determina si dos rects colisionan
    private bool Collision(Rect a, Rect b)
    {
        // AABB
        // return (b.xMin <= a.xMax && b.xMin >= a.xMin && b.yMax >= a.yMin && b.yMax <= a.yMax)   // Esquina superior izquierda de b
        //     || (b.xMax <= a.xMax && b.xMax >= a.xMin && b.yMax >= a.yMin && b.yMax <= a.yMax)   // Esquina superior derecha de b
        //     || (b.xMin <= a.xMax && b.xMin >= a.xMin && b.yMin >= a.yMin && b.yMin <= a.yMax)   // Esquina inferior izquierda de b
        //     || (b.xMax <= a.xMax && b.xMax >= a.xMin && b.yMin >= a.yMin && b.yMin<= a.yMax)
        //
        //     || (a.xMin <= b.xMax && b.xMin >= b.xMin && a.yMax >= b.yMin && a.yMax <= b.yMax)   // Esquina superior izquierda de a
        //     || (a.xMax <= b.xMax && b.xMax >= b.xMin && a.yMax >= b.yMin && a.yMax <= b.yMax)   // Esquina superior derecha de a
        //     || (a.xMin <= b.xMax && b.xMin >= b.xMin && a.yMin >= b.yMin && a.yMin <= b.yMax)   // Esquina inferior izquierda de a
        //     || (a.xMax <= b.xMax && b.xMax >= b.xMin && a.yMin >= b.yMin && a.yMin <= b.yMax);  // Esquina inferior derecha de a
        //

        if (a.x < (b.x + b.width)
            && (a.x + a.width) > b.x
            && a.y < (b.y + b.height)
            && (a.height + a.y) > b.y)
        {
            return true;
        }
        else return false;
    }

    private void InputMoving(Vector2 inputPos)
    {
        if (currTile != null)
        {
            //  Creamos un rect en donde se ha tocado la pantalla
            Rect touchRect = new Rect(inputPos.x, inputPos.y, 50, 50);

            //  El nuevo touch no está colisionando con el actual tile (Es nuevo)
            if (!touchRect.Overlaps(currTile.GetLogicRect()))
            {
                //  Buscamos el tile entre todas las tiles
                var dragedTile = GetTileOnCollision(touchRect);
                //print("draged: " + dragedTile.Value.x + " drY: " + dragedTile.Value.y);

                int c = dragedTile.Key.GetTileColor();

                if (cMovements[c].GetMovements().Count > 0)
                {
                    int countX = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetX();
                    int countY = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetY();

                    //print("countX: " + countX + " countY: " + countY);

                    if (!AreNeighbour(dragedTile.Value.x, dragedTile.Value.y, countY, countX))
                        return;
                }

                if (dragedTile.Key != null && dragedTile.Key != currTile)
                {
                    //  Dirección entre el nuevo tile y el anterior
                    Vector2 dir = (dragedTile.Key.GetLogicRect().position - currTile.GetLogicRect().position).normalized;

                    //  Hemos llegado al tile que le corresponde (solución)
                    if (dragedTile.Key.CircleActive() && dragedTile.Key.GetColor() == currTile.GetColor())
                    {
                        print("SOLUCIÓN");
                        //  Es un codo
                        if (IsElbow(dir))
                        {
                            currTile.ActiveElbow(currTileColor, dir, previousDir);
                        }
                        else if (!currTile.CircleActive())
                        {
                            currTile.ActiveBridge(dir, currTileColor);
                        }

                        dragedTile.Key.ActiveTail(dir * -1, currTileColor);
                        currTile = dragedTile.Key;
                        //currMovement.Add(currTile);
                        cMovements[c].AddMov(dragedTile.Key);
                        previousDir = dir;
                    }
                    //  No es un circulo
                    else if (!dragedTile.Key.CircleActive())
                    {
                        //print("dir " +dir + " / pre " + previousDir);
                        //  El anterior es un circulo
                        if (currTile.CircleActive())
                        {
                            currTile.ActiveTail(new Vector2(dir.x, -dir.y), currTileColor);
                            dragedTile.Key.ActiveTail(dir, currTileColor);
                            currTile = dragedTile.Key;
                            //currMovement.Add(currTile);
                            cMovements[c].AddMov(dragedTile.Key);
                            previousDir = dir;
                        }
                        //  El anterior no es un circulo
                        else if (!currTile.CircleActive())
                        {
                            // Es codo
                            if (IsElbow(dir))
                            {
                                currTile.ActiveElbow(currTileColor, dir, previousDir);
                                dragedTile.Key.ActiveTail(dir, currTileColor);
                                currTile = dragedTile.Key;
                                //currMovement.Add(currTile);
                                cMovements[c].AddMov(dragedTile.Key);
                                previousDir = dir;
                            }
                            else
                            {
                                currTile.ActiveBridge(dir, currTileColor);
                                dragedTile.Key.ActiveTail(dir, currTileColor);
                                currTile = dragedTile.Key;
                                //currMovement.Add(currTile);
                                cMovements[c].AddMov(dragedTile.Key);
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

    private bool AreNeighbour(int x1, int y1, int x2, int y2)
    {
        if ((x1 == x2 && (y1 == y2 - 1 || y1 == y2 + 1)) ||
            (y1 == y2 && (x1 == x2 - 1 || x1 == x2 + 1)))
            return true;
        else
            return false;
    }

    private void ProcessPointer(Vector2 inputPos)
    {
        if (currTile != null)
        {
            inputTile.GetCircleRender().enabled = true;
            Vector2 pos = Camera.main.ScreenToWorldPoint(inputPos);
            inputTile.transform.position = pos;
        }
        else
        {
            inputTile.GetCircleRender().enabled = false;
        }
    }

    private void InputUp()
    {
        if (currTile != null)
        {
            int c = currTile.GetTileColor();
            currTile = null;
            SolveMovement(c);
            inputTile.GetCircleRender().enabled = false;
        }
    }

    private void InputDown(Vector2 inputPos)
    {
        if (currTile == null)
        {
            Rect touchRect = new Rect(inputPos.x, inputPos.y, 50, 50);
            var pair = GetTileOnCollision(touchRect);
            if (pair.Key != null)
            {
                currTile = pair.Key;
                Vector2Int index = pair.Value;
                originPoint = tiles[index.x, index.y].GetLogicRect().position;
                currTileColor = currTile.GetColor();
                currTile.Touched();
                //currMovement.Add(currTile);
                int c = currTile.GetTileColor();
                cMovements[c].AddMov(currTile);
                //  Para el puntero en pantalla
                inputTile.GetCircleRender().enabled = true;
                inputTile.InitTile(0, new Color(currTileColor.r, currTileColor.g, currTileColor.b, 0.5f));
                previousDir = new Vector2(-2, -2);
            }
        }
    }

    private void SolveMovement(int c)
    {
        foreach (Tile tile in cMovements[c].GetMovements())
        {
            tile.ActiveBgColor(true, currTileColor);
        }
        cMovements[c].GetMovements().Clear();
    }

    public void GiveHint()
    {
        //Todo pistas iniciales

    }

    //  Determina si el tile anterior es un codo
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
    private KeyValuePair<Tile, int> GetCircleTileOnCollision(Rect touchRect)
    {
        bool collisionDetected = false;
        int cont = 0;
        Tile tile = null;
        Rect tileRect;
        while (!collisionDetected && cont < circleTiles.Count)
        {
            tileRect = circleTiles[cont].GetLogicRect();
            if (Collision(tileRect, touchRect))
            {
                collisionDetected = true;
                tile = circleTiles[cont];
            }
            else cont++;
        }
        return new KeyValuePair<Tile, int>(tile, cont);
    }

    //  Busca entre todas las tiles cual ha sido pulsada y la devuelve con su indice
    private KeyValuePair<Tile, Vector2Int> GetTileOnCollision(Rect touchRect)
    {
        bool collisionDetected = false;
        int x = 0;
        int y = 0;
        Tile tile = null;
        Rect tileRect;
        //bool limit = false;
        while (!collisionDetected && y < size.y)
        {
            tileRect = tiles[y, x].GetLogicRect();
            if (Collision(tileRect, touchRect))
            {
                collisionDetected = true;

                if (!tiles[y, x].GetEmpty())
                    tile = tiles[y, x];
                //limit = ((x == size.x - 1 || x == 0) && (y == size.y - 1 || y == 0));
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
        return new KeyValuePair<Tile, Vector2Int>(tile, !collisionDetected ? new Vector2Int(-1, -1) : new Vector2Int(x, y));
    }

    //  Inicializa el nivel actual
    public void Init()
    {
        float poolW = pool.rect.width;
        float poolH = pool.rect.height;

        size.x = currLevel.numBoardX;
        size.y = currLevel.numBoardY;
        float posX = 0;
        float posY = 0;
        float tileW = poolW / size.x;
        float tileH = poolH / size.y;

        Vector2 initPos = new Vector2(posX, posY);
        tiles = new Tile[currLevel.numBoardY, currLevel.numBoardX];
        for (int i = 0; i < currLevel.numBoardY; i++)
        {
            for (int j = 0; j < currLevel.numBoardX; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, initPos, Quaternion.identity, pool);
                tiles[i, j].SetSize(tileW, tileH);
                tiles[i, j].SetLocalGraphicPos(initPos.x, initPos.y);
                tiles[i, j].InitLogicalRect();
                tiles[i, j].SetX(i);
                tiles[i, j].SetY(j);
                initPos.x += tileW;
            }
            initPos.x = posX;
            initPos.y -= tileH;
        }
        initCircles(currLevel);
        initGaps(currLevel);
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
            tiles[filaA, colA].InitTile(i, colors[i], filaA, colA);
            circleTiles.Add(tiles[filaA, colA]);

            //Inicializamos la lista de movimientos de este color
            ColorMovements c = new ColorMovements(i);
            cMovements.Add(c);
            //List<Tile> t = new List<Tile>();
            //movements.Add(t);

            //Final de la tuberia
            float secElement = currLevel.solutions[i][currLevel.solutions[i].Count - 1];
            int filaB = (int)((secElement + 1) / currLevel.numBoardX);
            int colB = (int)((secElement + 1) % currLevel.numBoardX) - 1;
            if (colB < 0)
            {
                colB = currLevel.numBoardX - 1;
                filaB -= 1;
            }
            tiles[filaB, colB].InitTile(i, colors[i], filaB, colB);
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

        //Ponemos los muros que rodean el tablero, si es que lo pide el nivel
        if (currLevel.closed)
        {
            //Para ello recorremos todo el contorno del tablero
            //Primera fila del tablero → muro por encima
            for (int i = 0; i < currLevel.numBoardX; ++i)
            {
                if (!tiles[0, i].GetEmpty())
                    tiles[0, i].ActiveWall(0);
            }
            //Ultima columna del tablero → muro por la derecha
            for (int i = 0; i < currLevel.numBoardY; ++i)
            {
                if (!tiles[i, currLevel.numBoardX - 1].GetEmpty())
                    tiles[i, currLevel.numBoardX - 1].ActiveWall(1);
            }
            //Ultima fila del tablero → muro por debajo
            for (int i = 0; i < currLevel.numBoardX; ++i)
            {
                if (!tiles[currLevel.numBoardY - 1, i].GetEmpty())
                    tiles[currLevel.numBoardY - 1, i].ActiveWall(2);
            }
            //Primera columna del tablero → muro por la izquierda
            for (int i = 0; i < currLevel.numBoardY; ++i)
            {
                if (!tiles[i, 0].GetEmpty())
                    tiles[i, 0].ActiveWall(3);
            }
        }
    }

    //Pone los huecos del nivel
    private void initGaps(Level currLevel)
    {
        //Cambiamos la var empty de cada tile si es hueco
        for (int i = 0; i < currLevel.gaps.Count; i++)
        {
            //Cogemos el tile hueco
            int elem = currLevel.gaps[i];

            int fila = (int)((elem + 1) / currLevel.numBoardX);
            int colm = (int)((elem + 1) % currLevel.numBoardX) - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }
            tiles[fila, colm].InitEmptyTile();
        }

        //Recorremos de nuevo los huecos para ponerles los muros necesarios
        for (int i = 0; i < currLevel.gaps.Count; i++)
        {
            //Cogemos el tile hueco
            int elem = currLevel.gaps[i];

            int fila = (int)((elem + 1) / currLevel.numBoardX);
            int colm = (int)((elem + 1) % currLevel.numBoardX) - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }

            if (fila > 0 && !tiles[fila - 1, colm].GetEmpty())          //Muro encima
                tiles[fila, colm].ActiveWall(0);
            if (colm < currLevel.numBoardX - 1 && !tiles[fila, colm + 1].GetEmpty())    //Muro a la derecha
                tiles[fila, colm].ActiveWall(1);
            if (fila < currLevel.numBoardY - 1 && !tiles[fila + 1, colm].GetEmpty())     //Muro debajo
                tiles[fila, colm].ActiveWall(2);
            if (colm > 0 && !tiles[fila, colm - 1].GetEmpty())    //Muro a la izquierda
                tiles[fila, colm].ActiveWall(3);
        }
    }
}