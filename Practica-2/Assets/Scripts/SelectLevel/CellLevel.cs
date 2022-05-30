using TMPro;
using UnityEngine;
using UnityEngine.UI;

//  Check Amaro

/// <summary>
/// Clase para representar las celdas del grid de niveles
/// </summary>
public class CellLevel : MonoBehaviour
{
    [Tooltip("Referencia al componente Button de la celda")] [SerializeField]
    private Button button;

    [Tooltip("Referencia al componente Text que muestra el número de nivel")] [SerializeField]
    private TextMeshProUGUI numText;

    [Tooltip("Referencia al sprite del fondo de la celda")] [SerializeField]
    private RawImage background;

    /// <summary>
    /// Imagen para represetar la casilla
    /// </summary>
    [SerializeField] private RawImage frame;

    [Tooltip("Referencia al sprite del nivel completado")] [SerializeField]
    private RawImage completedImage;

    [Tooltip("Referencia al sprite del nivel completado perfecto")] [SerializeField]
    private RawImage starImage;

    [Tooltip("Referencia al candado del nivel bloqueado")] [SerializeField]
    private RawImage lockImage;

    /// <summary>
    /// Color actual del tile
    /// </summary>
    private Color color;

    /// <summary>
    /// Color del texto de la casilla
    /// </summary>
    private Color initTextColor;

    /// <summary>
    /// Color del marco
    /// </summary>
    private Color initFrameColor;

    /// <summary>
    /// Color del fondo
    /// </summary>
    private Color initBgColor;
    
    /// <summary>
    /// Cantidad de alpha que se va a ir aplicando para la animación
    /// </summary>
    private float offsetAlpha = -0.1f;

    /// <summary>
    /// Actual nivel
    /// </summary>
    private bool currentLevel;

    /// <summary>
    /// Referencia al selectLevelManager
    /// </summary>
    private SelectLevelManager selectLevelManager;


    /// <summary>
    /// Inicializa la celda del grid de niveles
    /// </summary>
    /// <param name="newColor">Color que le corresponde al tile</param>
    /// <param name="levelState"></param>
    /// <param name="manager">Menu de </param>
    public void InitBox(Color newColor, Levels.LevelState levelState, SelectLevelManager manager)
    {
        selectLevelManager = manager;
        color = newColor;

        switch (levelState)
        {
            case Levels.LevelState.PERFECT:
                starImage.enabled = true;
                break;
            case Levels.LevelState.COMPLETED:
                completedImage.enabled = true;
                break;
        }

        color.a = levelState != Levels.LevelState.UNCOMPLETED ? 1.0f : 0.2f;
        background.color = color;
        frame.color = color;
        initTextColor = numText.color;
        initFrameColor = frame.color;
        initBgColor = background.color;
    }

    /// <summary>
    /// Asigna el callback para cargar el nivel que corresponda
    /// a la celda
    /// </summary>
    /// <param name="level">nivel a cargar</param>
    public void SetCallBack(int level)
    {
        button.onClick.AddListener(() => selectLevelManager.LoadLevel(level));
    }

    /// <summary>
    /// Cambia el numero de un nivel
    /// </summary>
    /// <param name="num">Nivel</param>
    public void SetLevelNum(int num)
    {
        numText.text = num.ToString();
    }

    /// <summary>
    /// Activar el candado del bloque
    /// </summary>
    public void ActiveLockImage()
    {
        var colorAux = background.color;
        colorAux.a = 0.0f;
        background.color = colorAux;

        colorAux = Color.white;
        frame.color = colorAux;

        colorAux = Color.gray;
        numText.color = colorAux;
        lockImage.enabled = true;
        numText.enabled = false;
    }

    /// <summary>
    /// Cambio del nivel actual
    /// </summary>
    public void CurrentLevel()
    {
        currentLevel = true;
        var colorAux = Color.grey;
        background.color = colorAux;

        colorAux = Color.white;
        frame.color = colorAux;

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

        var colorAux = frame.color;
        colorAux.a += offsetAlpha;
        frame.color = colorAux;

        if (frame.color.a >= 1.0f)
        {
            offsetAlpha = -0.1f;
            CancelInvoke(nameof(CurrentLevelAnim));
            InvokeRepeating(nameof(CurrentLevelAnim), 1.0f, 0.1f);
        }
    }

    /// <summary>
    /// Reset a una celda
    /// </summary>
    public void Reset()
    {
        background.color = initBgColor;
        frame.color = initFrameColor;
        numText.enabled = true;
        numText.color = initTextColor;
        lockImage.enabled = false;
        completedImage.enabled = false;
        starImage.enabled = false;
        button.onClick.RemoveAllListeners();
        
        if (!currentLevel) return;
        
        CancelInvoke();
        currentLevel = false;
    }
}