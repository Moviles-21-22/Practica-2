using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct auxiliar para controlar el color del tile en función de los movimientos
/// </summary>
struct ColorMovements
{
    //Tile tiene los colores asociados a su numero
    int color;
    // Lista de tiles ordenada con el camino del flujo
    List<Tile> movements;
    // Lista del último movimiento
    List<Tile> lastMovements;
    //  Determina si existe un camino entre ambas esferas
    public bool conected;

    public ColorMovements(int c)
    {
        color = c;
        movements = new List<Tile>();
        lastMovements = new List<Tile>();
        conected = false;
    }

    public void AddMov(Tile t)
    {
        movements.Add(t);
        lastMovements.Add(t);
    }

    /// <summary>
    /// Limpia la lista con los últimos movimientos realizados
    /// </summary>
    public void ClearLastMovs() 
    {
        lastMovements.Clear();
    }

    public List<Tile> GetMovements()
    {
        return movements;
    }

    /// <summary>
    /// Borra todos los elementos de la lista de movimientos incluido t
    /// Devuelve el numero de elementos borrados
    /// </summary>
    public int ClearUntilTile(Tile t)
    {
        int p = 0;
        bool rem = true;
        while (rem && movements.Count > 0)
        {
            Tile ultTile = movements[movements.Count - 1];

            //Hemos encontrado el tile desde el que queremos mantener el camino
            if (ultTile == t)
            {
                rem = false;
            }

            p++;
            ultTile.ClearTile();
            movements.Remove(ultTile);
        }
        return p;
    }

    /// <summary>
    /// Borra todos los elementos de la lista desde el primero hasta t incluido
    /// Devuelve el numero de elementos borrados
    /// </summary>
    public int ClearFirstUntilTile(Tile t)
    {
        int p = 0, i = 0;
        bool rem = true;
        while (rem && movements.Count > i)
        {
            Tile firstTile = movements[i];
            movements[i] = movements[movements.Count - 1];
            movements[movements.Count - 1] = firstTile;

            //Hemos encontrado el tile desde el que queremos mantener el camino
            if (movements[i] == t)
            {
                rem = false;
            }

            p++;
            movements[movements.Count - 1].ClearTile();
            movements.Remove(movements[movements.Count - 1]);
            ++i;
        }

        //Si sobra algun movimiento se borra
        for (int j = movements.Count; j > i; --j)
        {
            p++;
            movements[movements.Count - 1].ClearTile();
            movements.Remove(movements[movements.Count - 1]);
        }

        return p;
    }

    /// <summary>
    /// Determina si el flujo está conectado o no
    /// </summary>
    public bool IsConected()
    {
        return movements.Count > 1 && movements[movements.Count - 1].CircleActive();
    }

    public int GetColor()
    {
        return color;
    }

    public void Conect()
    {
        this.conected = true;
    }

