using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Check Amaro

/* Clase que se encarga de gestionar toda la lógica del tablero*/
public class BoardManager : MonoBehaviour
{
    [Tooltip("Referencia al objeto padre donde se van a instanciar los tiles")] [SerializeField]
    private Transform pool;

    [Tooltip("Referencia al prefab de los tiles que se van a crear")] [SerializeField]
    private Tile tilePrefab;

    [Tooltip("Referencia al prefab de los tiles que se van a crear")] [SerializeField]
    private Camera mainCamera;
    
    //  Lista de colores
    private List<Color> currTheme;

    // Circulos instanciados en el tablero
    private List<Tile> circleTiles = new List<Tile>();

    // Muros instanciados en el tablero
    private List<Tile> wallTiles = new List<Tile>();
    
    // Lista con los movimientos de cada uno de los flujos
    private List<FlowMovements> cMovements = new List<FlowMovements>();
    
    // Determina si se está pulsadno la pantalla
    private bool inputDown;

    // Determina si se puede editar o no el tablero
    private bool editable = true;

    //Pistas
    private int currHints;
    
    //Guarda el color lógico del último movimiento que modificó el tablero
    private int lastColorMove;

    // Contador del número de flujos conectados
    private int connectedFlows;
    
    // Contador de movimientos
    private int currMovs;
    
    // Porcentaje de tablero completo
    private float percentage;

    // Porcentaje que se suma al total por cada tile ocupado
    private float plusPercentage;

    // Ultimo tile que han tocado, hasta que el dedo se levante
    private Tile currTile;

    // Tile que representa el recorrido del puntero 
    private Tile inputTile;

    // Tiles del tablero
    private Tile[,] tiles;
    
    // Color del tile tocado
    private Color currTileColor;

    // Dimensiones del tablero
    private Vector2Int tabSize;
    
    // Poisición donde se ha pulsado la primera vez
    private Vector3 initPosInput = new Vector2(-1, -1);

    // Struct auxiliar con la información del nivel actual
    private Level currLevel;

    //  Referencia al level manager
    private LevelManager levelManager;

    //-------------------------------------------------INICIALIZACION-----------------------------------------------------//

    /// <summary>
    /// Setea al board y activa el puntero
    /// </summary>
    /// <param name="level">Nivel que va a ser cargado</param>
    /// <param name="theme">Tema que va a ser cargado</param>
    /// <param name="numHints">Numero de pistas</param>
    /// <param name="hudRegion">Posiciones del hud</param>
    /// <param name="lvlMan">Referecia al level manager</param>
    public void Init(Level level, List<Color> theme, int numHints, RectTransform[] hudRegion, LevelManager lvlMan)
    {
        currLevel = level;
        currTheme = theme;
        currHints = numHints;
        levelManager = lvlMan;

        InitTab(hudRegion);
        InitData();
    }

    /// <summary>
    /// Inicializa el nivel actual
    /// </summary>
    private void InitTab(RectTransform[] hudRegion)
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

        InitCircles();
        InitGaps();
        InitWalls();

        //==============ESCALADO-TABLERO===================//
        // Dimensiones de la cámara, sin tener en cuenta el hud
        float camH = mainCamera.orthographicSize * 2.0f;
        float camW = camH * mainCamera.aspect;

        // Hay que tener en cuenta la altura del hud - Está en píxeles
        float hudOffsetY = (hudRegion[0].rect.height + 10) + (hudRegion[1].rect.height + 10);
        // Conversión a la proporción
        hudOffsetY /= mainCamera.pixelHeight;

        camH *= (1 - hudOffsetY - 0.05f); // 0.05f de margen
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

