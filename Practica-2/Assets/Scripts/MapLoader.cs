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
    //  numero de jugadas
    public int numFlow;
    //  Vector con las soluciones
    public List<List<int>> solutions;
    //public Vector<Vector<int>> solutions;
    //  Vector con casillas con muros
    public List<List<int>> walls;
    //  Vector con huecos
    public List<int> gaps;
    public Level(int _numBoardX,int _lvl,int _numFlow,int _numBoardY)
    {
        numBoardX = _numBoardX;
        numBoardY = _numBoardY;
        lvl = _lvl;
        numFlow = _numFlow;
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
        foreach (string chain in lvls)
        {
            if(chain != "")
                levels.Add(ProcessLevel(chain));
        }
    }

    private Level ProcessLevel(string level)
    {
        //  Separamos por segmentos
        string[] seg = level.Split(';');
        string[] subChain = seg[0].Split(',');
        string[] numBoard = subChain[0].Split(':');
        int numBoardX, numBoardY;
        if (numBoard.Length >= 2)   //No es cuadrado
        {
            numBoardX = int.Parse(subChain[0].ToString());
            numBoardY = int.Parse(subChain[1].ToString());
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
            string[] numBoard = subs[0].Split(':');
            string[] subChain = subs[0].Split(',');
            int numBoardX, numBoardY;
            if (numBoard.Length >= 2)   //No es cuadrado
            {
                numBoardX = int.Parse(subChain[0].ToString());
                numBoardY = int.Parse(subChain[1].ToString());
            }
            else
            {
                numBoardX = int.Parse(subChain[0].ToString());
                numBoardY = int.Parse(subChain[0].ToString());

            }
            int lvl = int.Parse(subChain[2].ToString());
            int numFlow = int.Parse(subChain[3].ToString());
            Level currLevel = new Level(numBoardX,lvl,numFlow,numBoardY);
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
