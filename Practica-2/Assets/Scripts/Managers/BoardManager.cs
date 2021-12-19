using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para controlar el color del tile en función de los movimientos
/// </summary>
class FlowMovements
{
    /// <summary>
    /// Tipos de caminos que se guardan
    /// </summary>
    public enum PathType
    {
        MAIN_PATH = 0,
        LAST_PATH = 1,
        CURR_PATH = 2
    }

    /// <summary>
    /// Color lógico de la tubería
    /// </summary>
    int flowColor;
    /// <summary>
    /// Lista de tiles ordenada con el camino del flujo tras soltar el dedo
    /// </summary>
    List<Tile> flowPath;
    /// <summary>
    /// Lista de tiles que se están usando durante el InputMoving
    /// </summary>
    List<Tile> currentPath;
    /// <summary>
    /// Lista del último movimiento que se puede deshacer
    /// </summary>
    List<Tile> lastPath;
    /// <summary>
    /// Lista de los tiles que han sido borrados en un corte junto con su
    /// correspondiente color lógico antes de ser cortado
    /// </summary>
    List<KeyValuePair<Tile, int>> cutPath;

    /// <summary>
    /// Construye un flujo con un color lógico
    /// </summary>
    /// <param name="c">Color lógico del camino</param>
    public FlowMovements(int c)
    {
        flowColor = c;
        flowPath = new List<Tile>();
        lastPath = new List<Tile>();
        cutPath = new List<KeyValuePair<Tile, int>>();
        currentPath = new List<Tile>();
    }

    /// <summary>
    /// Dibuja el flujo en función de un camino
    /// </summary>
    /// <param name="path">Tipo de camino que se quiere dibujar</param>
    public void DrawPath(PathType path)
    {
        List<Tile> drawPath = ChoosePath(path);

        // Dibuja el camino - Si solo está el círculo o no hay elementos, no hace falta dibujar nada
        if (drawPath.Count <= 1)
            return;

        Color color = drawPath[0].GetColor();
        Vector2 dir;
        int i = 0;
        foreach (Tile t in drawPath)
        {
            // Este siempre va a ser o el primer elemento o el último
            if (t.CircleActive())
            {
                int index = t == drawPath[0] ? 1 : drawPath.Count - 2;
                dir = new Vector2(t.GetX() - drawPath[index].GetX(), t.GetY() - drawPath[index].GetY());
                t.ActiveTail(dir, color);
            }
            else
            {
                // Dirección con el anterior
                dir = new Vector2(t.GetX() - drawPath[i].GetX(), t.GetY() - drawPath[i].GetY());
                // Si es el último
                if (t == drawPath[drawPath.Count - 1])
                {
                    t.ActiveTail(dir, color);
                }
                else // Se podría dibujar un bridge o un codo
                {
                    // Dirección con el siguiente
                    Vector2 nextDir = new Vector2(drawPath[i + 2].GetX() - t.GetX(), drawPath[i + 2].GetY() - t.GetY());
                    // Si se mantiene la misma direcicción
                    if (dir == nextDir)
                    {
                        t.ActiveTail(dir, color);
                        t.ActiveBridge(color);
                    }
                    else
                    {
                        t.ActiveElbow(color, dir, nextDir);
                    }
                }

                i++;
            }
        }
    }

    #region ChangePath
    /// <summary>
    /// Actualiza las listas de movimientos al levantar el dedo
    /// </summary>
    public void UpdatePaths()
    {
        cutPath.Clear();

        // Si no ha cambiado el estado de los movimientos, entonces, se deja como estaba
        if (IsEqualPath(PathType.CURR_PATH, PathType.MAIN_PATH))
            return;

        lastPath.Clear();
        int max = flowPath.Count;
        for (int i = 0; i < max; i++)
        {
            lastPath.Add(flowPath[i]);
        }

        flowPath.Clear();
        for (int i = 0; i < currentPath.Count; i++)
        {
            flowPath.Add(currentPath[i]);
        }

        DrawPath(PathType.MAIN_PATH);
    }