        mainCamera.gameObject.transform.position = new Vector2(offsetX, -offsetY);

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
    /// Inicializa algunos datos generales del componente
    /// </summary>
    private void InitData()
    {
        lastColorMove = (int) Tile.TILE_COLOR.NONE;

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
    private void InitCircles()
    {
        for (int i = 0; i < currLevel.solutions.Count; i++)
        {
            //Cabeza de la tuberia
            float firstElemt = currLevel.solutions[i][0];
            int filaA = (int) ((firstElemt + 1) / currLevel.numBoardX);
            int colA = (int) ((firstElemt + 1) % currLevel.numBoardX) - 1;
            if (colA < 0)
            {
                colA = currLevel.numBoardX - 1;
                filaA -= 1;
            }

            tiles[filaA, colA].InitTile(i, currTheme[i], colA, filaA);
            circleTiles.Add(tiles[filaA, colA]);

            //Inicializamos la lista de movimientos de este color
            FlowMovements c = new FlowMovements(i, currTheme[i]);
            cMovements.Add(c);

            //Final de la tuberia
            float secElement = currLevel.solutions[i][currLevel.solutions[i].Count - 1];
            int filaB = (int) ((secElement + 1) / currLevel.numBoardX);
            int colB = (int) ((secElement + 1) % currLevel.numBoardX) - 1;
            if (colB < 0)
            {
                colB = currLevel.numBoardX - 1;
                filaB -= 1;
            }

            tiles[filaB, colB].InitTile(i, currTheme[i], colB, filaB);
            circleTiles.Add(tiles[filaB, colB]);
        }
    }

    /// <summary>
    /// Inicializa los muros de los tiles que lo requieran
    /// </summary>
    private void InitWalls()
    {
        for (int i = 0; i < currLevel.walls.Count; i++)
        {
            //Cogemos los dos tiles adyacentes al muro
            int firstElemt = currLevel.walls[i][0];
            int secElemt = currLevel.walls[i][1];

            int fila = (int) ((firstElemt + 1) / currLevel.numBoardX);
            int colm = (int) ((firstElemt + 1) % currLevel.numBoardX) - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }

            //Encontramos en que direccion se encuentra el muro respecto a firstElemt
            if (firstElemt > secElemt + 1) //Muro encima
                tiles[fila, colm].ActiveWall(0);
            else if (firstElemt == secElemt + 1) //Muro a la derecha
                tiles[fila, colm].ActiveWall(1);
            else if (firstElemt < secElemt - 1) //Muro debajo
                tiles[fila, colm].ActiveWall(2);
            else if (firstElemt == secElemt - 1) //Muro a la izquierda
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
    private void InitGaps()
    {
        //Cambiamos la var empty de cada tile si es hueco
        foreach (var elem in currLevel.gaps)
        {
            int fila = (elem + 1) / currLevel.numBoardX;
            int colm = (elem + 1) % currLevel.numBoardX - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }

            tiles[fila, colm].InitEmptyTile();
        }

        //Recorremos de nuevo los huecos para ponerles los muros necesarios
        foreach (var elem in currLevel.gaps)
        {
            int fila = (elem + 1) / currLevel.numBoardX;
            int colm = (elem + 1) % currLevel.numBoardX - 1;
            if (colm < 0)
            {
                colm = currLevel.numBoardX - 1;
                fila -= 1;
            }

            if (fila > 0 && !tiles[fila - 1, colm].GetEmpty()) //Muro encima
                tiles[fila, colm].ActiveWall(0);
            if (colm < currLevel.numBoardX - 1 && !tiles[fila, colm + 1].GetEmpty()) //Muro a la derecha
                tiles[fila, colm].ActiveWall(1);
            if (fila < currLevel.numBoardY - 1 && !tiles[fila + 1, colm].GetEmpty()) //Muro debajo
                tiles[fila, colm].ActiveWall(2);
            if (colm > 0 && !tiles[fila, colm - 1].GetEmpty()) //Muro a la izquierda
                tiles[fila, colm].ActiveWall(3);
        }
    }

//--------------------------------------------------------------------------------------------------------------------//

    public void Update()
    {
        if (!editable)
        {
            return;
        }
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

//-------------------------------------------------INPUT-LOGIC-----------------------------------------------------//

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
                if (c != (int) Tile.TILE_COLOR.NONE)
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
            if (touchRect.Overlaps(currTile.GetLogicRect())) return;

            //  Buscamos el tile entre todas las tiles
            var dragedTile = GetTileOnCollision(touchRect); // Tile actual recibido por el input
            int c = currTile.GetTileColor(); // Anterior tile pulsado

            //Comprobamos que mueva alguna tubería y que la tubería no esté ya conectada
            if (c == (int) Tile.TILE_COLOR.NONE)
            {
                return;
            }

            //Si las casillas no son vecinas no hacemos nada, a no ser que sea para cortar la tubería
            if (!AreNeighbour(c, dragedTile.Value.x, dragedTile.Value.y))
            {
                //Comprobamos si se corta la tubería y se protege de reiniciar la tubería al tocar el círculo final de la cadena
                if (dragedTile.Key.GetColor() != currTile.GetColor() || dragedTile.Key.CircleActive() &&
                    cMovements[c].GetCurrentMoves()[0] != dragedTile.Key) return;

                BackFlowPath(dragedTile, c);

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

                levelManager.UpdatePercentage((int) Math.Round(percentage));
                cMovements[c].DrawPath(FlowMovements.PathType.CURR_PATH);
            }
            // Que haya tile y que sea distinto al anterior
            else if (dragedTile.Key != null && dragedTile.Key != currTile)
            {
                // Dirección entre el nuevo tile y el anterior
                Vector2 dir = (dragedTile.Key.GetLogicRect().position - currTile.GetLogicRect().position)
                    .normalized;

                //Comprobamos si hace colisión con un muro, en cuyo caso salimos sin hacer nada
                //Comprobamos si hace colisión con un muro, en cuyo caso salimos sin hacer nada
                if (dragedTile.Key.WallCollision(-dir) || currTile.WallCollision(dir))
                {
                    return;
                }

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
                    // Corte de tubería - siempre que no se haya completado el recorrido actual
                    else if (dragedTile.Key.GetTileColor() != (int) Tile.TILE_COLOR.NONE &&
                             !(currTile.CircleActive() && cMovements[c].GetCurrentMoves().Count > 1))
                    {
                        CutFlow(dragedTile);

                        // Se actualiza el porcentaje
                        percentage += plusPercentage;
                    }
                    //Si la tubería ya está completa y no se corta a si misma no hace nada
                    else if (currTile.CircleActive() && cMovements[c].GetCurrentMoves().Count > 1)
                    {
                        return;
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

                levelManager.UpdatePercentage((int) Math.Round(percentage));
                cMovements[c].DrawPath(FlowMovements.PathType.CURR_PATH);
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
            if (c == (int) Tile.TILE_COLOR.NONE)
            {
                currTile = null;
                return;
            }

            connectedFlows = 0;
            for (int i = 0; i < cMovements.Count; ++i)
                ApplyMovements(i);

            // El undo solo puede cambiar cuando el camino actual, que acabamos de asignar 
            // haya modificado su propio camino
            bool undoActive = !cMovements[c].IsEqualPath(FlowMovements.PathType.LAST_PATH,
                FlowMovements.PathType.MAIN_PATH);
            // Este if sirve para no desactivar el botón
            if (undoActive)
            {
                // Si ha cambiado el estado del tablero, entonces se suma un movimiento
                // Cambiar el camino del misamo color que antes no cuenta como movimiento
                if (c != lastColorMove)
                {
                    currMovs++;
                }

                lastColorMove = c;
                levelManager.ActivateUndoButton(lastColorMove);
            }

            inputTile.GetCircleRender().enabled = false;
            levelManager.UpdateFlowsCounter(connectedFlows);
        }

        levelManager.UpdateMovements(currMovs);

        if (IsSolution())
        {
            levelManager.AddSolutionLevel(currMovs, currLevel.numFlow);
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
                levelManager.UpdateFlowsCounter(-1);
            }

            p = cMovements[tileColor].ClearUntilTile(cMovements[tileColor].GetCurrentMoves()[0]) - 1;
        }
        // En caso de que se pulse un tile del flujo que no sea un círculo y que sea distinto al último que se guardó
        else if (!currTile.CircleActive() && currTile !=
                 cMovements[tileColor].GetCurrentMoves()[cMovements[tileColor].GetCurrentMoves().Count - 1])
        {
            if (cMovements[tileColor].IsConected())
            {
                levelManager.UpdateFlowsCounter(-1);
            }

            p = cMovements[tileColor].ClearUntilTile(currTile) - 1;
        }

        // Se actualiza el porcentaje
        percentage -= p > 0 ? plusPercentage * p : 0;
        levelManager.UpdatePercentage((int) Mathf.Round(percentage));

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

            currTile = dragedTile.Key;

            currTile.SetTileColor(tileColor);
            cMovements[tileColor].AddCurrentMov(dragedTile.Key);

            if (cMovements[tileColor].isHint)
                cMovements[tileColor].ActiveStars(true);
        }
        //Es el circulo con el que comenzamos, se borra toda la tubería
        else
        {
            BackFlowPath(dragedTile, tileColor);
        }
    }

