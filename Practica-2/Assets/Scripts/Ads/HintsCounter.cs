using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsCounter : MonoBehaviour
{
    [Tooltip("Texto variable del titulo")]
    [SerializeField] private Text text;

    /// <summary>
    /// Cambia el texto en funci�n de las pistas
    /// </summary>
    void Start()
    {
        text.text = "�Te quedan " + GameManager.instance.GetNumHints() + " pistas!";
    }

    /// <summary>
    /// A�ade pistas
    /// </summary>
    /// <param name="numHints">Numero de pistas a a�adir</param>
    public void AddHints(int numHints)
    {
        GameManager.instance.AddHints(numHints);
        text.text = "�Te quedan " + GameManager.instance.GetNumHints() + " pistas!";
    }
}