    /// <summary>
    /// Actualiza la lista de movimientos actuales
    /// a partir de la lista de movimientos flowPath.
    /// Sirve para usarse al aplicar pistas
    /// </summary>
    public void UpdateCurrentMovs()
    {
        currentPath.Clear();
        for (int i = 0; i < flowPath.Count; i++)
        {
            flowPath[i].SetTileColor(flowColor);
            currentPath.Add(flowPath[i]);
        }
    }

    /// <summary>
    /// Añade un nuevo movimiento a currMovemets
    /// </summary>
    public void AddCurrentMov(Tile t)
    {
        if (!currentPath.Contains(t))
        {
            currentPath.Add(t);
        }
    }

    /// <summary>
    /// Borra todos los elementos de la lista de movimientos actuales, incluido Tile t
    /// Devuelve el numero de elementos borrados
    /// </summary>
    public int ClearUntilTile(Tile t)
    {
        int p = 0;
        bool rem = true;

        while (rem && currentPath.Count > 0)
        {
            Tile ultTile = currentPath[currentPath.Count - 1];

            //Hemos encontrado el tile desde el que queremos mantener el camino
            if (ultTile == t)
            {
                rem = false;
            }

            p++;
            var aux = new KeyValuePair<Tile, int>(ultTile, ultTile.GetTileColor());
            if (!cutPath.Contains(aux))
            {
                cutPath.Add(aux);
            }

            ultTile.ClearTile();
            currentPath.Remove(ultTile);
        }

        return p;
    }

    /// <summary>
    /// Borra todos los elementos de la lista desde el primero hasta t incluido
    /// Devuelve el numero de elementos borrados
    /// </summary>
    public int ClearFirstUntilTile(Tile t)
    {
        //int p = 0, i = 0;
        //bool rem = true;
        //while (rem && currentPath.Count > i)
        //{
        //    Tile firstTile = currentPath[i];
        //    currentPath[i] = currentPath[currentPath.Count - 1];
        //    currentPath[currentPath.Count - 1] = firstTile;
        //
        //    //Hemos encontrado el tile desde el que queremos mantener el camino
        //    if (currentPath[i] == t)
        //    {
        //        rem = false;
        //    }
        //
        //    p++;
        //    cutPath.Add(currentPath[currentPath.Count - 1]);
        //    currentPath[currentPath.Count - 1].ClearTile();
        //    currentPath.Remove(currentPath[currentPath.Count - 1]);
        //    ++i;
        //}
        //
        ////Si sobra algun movimiento se borra
        //for (int j = currentPath.Count; j > i; --j)
        //{
        //    p++;
        //    cutPath.Add(currentPath[currentPath.Count - 1]);
        //    currentPath[currentPath.Count - 1].ClearTile();
        //    currentPath.Remove(currentPath[currentPath.Count - 1]);
        //}
        //
        //return p;
        return 0;
    }

    /// <summary>
    /// Deshace el último movimiento realizado y calcular el porcentaje total
    /// correspondiente al movimiento
    /// </summary>
    /// <param name="percentage">porcentaje del juego antes de deshacer</param>
    /// <returns></returns>
    public void UndoMovements(ref float percentage, float plusPercentage)
    {
        // Auxiliar para aplicar el porcentaje
        int p = 0;
        // Se limpian los tiles del flowPath
        for (int i = flowPath.Count - 1; i >= 0; i--)
        {
            p++;
            flowPath[i].ClearTile();
            flowPath.Remove(flowPath[i]);
        }
        flowPath.Clear();

        percentage -= p * plusPercentage;
        p = 0;

        for (int i = 0; i < lastPath.Count; i++)
        {
            flowPath.Add(lastPath[i]);
            p++;
        }

        percentage += p * plusPercentage;
        DrawPath(PathType.MAIN_PATH);
    }

