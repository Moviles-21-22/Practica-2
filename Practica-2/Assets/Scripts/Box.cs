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

    [Tooltip("Referencia al candado del nivel bloqueado")]
    [SerializeField]
    private RawImage lockImage;

    /// <summary>
    /// color actual del tile
    /// </summary>
    private Color color;
    /// <summary>
    /// Cantidad de alpha que se va a ir aplicando para la animación
    /// </summary>
    private float offsetAlpha = -0.1f;

    /// <summary>
    /// Cambia el numero de un nivel
    /// </summary>
    /// <param name="num">Nivel</param>
    public void SetLevelNum(int num)
    {
        numText.text = num.ToString();
    }

    /// <summary>
    /// Inicializa el tile de forma normal
    /// </summary>
    /// <param name="_color">Color que le corresponde al tile</param>
    public void InitBox(Color _color, bool perfect, bool completed)
    {
        color = _color;

        if (perfect)
        {
            starImage.enabled = true;
        }
        else if (completed)
        {
            completedImage.enabled = true;
        }

        color.a = completed ? 1.0f : 0.2f;
        background.color = color;
        frame.color = color;
    }

    /// <summary>
    /// Asiganar un callback a un botón
    /// </summary>
    /// <param name="level">nivel a cargar</param>
    public void SetCallBack(int level)
    {
        button.onClick.AddListener(() => GameManager.instance.LoadLevel(level));
    }

    /// <summary>
    /// Activar el candado del bloque
    /// </summary>
    public void ActiveLockImage() 
    {
        var color = background.color;
        color.a = 0.0f;
        background.color = color;

        color = frame.color;
        color = Color.white;
        frame.color = color;

        color = Color.gray;
        numText.color = color;
        lockImage.enabled = true;
    }

    /// <summary>
    /// Cambio del nivel actual
    /// </summary>
    public void CurrentLevel() 
    {
        var color = background.color;
        color = Color.grey;
        background.color = color;

        color = frame.color;
        color = Color.white;
        frame.color = color;

        InvokeRepeating(nameof(CurrentLevelAnim), 0.0f, 0.1f);
    }

    /// <summary>
    /// Animación del frame brillando del nivel que toca
    /// </summary>
    private void CurrentLevelAnim() 
    {
        if (frame.color.a <= 0.0f) 
        {
            offsetAlpha = 0.1f;
        }
        
        var color = frame.color;
        color.a += offsetAlpha;
        frame.color = color;

        if (frame.color.a >= 1.0f) 
        {
            offsetAlpha = -0.1f;
            CancelInvoke(nameof(CurrentLevelAnim));
            InvokeRepeating(nameof(CurrentLevelAnim), 1.0f, 0.1f);
        }
    }
}
