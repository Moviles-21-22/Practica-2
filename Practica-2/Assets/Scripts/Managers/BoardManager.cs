using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//No hace falta hacerlo así, se puede hacer con una lista de listas ordenada por color
/// <summary>
/// Struct auxiliar para controlar el color del tile en función de los movimientos
/// </summary>
struct ColorMovements
{
    //Tile tiene los colores asociados a su numero
    int color;
    List<Tile> movements;
    //  Determina si existe un camino entre ambas esferas
    public bool conected;

    public ColorMovements(int c)
    {
        color = c;
        movements = new List<Tile>();
        conected = false;
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

    public bool IsConected()
    {
        return conected;
    }

    public int GetColor()
    {
        return color;
    }

    internal void Conect()
    {
        this.conected = true;
    }

}

public class BoardManager : MonoBehaviour
{
    [Tooltip("Referencia al objeto padre donde se van a instanciar los tiles")]
    [SerializeField]
    private Transform pool;
    [Tooltip("Referencia al prefab de los tiles que se van a crear")]
    [SerializeField]
    private Tile tilePrefab;
    [Tooltip("Colores de los diferentes flujos que haya en el juego")]
    [SerializeField]
    private Color[] colors;
    [Tooltip("Referencia a los objetos del HUD")]
    [SerializeField]
    private RectTransform[] hudRegion;
    [Tooltip("Referencia al GameObject del canvas que se muestra al superar el nivel")]
    [SerializeField]
    private HUDManager hud;

    // Tiles del tablero
    private Tile[,] tiles;
    // Dimensiones del tablero
    private Vector2Int tabSize;
    // Circulos instanciados en el tablero
    private List<Tile> circleTiles = new List<Tile>();
    // Muros instanciados en el tablero
    private List<Tile> wallTiles = new List<Tile>();
    // Ultimo tile que han tocado, hasta que el dedo se levante
    private Tile currTile;
    // Color del tile tocado
    private Color currTileColor;
    // Dirección del movimiento anterior
    private Vector2 previousDir;
    // Tile del puntero 
    private Tile inputTile;
    // Número de flujos completados
    private int conFlows = 0;

    private List<ColorMovements> cMovements = new List<ColorMovements>();

    private bool inputDown = false;
    private Vector3 initPosInput = new Vector2(-1, -1);
    private Level currLevel;

    //  Setea al board y activa el puntero
    public void Start()
    {
        currLevel = GameManager.instance.GetCurrLevel();
        conFlows = 0;

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
        return a.Overlaps(b);

        //AABB
        //return (b.xMin <= a.xMax && b.xMin >= a.xMin && b.yMax >= a.yMin && b.yMax <= a.yMax)   // Esquina superior izquierda de b
        //    || (b.xMax <= a.xMax && b.xMax >= a.xMin && b.yMax >= a.yMin && b.yMax <= a.yMax)   // Esquina superior derecha de b
        //    || (b.xMin <= a.xMax && b.xMin >= a.xMin && b.yMin >= a.yMin && b.yMin <= a.yMax)   // Esquina inferior izquierda de b
        //    || (b.xMax <= a.xMax && b.xMax >= a.xMin && b.yMin >= a.yMin && b.yMin <= a.yMax)

        //    || (a.xMin <= b.xMax && b.xMin >= b.xMin && a.yMax >= b.yMin && a.yMax <= b.yMax)   // Esquina superior izquierda de a
        //    || (a.xMax <= b.xMax && b.xMax >= b.xMin && a.yMax >= b.yMin && a.yMax <= b.yMax)   // Esquina superior derecha de a
        //    || (a.xMin <= b.xMax && b.xMin >= b.xMin && a.yMin >= b.yMin && a.yMin <= b.yMax)   // Esquina inferior izquierda de a
        //    || (a.xMax <= b.xMax && b.xMax >= b.xMin && a.yMin >= b.yMin && a.yMin <= b.yMax);  // Esquina inferior derecha de a


        /*        if (a.x < (b.x + b.width)
                    && (a.x + a.width) > b.x
                    && a.y < (b.y + b.height)
                    && (a.height + a.y) > b.y)
                {
                    return true;
                }
                else return false;*/
    }

