using System.IO;
using System.Collections.Generic;


//  struct para cada nivel cargado
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
    public Level(int _numBoardX, int _lvl, int _numFlow, int _numBoardY, bool _closed = false)
    {
        numBoardX = _numBoardX;
        numBoardY = _numBoardY;
        lvl = _lvl;
        numFlow = _numFlow;
        closed = _closed;
        solutions = new List<List<int>>();
        walls = new List<List<int>>();
        gaps = new List<int>();
    }
}


public class Map
{
    //  Vector con el nivel pedido
    private List<Level> levels = new List<Level>();

    public Map(string text, int a)
    {
        //  Todos los niveles por separado
        string[] lvls = text.Split('\n');

        for (int i = 0; i < lvls.Length; i++)
        {
            if (lvls[i] != "")
            {
                levels.Add(ProcessLevel(lvls[i], i));
            }
        }
    }

    private Level ProcessLevel(string level, int index)
    {
        //  Separamos por segmentos
        string[] seg = level.Split(';');
        string[] subChain = seg[0].Split(',');
        string[] numBoard = subChain[0].Split(':');
        bool closed = false;
        int numBoardX, numBoardY;
        if (numBoard.Length >= 2)   //No es cuadrado
        {
            numBoardX = int.Parse(numBoard[0].ToString());
            string[] plusB = numBoard[1].Split('+');    //Para los ficheros que tienen "+B"
            if (plusB.Length > 1)                       //A esos tableros los rodearemos de muros
                closed = true;
            numBoardY = int.Parse(plusB[0].ToString());
        }
        else
        {
            numBoardX = int.Parse(subChain[0].ToString());
            numBoardY = int.Parse(subChain[0].ToString());
        }

        int lvl = int.Parse(subChain[2].ToString());
        int numFlow = int.Parse(subChain[3].ToString());
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

    //  Abre y lee un txt y crea un array bidimensional con la información del tablero
    public Map(string route)
    {
        StreamReader reader = new StreamReader(route);
        // En orden
        //  1- tamaño del tablero
        //  2- reservado
        //  3- nivel
        //  4- numero de flujos
        //  5- soluciones ...
        string chain;
        while (!reader.EndOfStream)
        {
            //  Una línea es un nivel
            chain = reader.ReadLine();
            string[] subs = chain.Split(';');
            string[] subChain = subs[0].Split(',');
            string[] numBoard = subChain[0].Split(':');
            int numBoardX, numBoardY;
            if (numBoard.Length >= 2)   //No es cuadrado
            {
                numBoardX = int.Parse(numBoard[0].ToString());
                numBoardY = int.Parse(numBoard[1].ToString());
            }
            else
            {
                numBoardX = int.Parse(subChain[0].ToString());
                numBoardY = int.Parse(subChain[0].ToString());

            }
            int lvl = int.Parse(subChain[2].ToString());
            int numFlow = int.Parse(subChain[3].ToString());
            Level currLevel = new Level(numBoardX, lvl, numFlow, numBoardY);
            for (int i = 0; i < currLevel.numFlow; i++)
            {
                string[] chars = subs[i + 1].Split(',');
                List<int> currSolution = new List<int>();
                for (int j = 0; j < chars.Length; j++)
                {
                    currSolution.Add(int.Parse(chars[j]));
                }
                currLevel.solutions.Add(currSolution);
            }
            levels.Add(currLevel);
        }
    }

    public List<Level> GetAllLevels()
    {
        return levels;
    }

    public Level GetLevel(int lvl)
    {
        return levels[lvl];
    }
}