    /// <summary>
    /// Añade mvimientos del corte de tubería?
    /// </summary>
    public void PutCutMovements()
    {
        //Color c = flowPath[0].GetColor();
        //Tile prevTile = flowPath[flowPath.Count - 2];
        //for (int i = 0; i < cutPath.Count; ++i)
        //{
        //    Tile t = cutPath[cutPath.Count - 1 - i];
        //    Tile lastTile;
        //
        //    if (i == 0)
        //        lastTile = flowPath[flowPath.Count - 1];
        //    else
        //        lastTile = cutPath[cutPath.Count - i];
        //
        //    Vector2 d = new Vector2(t.GetX() - lastTile.GetX(), t.GetY() - lastTile.GetY());
        //    Vector2 prevD = new Vector2(lastTile.GetX() - prevTile.GetX(), lastTile.GetY() - prevTile.GetY());
        //
        //    if (Math.Abs(d.x + prevD.y) == 2.0f || Math.Abs(d.y + prevD.x) == 2.0f
        //    || Math.Abs(d.x + prevD.y) == 0.0f || Math.Abs(d.y + prevD.x) == 0.0f)
        //    {
        //        lastTile.ActiveTail(d, c);
        //        lastTile.ActiveElbow(c, -d, prevD);
        //        lastTile.ActiveBgColor(true, c);
        //    }
        //    else if (!lastTile.CircleActive())
        //    {
        //        lastTile.ActiveTail(d, c);
        //        lastTile.ActiveBridge(c);
        //        lastTile.ActiveBgColor(true, c);
        //    }
        //
        //    if (i == cutPath.Count - 1)
        //    {
        //        t.ActiveTail(d, c);
        //        t.ActiveBgColor(true, c);
        //    }
        //    flowPath.Add(t);
        //    prevTile = lastTile;
        //}
        //ClearCutMovements();
    }

    /// <summary>
    /// Resetea el camino de cortes
    /// </summary>
    public void ClearCutMovements()
    {
        cutPath.Clear();
    }
    #endregion

