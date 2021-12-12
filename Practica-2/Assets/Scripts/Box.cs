using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Box : MonoBehaviour
{
    [Tooltip("Referencia al componente Button del tile")]
    [SerializeField]
    private Button button;
    [Tooltip("Referencia al componente Text que muestra el número de nivel")]
    [SerializeField]
    private Text numText;
    [Tooltip("Referencia al sprite del fondo del tile")]
    [SerializeField]
    private RawImage background;

    [SerializeField]
    private RawImage frame;

    [Tooltip("Referencia al sprite del nivel completado")]
    [SerializeField]
    private RawImage completedImage;

    [Tooltip("Referencia al sprite del nivel completado perfecto")]
    [SerializeField]
    private RawImage starImage;

    /// <summary>
    /// color actual del tile
    /// </summary>
    private Color color;

    public void SetLevelNum(int num)
    {
        numText.text = num.ToString();
    }

    public void SelectBox()
    {
        color = Color.white;
        background.enabled = true;
        frame.enabled = true;
        background.color = color;
        frame.color = color;
    }

    public void initBox(Color _color)
    {
        color = _color;
        background.color = color;
        frame.color = color;
    }

    public void SetCallBack(int level)
    {
        button.onClick.AddListener(() => GameManager.instance.LoadLevel(level));
    }

    public void CompletedBox(bool perfect, bool completed) 
    {
        if (perfect)
        {
            starImage.enabled = true;
        }
        else if(completed)
        {
            completedImage.enabled = true;
        }

        var color = background.color;
        color.a = completed ? 1.0f : 0.5f;
        background.color = color;
    }
}