    private void InputMoving(Vector2 inputPos)
    {
        if (currTile != null)
        {
            //  Creamos un rect en donde se ha tocado la pantalla
            Rect touchRect = new Rect(inputPos.x, inputPos.y, 1, 1);

            //  El nuevo touch no está colisionando con el actual tile (Es nuevo)
            if (!touchRect.Overlaps(currTile.GetLogicRect()))
            {
                //  Buscamos el tile entre todas las tiles
                var dragedTile = GetTileOnCollision(touchRect);
                if (dragedTile.Key)
                {
                    //print($"TILE: {dragedTile.Value}");
                }

                //print("draged: " + dragedTile.Value.x + " drY: " + dragedTile.Value.y);

                int c = currTile.GetTileColor();
                //Debug.Log("C " + c);

                if (cMovements[c].GetMovements().Count > 0)
                {
                    int countX = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetX();
                    int countY = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetY();

                    //print("countX: " + countX + " countY: " + countY + " dragedX: " + dragedTile.Value.x + " dragedY: " + dragedTile.Value.y);

                    if (!AreNeighbour(dragedTile.Value.x, dragedTile.Value.y, countX, countY))
                        return;
                }

                if (dragedTile.Key != null && dragedTile.Key != currTile)
                {
                    //  Dirección entre el nuevo tile y el anterior
                    Vector2 dir = (dragedTile.Key.GetLogicRect().position - currTile.GetLogicRect().position).normalized;

                    //  Hemos llegado al tile que le corresponde (solución)
                    // TODO: Revisar ese if para que se sume el nivel completado cuando corresponda
                    if (dragedTile.Key.CircleActive() && dragedTile.Key.GetColor() == currTile.GetColor())
                    {
                        conFlows++;
                        if (conFlows == currLevel.numFlow)
                        {
                            GameManager.instance.AddSolutionLevel(true);
                            hud.LevelCompleted(true);
                        }
                        else if(conFlows < currLevel.numFlow)
                        {
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
                            dragedTile.Key.SetX(dragedTile.Value.x);
                            dragedTile.Key.SetY(dragedTile.Value.y);
                            dragedTile.Key.SetTileColor(c);
                            currTile = dragedTile.Key;
                            cMovements[c].AddMov(dragedTile.Key);
                            previousDir = dir;
                        }
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
                            dragedTile.Key.SetX(dragedTile.Value.x);
                            dragedTile.Key.SetY(dragedTile.Value.y);
                            dragedTile.Key.SetTileColor(c);
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
                                dragedTile.Key.SetX(dragedTile.Value.x);
                                dragedTile.Key.SetY(dragedTile.Value.y);
                                dragedTile.Key.SetTileColor(c);
                                currTile = dragedTile.Key;
                                //currMovement.Add(currTile);
                                cMovements[c].AddMov(dragedTile.Key);
                                previousDir = dir;
                            }
                            else
                            {
                                currTile.ActiveBridge(dir, currTileColor);
                                dragedTile.Key.ActiveTail(dir, currTileColor);
                                dragedTile.Key.SetX(dragedTile.Value.x);
                                dragedTile.Key.SetY(dragedTile.Value.y);
                                dragedTile.Key.SetTileColor(c);
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
            Rect touchRect = new Rect(inputPos.x, inputPos.y, 1, 1);
            var pair = GetTileOnCollision(touchRect);
            if (pair.Key != null)
            {
                currTile = pair.Key;
                Vector2Int index = pair.Value;
                //originPoint = tiles[index.x, index.y].GetLogicRect().position;
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
        if (GameManager.instance.GetNumHints() > 0)
        {
            List<int> movs = new List<int>();
            int cont = 0;
            foreach (ColorMovements cM in cMovements)
            {
                if (!cM.IsConected()) 
                {
                    int cl = cM.GetColor();
                    if(cl != -1)
                        movs.Add(cont);
                }
                cont++;
            }
            if (movs.Count > 0)
            {
                int elc = UnityEngine.Random.Range(0,movs.Count - 1);
                int choice = movs[elc];
                var d = cMovements[choice];
                d.Conect();
                cMovements[choice] = d;

                List<int> currSolution = GameManager.instance.GetCurrLevel().solutions[choice];
                Vector2Int ind = ConvertIndex(currSolution[0]);
                Color color = tiles[ind.y, ind.x].GetColor();
                for (int i = 0; i < currSolution.Count; i++)
                {
                    Vector2Int index = ConvertIndex(currSolution[i]);
                    cMovements[choice].AddMov(tiles[index.y,index.x]);
                    if (i == 0 || i == currSolution.Count - 1)
                    {
                        tiles[index.y, index.x].ActiveStar(true);
                    }
                    else
                    {
                        
                        tiles[index.y, index.x].ActiveTail(Vector2.down,color);
                    }
                }
                GameManager.instance.UseHint();
            }
        }
    }

    private Vector2Int ConvertIndex(int index)
    {
        int x = (int)((index + 1) % currLevel.numBoardX) - 1;
        int y = (int)((index + 1) / currLevel.numBoardX);
        if (x < 0)
        {
            x = currLevel.numBoardX - 1;
            y -= 1;
        }
        return new Vector2Int(x,y);
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
        while (!collisionDetected && y < tabSize.y)
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
                if (x >= tabSize.x)
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
        // Creación de los tiles
        tabSize.x = currLevel.numBoardX;
        tabSize.y = currLevel.numBoardY;
        Vector2 initPos = Vector2.zero;

        tiles = new Tile[currLevel.numBoardY, currLevel.numBoardX];
        for (int i = 0; i < currLevel.numBoardY; i++)
        {
            for (int j = 0; j < currLevel.numBoardX; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, pool);
                tiles[i, j].transform.position = initPos;
                initPos.x += 1;
            }
            initPos.x = 0;
            initPos.y -= 1;
        }

        initCircles(currLevel);
        initGaps(currLevel);
        initWalls(currLevel);

        // Escalado del tablero
        var cam = Camera.main;

        // Unidades de Unity
        var a = hudRegion[0].sizeDelta;
        var b = hudRegion[1].sizeDelta;

        float camH = cam.orthographicSize * 2.0f;
        float camW = camH * cam.aspect;

        float tileH = camH / tabSize.y;
        float tileW = camW / tabSize.x;

        float tileAspect = tileH >= tileW ? tileW : tileH;

        pool.localScale = Vector2.one * tileAspect;
        // Hay que tener en cuenta que la cámara está situada en el (0, 0) y el tablero también,
        // por tanto, se estará viendo un cacho del tablero y habría que desplazar el tablero o la cámara.
        // Optamos por la cámara.

        // A partir de la mitad del ancho de la cámara se calculan el número de tiles visibles sin desplazamiento
        float numTilesX = (camW / 2) / tileW;
        float numTilesY = (camH / 2) / tileH;

        // El offset de desplazamiento para la cámara será el número de tiles que no se vean por el tamaño de tile
        // Se le resta 0.5f porque el pivote de los objetos está en el centro, es decir, nos sobra la mitad de un tile
        // que en unidades de Unity es 0.5f
        float offsetX = (tabSize.x - numTilesX - 0.5f) * tileAspect;
        float offsetY = (tabSize.y - numTilesY - 0.5f) * tileAspect;

        cam.gameObject.transform.position = new Vector2(offsetX, -offsetY);

        // Inicializacion de los rectangulos logicos de cada tile
        for (int i = 0; i < currLevel.numBoardY; i++)
        {
            for (int j = 0; j < currLevel.numBoardX; j++)
            {
                tiles[i, j].InitLogicalRect(j, i);
            }
        }

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
            tiles[filaA, colA].InitTile(i, colors[i], colA, filaA);
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
            tiles[filaB, colB].InitTile(i, colors[i], colB, filaB);
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