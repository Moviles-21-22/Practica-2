using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [Tooltip("Escena a la que se quiere ir")]
    [SerializeField]
    private int sceneToLoad;

    public void Start()
    {
        button.onClick.AddListener(()=> GameManager.instance.LoadScene(sceneToLoad));
    }
}
