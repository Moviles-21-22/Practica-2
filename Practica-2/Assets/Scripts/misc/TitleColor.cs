using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "CheckNamespace")]
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
    private List<Color> currThemeColors;

    public void Init(List<Color> themeColors)
    {
        currThemeColors = themeColors;
        for (var i = 0; i < letters.Length; i++)
        {
            letters[i].color = currThemeColors[i];
        }
    }

    public void ChangeTheme(List<Color> newTheme)
    {
        currThemeColors = newTheme;
    }
    
    /// <summary>
    /// Cambio de colores en función del delta time 
    /// </summary>
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= timeToMove)
        {
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i].color = currThemeColors[index];
                index++;
                if (index >= currThemeColors.Count)
                {
                    index = 0;
                }
                currTime = 0.0f;
            }
            index -= letters.Length - 1;
            if (index < 0)
            {
                index = currThemeColors.Count - 1;
            }
        }
    }
}