    /// <summary>
    /// Procesa la lógica del tile cuando retrocede sin soltar el input
    /// </summary>
    /// <param name="dragedTile">El nuevo tile que ha detectado el InputMoving</param>
    /// <param name="tileColor">Color del tile</param>
    private void BackFlowPath(KeyValuePair<Tile, Vector2Int> dragedTile, int tileColor)
    {
        int p = cMovements[tileColor].ClearUntilTile(dragedTile.Key) - 1;
        percentage -= plusPercentage * p;
        dragedTile.Key.SetTileColor(tileColor);
        cMovements[tileColor].AddCurrentMov(dragedTile.Key);
        int count = cMovements[tileColor].GetCurrentMoves().Count;
        currTile = cMovements[tileColor].GetCurrentMoves()[count - 1];
        ResetCutMoves(tileColor);
    }

    /// <summary>
    /// Resetea los movimientos cortados por otra tubería
    /// </summary>
    private void ResetCutMoves(int tileColor)
    {
        var currFlow = cMovements[tileColor];
        var currPath = currFlow.GetCurrentMoves();

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
        if (cMovements[cM].IsConected())
        {
            //  Si es una pista, se desactivan las estrellas
            if (cMovements[cM].isHint)
            {
                cMovements[cM].ActiveStars(false);
            }

            //Encontramos el lado más largo
            int i = 0;
            while (i < cMovements[cM].GetCurrentMoves().Count)
            {
                if (cMovements[cM].GetCurrentMoves()[i] == dragedTile.Key)
                    break;
                ++i;
            }

            //Empezamos a borrar según que lado sea más largo
            int p;

            if (i < cMovements[cM].GetCurrentMoves().Count / 2)
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

        // Se resetea el último elemento que quedó en la lista para que aplique la forma correcta del tile
        var currentPath = cMovements[cM].GetCurrentMoves();
        var ulTile = currentPath[currentPath.Count - 1];
        ulTile.ClearTile();
        ulTile.SetTileColor(cMovements[cM].GetFlowColor());

        // se dibuja el camino actual del flujo que se acaba de cortar
        cMovements[cM].DrawPath(FlowMovements.PathType.CURR_PATH);

        dragedTile.Key.ActiveBgColor(true, currTheme[cM]);
    }

//--------------------------------------------------------------------------------------------------------------------//

//-----------------------------------------------------GET-SET--------------------------------------------------------//

    /// <summary>
    /// Determina si la casilla que se está pulsando y la vecina son "jugables"
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


        // Si hay algún flujo desconectado, se descarta
        if (cMovements.Any(cM => !cM.IsConected()))
        {
            return false;
        }

        // Desactiva el uso del tablero
        editable = false;
        return true;
    }

