using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsCounter : MonoBehaviour
{
    [Tooltip("Texto variable del titulo")]
    [SerializeField] private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "¡Te quedan " + GameManager.instance.GetNumHints() + " pistas!";
    }

    public void AddHints(int numHints)
    {
        GameManager.instance.AddHints(numHints);
        text.text = "¡Te quedan " + GameManager.instance.GetNumHints() + " pistas!";
    }
}
