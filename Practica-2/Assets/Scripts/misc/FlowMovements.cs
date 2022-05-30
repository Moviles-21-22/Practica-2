using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

//  Check Amaro

/// <summary>
/// Clase auxiliar para controlar el color del tile en función de los movimientos
/// </summary>
[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("ReSharper", "ArrangeTypeModifiers")]
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
    /// Para colocar la estrella en caso de que se rompa un camino hecho con pistas
    /// </summary>
    /// 
    public bool isHint = false;

    /// <summary>
    /// Color lógico de la tubería
    /// </summary>
    private readonly int flowColor;

    /// <summary>
    /// Color gráfico de la tubería
    /// </summary>
    private readonly Color color;

    /// <summary>
    /// Lista de tiles ordenada con el camino del flujo tras soltar el dedo
    /// </summary>
    private List<Tile> flowPath;

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
    /// Construye un flujo con un color lógico y un color gráfico
    /// </summary>
    /// <param name="c">Color lógico del camino</param>
    /// <param name="col">Color gráfico del camino</param>
    public FlowMovements(int c, Color col)
    {
        flowColor = c;
        color = col;
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
        currentPath.Reverse();

        return ClearUntilTile(t);
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
        // Se limpian los tiles del flowPath y del currentPath
        for (int i = flowPath.Count - 1; i >= 0; i--)
        {
            p++;
            flowPath[i].ClearTile();
            flowPath.Remove(flowPath[i]);
        }
        currentPath.Clear();
        flowPath.Clear();

        percentage -= p * plusPercentage;
        p = 0;

        for (int i = 0; i < lastPath.Count; i++)
        {
            flowPath.Add(lastPath[i]);
            flowPath[i].ActiveBgColor(true, color);
            p++;
        }

        percentage += p * plusPercentage;
        DrawPath(PathType.MAIN_PATH);
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
    /// Activa una estrella en el tile
    /// </summary>
    /// <param name="status">true si está activa la estrella</param>
    public void ActiveStars(bool status)
    {
        currentPath[0].ActiveStar(status);
        currentPath[currentPath.Count - 1].ActiveStar(status);
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