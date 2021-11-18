using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //  Tiles del tablero
    private GameObject[,] tiles;
    [SerializeField]
    private GameObject tilePrefab;

    public void init(Map map)
    {
        // creamos el tablero con el mapa
    }
}