    /// <summary>
    /// Convierte el índice dentro del mapa leído de cada uno de los tiles
    /// en coordenadas dentro de la matriz que contiene la información del tablero
    /// </summary>
    private Vector2Int ConvertIndex(int index)
    {
        int x = (index + 1) % currLevel.numBoardX - 1;
        int y = (index + 1) / currLevel.numBoardX;
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
        while (!collisionDetected && y < tabSize.y)
        {
            tileRect = tiles[y, x].GetLogicRect();
            if (tileRect.Overlaps(touchRect))
            {
                collisionDetected = true;

                if (!tiles[y, x].GetEmpty())
                    tile = tiles[y, x];
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

        return new KeyValuePair<Tile, Vector2Int>(tile,
            !collisionDetected ? new Vector2Int(-1, -1) : new Vector2Int(x, y));
    }

//--------------------------------------------------------------------------------------------------------------------//

//-----------------------------------------------------OTROS----------------------------------------------------------//

    /// <summary>
    /// Aplica una pista: une dos tuberías que no esten ya resueltas
    /// </summary>
    public void ApplyHint()
    {
        if (currHints <= 0 || !editable) return;

        // Buscamos qué elementos no están conectados como posibles candidatos a pista
        var movs = new List<int>();
        int cont = 0;

        foreach (var cM in cMovements)
        {
            if (!cM.IsConected())
            {
                int cl = cM.GetFlowColor();
                if (cl != -1)
                    movs.Add(cont);
            }

            cont++;
        }

        // Elegimos aleatoriamente uno
        if (movs.Count <= 0) return;

        int elc = UnityEngine.Random.Range(0, movs.Count - 1);
        int choice = movs[elc];
        var d = cMovements[choice];
        cMovements[choice] = d;
        cMovements[choice].isHint = true;

        var currSolution = currLevel.solutions[choice];

        for (int i = 0; i < currSolution.Count; i++)
        {
            Vector2Int index = ConvertIndex(currSolution[i]);
            Tile tile = tiles[index.y, index.x];
            if (tile.CircleActive() && i == 0)
            {
                InputDown(tile.GetLogicRect().center);
                tile.ActiveStar(true);
            }
            else if (i == currSolution.Count - 1)
            {
                InputMoving(tile.GetLogicRect().center);
                tile.ActiveStar(true);
            }
            else
            {
                InputMoving(tile.GetLogicRect().center);
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
        if (currTile != null && currTile.GetTileColor() != (int) Tile.TILE_COLOR.NONE)
        {
            Vector2 pos = mainCamera.ScreenToWorldPoint(inputPos);
            inputTile.transform.position = new Vector3(pos.x, pos.y, 10.0f);
            inputTile.GetCircleRender().enabled = true;
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

        foreach (Tile tile in cMovements[c].GetFlowPath())
        {
            tile.ActiveBgColor(true, tile.GetColor());
        }

        if (cMovements[c].IsConected())
        {
            connectedFlows++;
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
            levelManager.UpdateFlowsCounter(-1);
        }

        // Si el color del movimiento que se deshace es igual al color del último
        // color usado entonces no hace falta descontar el número de movimientos,
        // porque no había sumado movimientos entonces
        if (c != lastColorMove)
        {
            currMovs--;
        }

        cMovements[c].UndoMovements(ref percentage, plusPercentage);
        levelManager.UpdatePercentage((int) Math.Round(percentage));
    }

    /// <summary>
    /// Actualiza el numero de pistas locales por las globales
    /// </summary>
    /// <param name="numHints">numero de pistas globales</param>
    public void UpdateHint(int numHints)
    {
        currHints = numHints;
    }

//--------------------------------------------------------------------------------------------------------------------//
}