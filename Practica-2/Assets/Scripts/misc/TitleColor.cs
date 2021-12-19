using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleColor : MonoBehaviour
{
    [Tooltip("Letras del titulo")]
    [SerializeField] public Text[] letters;
    // Start is called before the first frame update
    [Tooltip("Tiempo para mover las letras")]
    public float timeToMove = 0.5f;
    //  Tiempo actual
    private float currTime = 0.0f;
    //  Index de los colores
    private int index = 0;
    //  Coles cargados
    private List<Color> colors;

    void Start()
    {
        colors = GameManager.instance.GetCurrTheme().colors;
        for (int i = 0; i < letters.Length; i++)
        {
            letters[i].color = colors[i];
        }
    }

    /// <summary>
    /// Cambio de colores en función del delta time 
    /// </summary>
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= timeToMove)
        {
            colors = GameManager.instance.GetCurrTheme().colors;
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i].color = colors[index];
                index++;
                if (index >= colors.Count)
                {
                    index = 0;
                }
                currTime = 0.0f;
            }
            index -= letters.Length - 1;
            if (index < 0)
            {
                index = colors.Count - 1;
            }
        }
    }
}