    #region Getters
    /// <summary>
    /// Determina si dos caminos son iguales
    /// </summary>
    /// <param name="pathA">Camino A de la comparación</param>
    /// <param name="pathB">Camino B de la comparación</param>
    public bool IsEqualPath(PathType pathA, PathType pathB)
    {
        List<Tile> a = ChoosePath(pathA);
        List<Tile> b = ChoosePath(pathB);

        if (a.Count != b.Count)
        {
            return false;
        }

        // Tienen la misma dimensión
        for (int i = 0; i < a.Count; i++)
        {
            // En el momento en el que uno sea distinto a otro, lo retornamos
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determina si el flujo está conectado o no
    /// </summary>
    public bool IsConected()
    {
        return flowPath.Count > 1 && flowPath[flowPath.Count - 1].CircleActive();
    }

    /// <summary>
    /// Devuelve el color lógico de la tubería
    /// </summary>
    public int GetFlowColor()
    {
        return flowColor;
    }

    /// <summary>
    /// Devuelve el camino del fujo consolidado tras levantar el Input
    /// </summary>
    /// <returns></returns>
    public List<Tile> GetFlowPath()
    {
        return flowPath;
    }

    /// <summary>
    /// Devuelve la lista de los movimientos cortados
    /// </summary>
    /// <returns></returns>
    public List<KeyValuePair<Tile, int>> GetCutMovements()
    {
        return cutPath;
    }

    /// <summary>
    /// Devuelve la lista de los últimos movimientos realizados
    /// </summary>
    /// <returns></returns>
    public List<Tile> GetLastMovs()
    {
        return lastPath;
    }

    /// <summary>
    /// Devuelve el camino actual del flujo
    /// </summary>
    /// <returns></returns>
    public List<Tile> GetCurrentMoves()
    {
        return currentPath;
    }

    /// <summary>
    /// Devuelve una lista auxiliar de tiles en función del camino que se
    /// quiere obtener
    /// </summary>
    private List<Tile> ChoosePath(PathType path)
    {
        List<Tile> a = new List<Tile>();
        switch (path)
        {
            case PathType.MAIN_PATH:
                a = flowPath;
                break;
            case PathType.LAST_PATH:
                a = lastPath;
                break;
            case PathType.CURR_PATH:
                a = currentPath;
                break;
        }

        return a;
    }
    #endregion
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
    private List<FlowMovements> cMovements = new List<FlowMovements>();
    // Contador de movimientos
    private int currMovs = 0;

    //  Setea al board y activa el puntero
    public void Start()
    {
        currLevel = GameManager.instance.GetCurrLevel();
        colors = GameManager.instance.GetCurrTheme().colors;

        Init();
        InitData();
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
            DrawCirclePointer(Input.mousePosition);
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
#elif UNITY_ANDROID

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
    private void Init()
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

    private void InitData()
    {
        // Calculos para el porcentaje del nivel
        plusPercentage = 100.0f / ((tabSize.x * tabSize.y) - currLevel.numFlow - currLevel.gaps.Count);

        // Tile qu representa el input
        inputTile = Instantiate(tilePrefab, transform);
        inputTile.DesactiveLines();
        inputTile.transform.localScale = new Vector3(2, 2, 1);
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
            FlowMovements c = new FlowMovements(i);
            cMovements.Add(c);

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

                //Solo hacemos algo si hemos tocado un tile que no esté vacío
                int c = currTile.GetTileColor();
                if (c != (int)Tile.TILE_COLOR.NONE)
                {
                    ProcessTileDown(c);
                }
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
                var dragedTile = GetTileOnCollision(touchRect); // Tile actual recibido por el input
                int c = currTile.GetTileColor();    // Anterior tile pulsado
                if (c == (int)Tile.TILE_COLOR.NONE || !AreNeighbour(c, dragedTile.Value.x, dragedTile.Value.y))
                {
                    return;
                }
                // Que haya tile y que sea distinto al anterior
                else if (dragedTile.Key != null && dragedTile.Key != currTile)
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
                        ConnectFlows(dragedTile, c);
                    }
                    // No es un circulo
                    else if (!dragedTile.Key.CircleActive())
                    {
                        //Si nos movemos a una tubería que ya tenía nuestro color, se resetea
                        if (dragedTile.Key.GetColor() == currTile.GetColor())
                        {
                            BackFlowPath(dragedTile, c);
                        }
                        // Corte de tubería
                        else if (dragedTile.Key.GetTileColor() != (int)Tile.TILE_COLOR.NONE)
                        {
                            CutFlow(dragedTile);

                            // Se actualiza el porcentaje
                            percentage += plusPercentage;
                        }
                        else
                        {
                            // Se actualiza el porcentaje
                            percentage += plusPercentage;
                        }

                        
                        // Actualización del dragedTile
                        dragedTile.Key.SetX(dragedTile.Value.x);
                        dragedTile.Key.SetY(dragedTile.Value.y);
                        dragedTile.Key.SetTileColor(c);

                        // Si solo hay un elemento, se añade el nuevo mov
                        // Si hay más de uno, entonces interesa añadir solo cuando el último no sea un círculo
                        // Esto evita arrastrar la tubería más allá de la conexión del flujo
                        int count = cMovements[c].GetCurrentMoves().Count;
                        if (count <= 1 || !cMovements[c].GetCurrentMoves()[count - 1].CircleActive())
                            cMovements[c].AddCurrentMov(dragedTile.Key);

                        currTile = dragedTile.Key;
                    }

                        hud.ShowPercentage((int)Math.Round(percentage));
                    cMovements[c].DrawPath(FlowMovements.PathType.CURR_PATH);
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
            if (c == (int)Tile.TILE_COLOR.NONE)
            {
                currTile = null;
                return;
            }
            for (int i = 0; i < cMovements.Count; ++i) ApplyMovements(i);

            inputTile.GetCircleRender().enabled = false;
            //foreach (FlowMovements movs in cMovements)
            //{
            //    if (movs.GetColor() != c && movs.GetCutMovements().Count > 0)
            //        if (movs.GetCutMovements()[movs.GetCutMovements().Count - 1].GetTileColor() == c)
            //            movs.ClearCutMovements();
            //        else
            //            movs.PutCutMovements();
            //}
        }

        if (IsSolution())
        {
            bool perfect = currMovs == currLevel.numFlow;
            GameManager.instance.AddSolutionLevel(perfect, currMovs);
            hud.LevelCompleted(perfect);
        }

        currTile = null;
    }

    /// <summary>
    /// Procesa la lógica del tile recibido por InputDown
    /// </summary>
    /// <param name="tileColor">Color del tile pulsado</param>
    private void ProcessTileDown(int tileColor)
    {
        currTileColor = currTile.GetColor();
        // En el momento en el que se pulsa, los movimiento actuales serán iguales al flujo
        // cuando se levantó el dedo
        cMovements[tileColor].UpdateCurrentMovs();

        int p = 0;
        //Comprobamos si hay que hacer algún clear para los movimientos
        // En caso de que se pulse un círculo (cualquier extremo del flujo), se borra el camino entero
        if (currTile.CircleActive() && cMovements[tileColor].GetCurrentMoves().Count > 0)
        {
            if (cMovements[tileColor].IsConected())
            {
                hud.AddFlow(-1);
            }

            p = cMovements[tileColor].ClearUntilTile(cMovements[tileColor].GetCurrentMoves()[0]) - 1;
        }
        // En caso de que se pulse un tile del flujo que no sea un círculo y que sea distinto al último que se guardó
        else if (!currTile.CircleActive() && currTile != cMovements[tileColor].GetCurrentMoves()[cMovements[tileColor].GetCurrentMoves().Count - 1])
        {
            if (cMovements[tileColor].IsConected())
            {
                hud.AddFlow(-1);
            }

            p = cMovements[tileColor].ClearUntilTile(currTile) - 1;
        }

        // Se actualiza el porcentaje
        percentage -= p > 0 ? plusPercentage * p : 0;
        hud.ShowPercentage((int)Mathf.Round(percentage));

        // Se actualiza el tile actual pulsado
        currTile.SetTileColor(tileColor);
        // Se añade a la lista
        cMovements[tileColor].AddCurrentMov(currTile);
        cMovements[tileColor].DrawPath(FlowMovements.PathType.CURR_PATH);

        // Para el puntero en pantalla
        inputTile.GetCircleRender().enabled = true;
        inputTile.InitTile(currTile.GetTileColor(), new Color(currTileColor.r, currTileColor.g, currTileColor.b, 0.5f));
    }

    /// <summary>
    /// Añade el otro extremo del flujo a la lista de movimientos del flujo
    /// </summary>
    /// <param name="dragedTile">El nuevo tile que ha detectado el InputMoving</param>
    /// <param name="tileColor">Color del tile</param>
    private void ConnectFlows(KeyValuePair<Tile, Vector2Int> dragedTile, int tileColor)
    {
        //Comprobamos si es el mismo círculo con el que empecé o no
        if (dragedTile.Key != cMovements[tileColor].GetCurrentMoves()[0])
        {
            dragedTile.Key.PlayParticle();

            percentage += plusPercentage;
            hud.ShowPercentage((int)Math.Round(percentage));

            currTile = dragedTile.Key;

            currTile.SetTileColor(tileColor);
            cMovements[tileColor].AddCurrentMov(dragedTile.Key);
        }
        //Es el circulo con el que comenzamos, se borra toda la tubería
        else
        {
            BackFlowPath(dragedTile, tileColor);

            // Se limpia todo el camino hasta el inicial incluido
            //int p = cMovements[tileColor].ClearUntilTile(dragedTile.Key) - 1;
            //percentage -= plusPercentage * p;
            //
            //hud.ShowPercentage((int)Math.Round(percentage));
            //
            //currTile = dragedTile.Key;
        }

    }

    /// <summary>
    /// Procesa la lógica del tile cuando retrocede sin soltar el input
    /// </summary>
    /// <param name="dragedTile">El nuevo tile que ha detectado el InputMoving</param>
    /// <param name="tileColor">Color del tile</param>
    /// <param name="dir">Dirección del input</param>
    private void BackFlowPath(KeyValuePair<Tile, Vector2Int> dragedTile, int tileColor)
    {
        int p = cMovements[tileColor].ClearUntilTile(dragedTile.Key) - 1;
        percentage -= plusPercentage * p;

        dragedTile.Key.SetTileColor(tileColor);
        cMovements[tileColor].AddCurrentMov(dragedTile.Key);

        int count = cMovements[tileColor].GetCurrentMoves().Count;
        currTile = cMovements[tileColor].GetCurrentMoves()[count - 1];

        // Al limpiar los tiles, habría que comprobar a quién pertenecían esos tiles (buscando
        // entre todos los demás flujos para saber su el tile del flujo actual está solapando algún otro.
        // Entonces, cuando se esté volviendo hacía atrás, los tiles cortados del flujo actual, serán añadidos a sus
        // anteriores flujos
        ResetCutMoves(tileColor);
    }

    /// <summary>
    /// Resetea los movimientos cortados por otra tubería
    /// </summary>
    private void ResetCutMoves(int tileColor)
    {
        var currFlow = cMovements[tileColor];
        var currPath = currFlow.GetCurrentMoves();
        //var cutPath = currFlow.GetCutMovements();

        foreach (FlowMovements flow in cMovements)
        {
            // Solo nos interesan los flujos con cutMoves
            if (flow != currFlow && flow.GetCutMovements().Count > 0)
            {
                // Se recorre la lista de tiles cortados del otro flujo
                for (int i = flow.GetCutMovements().Count - 1; i >= 0; i--)
                {
                    var ultTile = flow.GetCutMovements()[i].Key;
                    // Si el camino actual ya no contiene al movimiento cortado
                    if (!currPath.Contains(ultTile))
                    {
                        //ultTile.ClearTile();
                        flow.GetCutMovements().Remove(flow.GetCutMovements()[i]);
                        ultTile.SetTileColor(flow.GetFlowColor());
                        flow.AddCurrentMov(ultTile);

                        percentage += plusPercentage;
                    }
                    //En cuanto haya un tile cortado no se pintarán más de la cadena
                    else break;
                }

                flow.DrawPath(FlowMovements.PathType.CURR_PATH);
            }
        }
    }

    /// <summary>
    /// Procesa la lógica de cortar una tubería cuando se tiene
    /// agarrada otra tubería.
    /// </summary>
    /// <param name="dragedTile">Tile que recibe el input actual</param>
    private void CutFlow(KeyValuePair<Tile, Vector2Int> dragedTile)
    {
        int cM = dragedTile.Key.GetTileColor();

        //Si la tubería con la que chocamos ya estaba completa, se recortará dejando el lado más largo
        if (cMovements[cM].GetFlowPath()[cMovements[cM].GetFlowPath().Count - 1].CircleActive())
        {
            ////Encontramos el lado más largo
            //int i = 0;
            //while (i < cMovements[cM].GetFlowPath().Count)
            //{
            //    if (cMovements[cM].GetFlowPath()[i] == dragedTile.Key)
            //        break;
            //    ++i;
            //}
            //
            ////Empezamos a borrar según que lado sea más largo
            //int p = 0;
            //if (i < cMovements[cM].GetFlowPath().Count / 2)
            //    p = cMovements[cM].ClearFirstUntilTile(dragedTile.Key);
            //else
            //    p = cMovements[cM].ClearUntilTile(dragedTile.Key);
            //percentage -= plusPercentage * p;

        }
        //Si la tubería no estaba completa se recortará directamente
        else
        {
            int p = cMovements[cM].ClearUntilTile(dragedTile.Key);
            percentage -= plusPercentage * p;

            // Se resetea el último elemento que quedó en la lista para que aplique la forma correcta del tile
            var currentPath = cMovements[cM].GetCurrentMoves();
            var ulTile = currentPath[currentPath.Count - 1];
            ulTile.ClearTile();
            ulTile.SetTileColor(cMovements[cM].GetFlowColor());
        }

        // se dibuja el camino actual del flujo que se acaba de cortar
        cMovements[cM].DrawPath(FlowMovements.PathType.CURR_PATH);

        //if (cMovements[cM].GetFlowPath().Count > 1)
        //{
        //    Tile prevTile = cMovements[cM].GetFlowPath()[cMovements[cM].GetFlowPath().Count - 2];
        //    Vector2 d = (lastTile.GetLogicRect().position - prevTile.GetLogicRect().position).normalized;
        //
        //    lastTile.DesactiveAll();
        //    lastTile.ActiveTail(d, lastTile.GetColor());
        //}
        //else
        //    lastTile.DesactiveAll();
    }
    #endregion

    #region Getters
    /// <summary>
    /// Determina si la casilla que se está pulsando y la vecina son "juagbles"
    /// </summary>
    private bool AreNeighbour(int tileColor, int x1, int y1)
    {
        // Para poner solo las tuberías en casillas vecinas, para no saltar círculos, huecos, entre otros
        if (cMovements[tileColor].GetCurrentMoves().Count == 0)
            return false;

        int index = cMovements[tileColor].GetCurrentMoves().Count - 1;
        int countX = cMovements[tileColor].GetCurrentMoves()[index].GetX();
        int countY = cMovements[tileColor].GetCurrentMoves()[index].GetY();

        if ((x1 == countX && (y1 == countY - 1 || y1 == countY + 1)) ||
            (y1 == countY && (x1 == countX - 1 || x1 == countX + 1)))
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
        foreach (FlowMovements cM in cMovements)
        {
            //Si el primer elem y el ultimo de una lista no es circulo: no es solución
            if (!cM.IsConected())
                return false;
        }

        //Si hemos salido es porque todos los colores tienen la solución
        return true;
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
            foreach (FlowMovements cM in cMovements)
            {
                if (!cM.IsConected())
                {
                    int cl = cM.GetFlowColor();
                    if (cl != -1)
                        movs.Add(cont);
                }
                cont++;
            }
            //  elegimos aleatoriamente uno
            if (movs.Count > 0)
            {
                int elc = UnityEngine.Random.Range(0, movs.Count - 1);
                int choice = movs[elc];
                var d = cMovements[choice];
                cMovements[choice] = d;

                List<int> currSolution = GameManager.instance.GetCurrLevel().solutions[choice];

                for (int i = 0; i < currSolution.Count; i++)
                {
                    Vector2Int index = ConvertIndex(currSolution[i]);
                    Tile tile = tiles[index.y, index.x];
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
        if (currTile != null && currTile.GetTileColor() != (int)Tile.TILE_COLOR.NONE)
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
        foreach (var l in cMovements)
        {
            l.UpdatePaths();
        }

        bool undoActive = !cMovements[c].IsEqualPath(FlowMovements.PathType.LAST_PATH,
            FlowMovements.PathType.MAIN_PATH);
        hud.UndoButtonBehaviour(undoActive, () => UndoMovement(c));

        foreach (Tile tile in cMovements[c].GetFlowPath())
        {
            tile.ActiveBgColor(true, tile.GetColor());
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

        cMovements[c].UndoMovements(ref percentage, plusPercentage);
        hud.ShowPercentage((int)Math.Round(percentage));
        hud.UndoButtonBehaviour(false);
    }

    #endregion
}