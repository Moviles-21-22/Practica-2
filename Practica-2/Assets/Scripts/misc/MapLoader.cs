using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

//  struct para cada nivel cargado
[Serializable]
[SuppressMessage("ReSharper", "CheckNamespace")]
public struct Level
{
    //  tamaño del tablero
    public int numBoardX;
    public int numBoardY;
    //  nivel que le corresponede
    public int lvl;
    //  numero de flujos
    public int numFlow;
    //  Determina si hay que poner o no muros rodeando el tablero
    public bool closed;
    //  Vector con las soluciones
    public List<List<int>> solutions;
    //public Vector<Vector<int>> solutions;
    //  Vector con casillas con muros -- cada muro es una lista de dos elementos (casillas adyacentes al muro)
    public List<List<int>> walls;
    //  Vector con huecos
    public List<int> gaps;
    public Level(int tamX, int newLevel, int newNumFlows, int tamY, bool wallsAround = false)
    {
        numBoardX = tamX;
        numBoardY = tamY;
        lvl = newLevel;
        numFlow = newNumFlows;
        closed = wallsAround;
        solutions = new List<List<int>>();
        walls = new List<List<int>>();
        gaps = new List<int>();
    }
}

public class Map
{
    //  Vector con el nivel pedido
    private readonly List<Level> levels = new List<Level>();

    /// <summary>
    /// Cargado de niveles
    /// </summary>
    /// <param name="text"></param>
    public Map([NotNull] string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        //  Todos los niveles por separado
        var lvls = text.Split('\n');

        for (var i = 0; i < lvls.Length; i++)
        {
            if (lvls[i] != "")
            {
                levels.Add(ProcessLevel(lvls[i], i));
            }
        }
    }

    /// <summary>
    /// Procesa un pack 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private Level ProcessLevel([NotNull] string level, int index)
    {
        if (level == null) throw new ArgumentNullException(nameof(level));
        //  Separamos por segmentos
        var seg = level.Split(';');
        var subChain = seg[0].Split(',');
        var numBoard = subChain[0].Split(':');
        var closed = false;
        int numBoardX, numBoardY;
        if (numBoard.Length >= 2)   //No es cuadrado
        {
            numBoardX = int.Parse(numBoard[0]);
            string[] plusB = numBoard[1].Split('+');    //Para los ficheros que tienen "+B"
            if (plusB.Length > 1)                       //A esos tableros los rodearemos de muros
                closed = true;
            numBoardY = int.Parse(plusB[0]);
        }
        else
        {
            numBoardX = int.Parse(subChain[0]);
            numBoardY = int.Parse(subChain[0]);
        }

        int numFlow = int.Parse(subChain[3]);
        Level currLevel = new Level(numBoardX, index, numFlow, numBoardY, closed);

        //Miramos a ver si hay muros o huecos
        //i == 4 → puentes (no hay que implementarlos)
        //i == 5 → huecos
        //i == 6 → muros
        if (subChain.Length > 5 && subChain[5] != "")    //Hay huecos
        {
            string[] numGaps = subChain[5].Split(':');
            for (int i = 0; i < numGaps.Length; i++)
            {
                currLevel.gaps.Add(int.Parse(numGaps[i]));
            }
        }

        if (subChain.Length > 6 && subChain[6] != "")    //Hay muros
        {
            string[] numWalls = subChain[6].Split(':');
            for (int i = 0; i < numWalls.Length; i++)
            {
                List<int> currWall = new List<int>();
                currWall.Add(int.Parse(numWalls[i].Split('|')[0]));
                currWall.Add(int.Parse(numWalls[i].Split('|')[1]));
                currLevel.walls.Add(currWall);
            }
        }

        //Guarda las tuberias solucion del tablero
        for (int i = 0; i < currLevel.numFlow; i++)
        {
            string[] chars = seg[i + 1].Split(',');
            List<int> currSolution = new List<int>();
            for (int j = 0; j < chars.Length; j++)
            {
                currSolution.Add(int.Parse(chars[j]));
            }
            currLevel.solutions.Add(currSolution);
        }
        return currLevel;
    }

    /// <summary>
    /// Devueve un nivel de un pack
    /// </summary>
    /// <param name="lvl">Nivel que se quiere</param>
    /// <returns></returns>
    public Level GetLevel(int lvl)
    {
        return levels[lvl];
    }
}
