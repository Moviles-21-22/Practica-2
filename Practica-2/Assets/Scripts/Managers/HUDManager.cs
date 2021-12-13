using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")]
    [SerializeField]
    private GameObject winInfo;

    [Tooltip("Referencia a la imagen de la estrella cuando se gana")]
    [SerializeField]
    private RawImage winStar;

    [Tooltip("Sprites que se usan para el icono de ganar")]
    [SerializeField]
    private Texture[] winSprites;

    [Tooltip("Referencia al objeto que muestra si el nivel se ha completado")]
    [SerializeField]
    private HUD_Element completedLevelIcon;

    [Tooltip("Referencia al objeto que regresa al nivel anterior")]
    [SerializeField]
    private HUD_Element prevLevel;

    [Tooltip("Referencia al objeto que pasa al nivel posterior")]
    [SerializeField]
    private HUD_Element nextLevel;

    [Tooltip("Referencia al objeto que muestra las pistas")]
    [SerializeField]
    private HUD_Element hints;

    [Tooltip("Referencia al objeto que deshace el último movimiento")]
    [SerializeField]
    private HUD_Element undoMovement;

    [Header("Textos")]
    [Tooltip("Referencia al texto que muestra el nivel actual")]
    [SerializeField]
    private Text numLevelText;
    [Tooltip("Referencia al texto que muestra el tamaño del grid")]
    [SerializeField]
    private Text numFlowsText;
    [Tooltip("Referencia al texto que muestra el número de movimientos")]
    [SerializeField]
    private Text numPasosText;
    [Tooltip("Referencia al texto que muestra el récord actual")]
    [SerializeField]
    private Text recordText;
    [Tooltip("Referencia al texto que muestra el porcentaje completado")]
    [SerializeField]
    private Text percentageText;

    [Header("Botones")]
    [Tooltip("Referencia al botón de volver atrás")]
    [SerializeField]
    private Button backButton;
    [Tooltip("Referencia al botón de reiniciar nivel")]
    [SerializeField]
    private Button restartButton;

    private Map map;
    private Level currLevel;
    private GameManager gm;
    private bool winPerfect;
    private float timer = 0.0f;

    [System.Serializable]
    class HUD_Element
    {
        [Tooltip("Diferentes sprites del elemento de HUD")]
        public Sprite[] elementSprites;

        [Tooltip("Referencia al componente Image del elemento del HUD")]
        public Image elementImage;

        [Tooltip("Referencia al componente Text del elemento del HUD")]
        public Text elementText;

        [Tooltip("Referencia al componente Button del elemento del HUD")]
        public Button elementButton;
    }

    private void Start()
    {
        // TODO : Organizar esto
        gm = GameManager.instance;
        map = gm.GetMap();
        currLevel = gm.GetCurrLevel();
        numLevelText.text = "Nivel " + currLevel.lvl;
        LevelPack levelPack = gm.GetCurrentPack();
        completedLevelIcon.elementText.text = currLevel.numBoardX + "x" + currLevel.numBoardY;
        // TODO: en funcion si se ha completado el nivel o no
        completedLevelIcon.elementImage.gameObject.SetActive(true);
        completedLevelIcon.elementImage.sprite = completedLevelIcon.elementSprites[0];

        backButton.onClick.AddListener(() => gm.LoadScene(2));
        restartButton.onClick.AddListener(() => gm.LoadScene(3));

        int numHints = gm.GetNumHints();
        hints.elementText.text = numHints + "x";
        if (gm.GetNumHints() == 0)
        {
            hints.elementImage.sprite = hints.elementSprites[0];
        }
        else 
        {
            hints.elementImage.sprite = hints.elementSprites[1];
            //hints.elementButton.onClick.AddListener(() => UseHint());
        }

        if (currLevel.lvl == 1)
        {
            prevLevel.elementImage.sprite = prevLevel.elementSprites[0];
        }
        else
        {
            prevLevel.elementImage.sprite = prevLevel.elementSprites[1];
            prevLevel.elementButton.onClick.AddListener(() => gm.ChangeLevel(currLevel.lvl - 2));
        }

        if (currLevel.lvl == levelPack.levelsInfo.Count)
        {
            nextLevel.elementImage.sprite = nextLevel.elementSprites[0];
        }
        else 
        {
            nextLevel.elementImage.sprite = nextLevel.elementSprites[1];
            nextLevel.elementButton.onClick.AddListener(() => gm.ChangeLevel(currLevel.lvl));
        }
    }

    public void LevelCompleted(bool perfect) 
    {
        winPerfect = perfect;
        winStar.enabled = true;
        // Si es una solución con movimientos perfectos sale una estrella grande
        if (perfect)
        {
            winStar.texture = winSprites[0];
        }
        else
        {
            // Si es una solución normal aparece un tic
            winStar.texture = winSprites[1];
        }

        InvokeRepeating(nameof(WinAnimation), 0.0f, 0.05f);
    }

    /// <summary>
    /// Animación de aparición de la estrella al ganar
    /// </summary>
    private void WinAnimation() 
    {
        timer += 0.05f;
        if (timer < 0.5f)
        {
            var color = winStar.color;
            color.a += 0.1f;
            winStar.color = color;
        }
        else if (timer < 1.0f) 
        {
            var color = winStar.color;
            color.a -= 0.1f;
            winStar.color = color;
        }
        else if(timer >= 1.0f)
        {
            winStar.enabled = false;
            winInfo.SetActive(true);
            CancelInvoke();
        }
    }

    /// <summary>
    /// Muestra la parte entera del porcentaje que hay resuelto del tablero
    /// </summary>
    public void ShowPercentage(float percentage)
    {
        percentageText.text = "tubería: " + ((int)percentage).ToString() + "%";
    }
}