    /// <summary>
    /// Aplica el efecto de deshacer para cada tile y devuelve
    /// el número de tiles deshechos, sin contar el primero de la lista
    /// en caso de que sea un círculo, pues éste no cuenta para el porcentaje total
    /// </summary>
    public int UndoMovements()
    {
        int p = 0;
        foreach (Tile t in lastMovements) 
        {
            if (t == lastMovements[0] && t.CircleActive()) 
            {
                p--;
            }

            p++;
            t.ClearTile();
        }

        lastMovements.Clear();
        return p;
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
    [Tooltip("Referencia a los objetos del HUD")]
    [SerializeField]
    private RectTransform[] hudRegion;
    [Tooltip("Referencia al GameObject del canvas que se muestra al superar el nivel")]
    [SerializeField]
    private HUDManager hud;

    private List<Color> colors;

    // Determina si se está pulsadno la pantalla
    private bool inputDown = false;
    // Porcentaje de tablero completo
    private float percentage = 0.0f;
    // Porcentaje que se suma al total por cada tile ocupado
    private float plusPercentage = 0.0f;
    // Color del tile tocado
    private Color currTileColor;
    // Dirección del movimiento anterior
    private Vector2 previousDir;
    // Poisición donde se ha pulsado la primera vez
    private Vector3 initPosInput = new Vector2(-1, -1);

    // Ultimo tile que han tocado, hasta que el dedo se levante
    private Tile currTile;
    // Tile que representa el recorrido del puntero 
    private Tile inputTile;
    // Circulos instanciados en el tablero
    private List<Tile> circleTiles = new List<Tile>();
    // Muros instanciados en el tablero
    private List<Tile> wallTiles = new List<Tile>();
    // Tiles del tablero
    private Tile[,] tiles;
    // Dimensiones del tablero
    private Vector2Int tabSize;
    // Struct auxiliar con la información del nivel actual
    private Level currLevel;
    // Lista con los movimientos de cada uno de los flujos
    private List<ColorMovements> cMovements = new List<ColorMovements>();
    // Contador de movimientos
    private int currMovs = 0;

    //  Setea al board y activa el puntero
    public void Start()
    {
        currLevel = GameManager.instance.GetCurrLevel();
        colors = GameManager.instance.GetCurrTheme().colors;
        Init();
        plusPercentage = 100.0f / ((tabSize.x * tabSize.y) - currLevel.numFlow);
        inputTile = Instantiate(tilePrefab, transform);
        inputTile.DesactiveLines();
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
            DrawCirclePointer(Input.mousePosition);
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
                DrawCirclePointer(Input.GetTouch(i).position);
            }
        }
#endif
    }

    #region Inits
    /// <summary>
    /// Inicializa el nivel actual
    /// </summary>
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

        InitCircles(currLevel);
        InitGaps(currLevel);
        InitWalls(currLevel);

        //==============ESCALADO-TABLERO===================//
        // Auxiliar de la cámara principal
        var cam = Camera.main;

        // Está en unidades de Unity
        var a = hudRegion[0].sizeDelta;
        var b = hudRegion[1].sizeDelta;

        // Dimensiones de la cámara, sin tener en cuenta el hud
        float camH = cam.orthographicSize * 2.0f;
        float camW = camH * cam.aspect;

        // Hay que tener en cuenta la altura del hud - Está en píxeles
        float hudOffsetY = (hudRegion[0].rect.height + 10) + (hudRegion[1].rect.height + 10);
        // Conversión a la proporción
        hudOffsetY = hudOffsetY / cam.pixelHeight;

        camH *= (1 - hudOffsetY - 0.05f);    // 0.05f de margen
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

    /// <summary>
    /// Inicializa los círculos de los flujos
    /// </summary>
    private void InitCircles(Level currLevel)
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

    /// <summary>
    /// Inicializa los muros de los tiles que lo requieran
    /// </summary>
    private void InitWalls(Level currLevel)
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

    /// <summary>
    /// Inicializa los huecos de los tiles que lo requieran
    /// </summary>
    private void InitGaps(Level currLevel)
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
    #endregion

    #region InputLogic
    /// <summary>
    /// Lógica que procesa el toque en pantalla
    /// </summary>
    /// <param name="inputPos">Posición del toque en pantalla</param>
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
                // Se limpia la antigua lista de antiguos movimientos
                cMovements[c].ClearLastMovs();
                cMovements[c].AddMov(currTile);
                //  Para el puntero en pantalla
                inputTile.GetCircleRender().enabled = true;
                inputTile.InitTile(currTile.GetTileColor(), new Color(currTileColor.r, currTileColor.g, currTileColor.b, 0.5f));
                previousDir = new Vector2(-2, -2);
            }
        }
    }

    /// <summary>
    /// Lógica que procesa el comportamiento del input arrastrándose por pantalla
    /// </summary>
    /// <param name="inputPos">Posición del toque en pantalla</param>
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

                int c = currTile.GetTileColor();

                if (cMovements[c].GetMovements().Count > 0)
                {
                    int countX = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetX();
                    int countY = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1].GetY();

                    if (!AreNeighbour(dragedTile.Value.x, dragedTile.Value.y, countX, countY))
                        return;
                }

                if (dragedTile.Key != null && dragedTile.Key != currTile)
                {
                    // Dirección entre el nuevo tile y el anterior
                    Vector2 dir = (dragedTile.Key.GetLogicRect().position - currTile.GetLogicRect().position).normalized;

                    //Comprobamos si hace colisión con un muro, en cuyo caso salimos sin hacer nada
                    if (dragedTile.Key.WallCollision(-dir) || currTile.WallCollision(dir))
                        return;

                    // Hemos llegado al tile que le corresponde (solución)
                    // Condiciones: Sea un círculo y sea del mismo color que mi tubería
                    if (dragedTile.Key.CircleActive() && dragedTile.Key.GetColor() == currTile.GetColor())
                    {
                        //Comprobamos si es el mismo círculo con el que empecé o no
                        if (dragedTile.Key != cMovements[c].GetMovements()[0])
                        {
                            dragedTile.Key.PlayParticle();
                            // Es un codo
                            if (IsElbow(dir))
                            {
                                currTile.ActiveElbow(currTileColor, dir, previousDir);
                            }
                            else if (!currTile.CircleActive())
                            {
                                currTile.ActiveBridge(dir, currTileColor);
                            }
                            percentage += plusPercentage;
                            hud.ShowPercentage((int)Math.Round(percentage));

                            dragedTile.Key.ActiveTail(dir * -1, currTileColor);
                            dragedTile.Key.SetX(dragedTile.Value.x);
                            dragedTile.Key.SetY(dragedTile.Value.y);
                            dragedTile.Key.SetTileColor(c);
                            currTile = dragedTile.Key;
                            previousDir = dir;
                        }
                        //Es el circulo con el que comenzamos, se borra toda la tubería
                        else
                        {
                            int p = cMovements[c].ClearUntilTile(dragedTile.Key);
                            percentage -= plusPercentage * p;

                            percentage += plusPercentage;
                            hud.ShowPercentage((int)Math.Round(percentage));

                            currTile = dragedTile.Key;
                            previousDir = -dir;
                        }
                        
                        cMovements[c].AddMov(dragedTile.Key);
                    }
                    // No es un circulo
                    else if (!dragedTile.Key.CircleActive())
                    {
                        //Si nos movemos a una tubería que ya tenía nuestro color, se resetea
                        if (dragedTile.Key.GetColor() == currTile.GetColor())
                        {
                            int p = cMovements[c].ClearUntilTile(dragedTile.Key);
                            percentage -= plusPercentage * p;

                            currTile = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 1];
                            dir = (dragedTile.Key.GetLogicRect().position - currTile.GetLogicRect().position).normalized;

                            if (cMovements[c].GetMovements().Count > 1)
                            {
                                Tile prevTile = cMovements[c].GetMovements()[cMovements[c].GetMovements().Count - 2];
                                previousDir = (currTile.GetLogicRect().position - prevTile.GetLogicRect().position).normalized;
                            }
                        }
                        //Si nos movemos a una tubería que ya tenía otro color, se resetea dicha tubería
                        else if (dragedTile.Key.GetTileColor() != (int)Tile.TILE_COLOR.NONE)
                        {
                            int cM = dragedTile.Key.GetTileColor();

                            //Si la tubería con la que chocamos ya estaba completa, se recortará dejando el lado más largo
                            if (cMovements[cM].GetMovements()[cMovements[cM].GetMovements().Count - 1].CircleActive()) {
                                //Encontramos el lado más largo
                                int i = 0;
                                while (i < cMovements[cM].GetMovements().Count)
                                {
                                    if (cMovements[cM].GetMovements()[i] == dragedTile.Key)
                                        break;
                                    ++i;
                                }

                                //Empezamos a borrar según que lado sea más largo
                                int p = 0;
                                if (i < cMovements[cM].GetMovements().Count / 2)
                                    p = cMovements[cM].ClearFirstUntilTile(dragedTile.Key);
                                else
                                    p = cMovements[cM].ClearUntilTile(dragedTile.Key);
                                percentage -= plusPercentage * p;

                            }
                            //Si la tubería no estaba completa se recortará directamente
                            else
                            {
                                int p = cMovements[cM].ClearUntilTile(dragedTile.Key);
                                percentage -= plusPercentage * p;

                            }

                            //Renderizamos la puntita
                            Tile lastTile = cMovements[cM].GetMovements()[cMovements[cM].GetMovements().Count - 1];
                                
                            if (cMovements[cM].GetMovements().Count > 1)
                            {
                                Tile prevTile = cMovements[cM].GetMovements()[cMovements[cM].GetMovements().Count - 2];
                                Vector2 d = (lastTile.GetLogicRect().position - prevTile.GetLogicRect().position).normalized;

                                lastTile.DesactiveAll();
                                lastTile.ActiveTail(d, lastTile.GetColor());
                            }
                            else
                                lastTile.DesactiveAll();
                        }

                        percentage += plusPercentage;
                        hud.ShowPercentage((int)Math.Round(percentage));

                        dragedTile.Key.ActiveTail(dir, currTileColor);
                        dragedTile.Key.SetX(dragedTile.Value.x);
                        dragedTile.Key.SetY(dragedTile.Value.y);
                        dragedTile.Key.SetTileColor(c);
                        cMovements[c].AddMov(dragedTile.Key);

                        //  El anterior es un circulo
                        if (currTile.CircleActive())
                        {
                            if (cMovements[c].GetMovements().Count > 1)
                                currTile.ActiveTail(new Vector2(dir.x, dir.y), currTileColor);
                        }
                        //  El anterior no es un circulo
                        else if (!currTile.CircleActive())
                        {
                            // Es codo
                            if (IsElbow(dir))
                            {
                                currTile.ActiveElbow(currTileColor, dir, previousDir);
                            }
                            else
                            {
                                currTile.ActiveBridge(dir, currTileColor);
                            }
                        }
                        currTile = dragedTile.Key;
                        previousDir = dir;
                    }
                    ////  Es un circulo de otro color
                    //else if (dragedTile.Key.CircleActive())
                    //{
                    //    print("MovIncorrecto");
                    //}
                }
            }
        }
    }

    /// <summary>
    /// Lógica que procesa el comportamiento al dejar de tocar la pantalla
    /// </summary>
    private void InputUp()
    {
        if (currTile != null)
        {
            int c = currTile.GetTileColor();
            hud.UndoButtonBehaviour(true, () => UndoMovement(c));
            currTile = null;
            ApplyMovements(c);
            inputTile.GetCircleRender().enabled = false;
        }

        if (IsSolution())
        {
            GameManager.instance.AddSolutionLevel(true);
            bool perfect = currMovs == currLevel.numFlow;
            hud.LevelCompleted(perfect);
        }
    }
    #endregion

    #region Getters
    /// <summary>
    /// Determina si la casilla que se está pulsando y la vecina son "juagbles"
    /// </summary>
    private bool AreNeighbour(int x1, int y1, int x2, int y2)
    {
        if ((x1 == x2 && (y1 == y2 - 1 || y1 == y2 + 1)) ||
            (y1 == y2 && (x1 == x2 - 1 || x1 == x2 + 1)))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Determina si el tablero está solucionado o no
    /// </summary>
    private bool IsSolution()
    {
        if (Math.Round(percentage) < 100)
            return false;


        //Recorremos todas las listas de movimientos
        foreach (ColorMovements cM in cMovements)
        {
            //Si el primer elem y el ultimo de una lista no es circulo: no es solución
            if (!cM.IsConected())
                return false;
        }

        //Si hemos salido es porque todos los colores tienen la solución
        return true;
    }

    /// <summary>
    /// Determina si hay que dibujar un codo en el tile y su dirección
    /// </summary>
    private bool IsElbow(Vector2 dir)
    {
        if (Math.Abs(dir.x + previousDir.y) == 2.0f || Math.Abs(dir.y + previousDir.x) == 2.0f
            || Math.Abs(dir.x - previousDir.y) == 0.0f || Math.Abs(dir.y - previousDir.x) == 0.0f)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Convierte el índice dentro del mapa leído de cada uno de los tiles
    /// en coordenadas dentro de la matriz que contiene la información del tablero
    /// </summary>
    private Vector2Int ConvertIndex(int index)
    {
        int x = (int)((index + 1) % currLevel.numBoardX) - 1;
        int y = (int)((index + 1) / currLevel.numBoardX);
        if (x < 0)
        {
            x = currLevel.numBoardX - 1;
            y -= 1;
        }
        return new Vector2Int(x, y);
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
            if (tileRect.Overlaps(touchRect))
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
            if (tileRect.Overlaps(touchRect))
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
    #endregion

    #region Otros
    public void GiveHint()
    {
        var gm = GameManager.instance;
        if (gm.GetNumHints() > 0)
        {
            //  Buscamos qué elementos no están conectados como posibles candidatos a pista
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
            //  elegimos aleatoriamente uno
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
                //  
                for (int i = 0; i < currSolution.Count; i++)
                {
                    Vector2Int index = ConvertIndex(currSolution[i]);
                    Tile tile = tiles[index.y,index.x];
                    if (tile.CircleActive() && i == 0)
                    {
                        InputDown(tile.GetLogicRect().position);
                        tile.ActiveStar(true);
                    }
                    else if (i == currSolution.Count - 1) 
                    {
                        InputMoving(tile.GetLogicRect().position);
                        tile.ActiveStar(true);
                    }
                    else
                    {
                        InputMoving(tile.GetLogicRect().position);
                    }
                }
                gm.UseHint();
                hud.UseHint();
                currMovs++;
                hud.ShowMovements(currMovs);
            }
        }
    }

    /// <summary>
    /// Dibuja el círculo con el color del flujo que se está tocando
    /// para indicar dónde se está pulsando y por dónde se arrastra el input dentro
    /// del tablero
    /// </summary>
    private void DrawCirclePointer(Vector2 inputPos)
    {
        if (currTile != null)
        {
            inputTile.GetCircleRender().enabled = true;
            Vector2 pos = Camera.main.ScreenToWorldPoint(inputPos);
            inputTile.transform.position = new Vector3(pos.x, pos.y, 10.0f);
        }
        else
        {
            inputTile.GetCircleRender().enabled = false;
        }
    }

    /// <summary>
    /// Aplica los movimientos realizados cuando se levanta el input de la pantalla
    /// </summary>
    private void ApplyMovements(int c)
    {
        foreach (Tile tile in cMovements[c].GetMovements())
        {
            tile.ActiveBgColor(true, currTileColor);
        }

        if (cMovements[c].IsConected()) 
        {
            hud.AddFlow(1);
        }
    }

    /// <summary>
    /// Deshace el último movimiento
    /// </summary>
    public void UndoMovement(int c) 
    {
        // Si está conectada, entonces hay que descontar el número de flujos conectados
        if (cMovements[c].IsConected()) 
        {
            hud.AddFlow(-1);
        }

        int p = cMovements[c].UndoMovements();
        percentage -= plusPercentage * p;
        hud.ShowPercentage((int)Math.Round(percentage));
        hud.UndoButtonBehaviour(false);
    }

    #endregion
}