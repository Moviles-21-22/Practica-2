using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    public Text categoryTitle;

    [SerializeField]
    public Transform pool;
    [SerializeField]
    public RectTransform content;

    [SerializeField]
    public GameObject packPrefab;

    //  Numero de casillas en x
    private const int numX = 30;
    //  Numero de casillas en y
    private const int numY = 30;

    void Start()
    {
        categoryTitle.color = GameManager.instance.GetCurrentCategory().color;
        categoryTitle.text = GameManager.instance.GetCurrentLevel().levelName;

        CreateGrid();
    }

    private void CreateGrid()
    {
        Vector2 pos = new Vector2(1, 1);

        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                    
            }
        }
    }
}
