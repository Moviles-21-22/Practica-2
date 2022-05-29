using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Tooltip("Referencia a AdsManager")] [SerializeField]
    private AdsManager ads;

    [Tooltip("GameObject que muestras las pistas que quedan")] [SerializeField]
    private TextMeshProUGUI hintsText;

    [Tooltip("Sprite del candado")] [SerializeField]
    private Sprite selectedSprite;
    
    [Header("Efectos")]
    [Tooltip("Texto a escribir como titulo")]
    [SerializeField]
    private string textTittle = "TIENDA";

    [Tooltip("Tiempo de refresco del rótulo niveles")]
    [SerializeField]
    [Min(0.2f)]
    private float tiendaTitleFr;

    [Header("Átributos para generar temas de forma dinámica")]
    [Tooltip("Referencia al contenido del scroll")]
    [SerializeField]
    private RectTransform contentScroll;

    [Tooltip("Referencia RectTransform de la parte superior del UI")] [SerializeField]
    private RectTransform topSection;

    [Tooltip("Referencia RectTransform de la parte del premium")] [SerializeField]
    private RectTransform premiumSection;

    [Tooltip("Referencia RectTransform de la parte de las pistas")] [SerializeField]
    private RectTransform hintsSection;

    [Tooltip("Referencia RectTransform de la parte de las pistas")] [SerializeField]
    private RectTransform themesSection;

    [Tooltip("Referencia RectTransform del marco de los temas")] [SerializeField]
    private RectTransform themesBound;
    
    [Tooltip("GameObject padre de los temas que se van a instanciar")] [SerializeField]
    private RectTransform themesLayout;

    [Tooltip("RectTransform usado como base para el tamaño de los temas")] [SerializeField]
    private RectTransform baseRect;

    [Tooltip("Prefab para crear la informaicón de los temas")] [SerializeField]
    private ThemeFeatures themeFeaturesGo;

    [Header("__")] [Tooltip("Lista de las imágenes del menú que se ven cambiadas por el tema")] [SerializeField]
    private List<Image> menu;

    [Tooltip("Lista de los paquetes de temas del juego")] [SerializeField]
    private List<ColorPack> colorThemes;

    [Tooltip("Referencia al textMeshPro del rótulo")]
    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    /// <summary>
    /// Index para saber en qúé letra estamos
    /// </summary>
    private int tiendaIndex;

    private bool isPremium;
    private int currHints;
    private List<GameManager.ThemeData> themesList;
    private GameManager.ThemeData currTheme;
    private ThemeFeatures currThemeGo;
    private GameManager gm;

    /// <summary>
    /// Inicializa la tienda
    /// </summary>
    /// <param name="themes">Lista de los temas guardados en el GM</param>
    /// <param name="lastTheme">Último tema aplicado</param>
    /// <param name="numHints">Número de pistas disponibles</param>
    /// <param name="defaultInit">Determina si se inicializa desde la propia escena ShopScene</param>
    public void Init(List<GameManager.ThemeData> themes, GameManager.ThemeData lastTheme,
        int numHints, bool defaultInit = false)
    {
        gm = GameManager.instance;
        isPremium = gm.IsPlayerPremium();
        currHints = numHints;
        if (defaultInit)
        {
            GenerateDefaultData();
        }
        else
        {
            themesList = themes;
            currTheme = lastTheme;
        }

        ads.Init();

        InitElements();

        InvokeRepeating(nameof(TitleColor), 0, tiendaTitleFr);
    }
//-------------------------------------------------INICIALIZACION-----------------------------------------------------//

    /// <summary>
    /// Genera datos por defecto para que la escena
    /// se pueda lanzar de forma independiente
    /// </summary>
    private void GenerateDefaultData()
    {
        int numThemes = colorThemes.Count;
        themesList = new List<GameManager.ThemeData>();
        for (int i = 0; i < numThemes; i++)
        {
            themesList.Add(new GameManager.ThemeData());
            themesList[i].name = colorThemes[i].name;
            themesList[i].colors = colorThemes[i].colors;
            themesList[i].unlocked = colorThemes[i].unlocked;
        }

        themesList[0].isCurrTheme = true;
        currTheme = themesList[0];
    }

    /// <summary>
    /// Inicializa todos los elementos de la UI de la tienda
    /// en funión de los datos generados u obtenidos
    /// </summary>
    private void InitElements()
    {
        hintsText.text = "¡Te quedan " + currHints + " pistas!";

        InitThemes();
        UpdateShopColor();
    }

    /// <summary>
    /// Inicializa la lista de temas de la tienda deforma dinámica
    /// </summary>
    private void InitThemes()
    {
        ResizeUI();
        var numThemes = themesList.Count;

        for (var i = 0; i < numThemes; i++)
        {
            // Instatiate del GO de los temas
            var themeGo = Instantiate(themeFeaturesGo, themesLayout);

            var aux = i;
            themeGo.themeButton.onClick.AddListener(() => ChangeTheme(aux, themeGo));
            
             // Si es el tema actual, entonces se activa la imagen del tick
             if (themesList[i] == currTheme)
             {
                 themeGo.selectedImage.sprite = selectedSprite;
                 currThemeGo = themeGo;
             }
             // Si simplemente está desbloqueado, se desactiva el sprite
             else if (themesList[i].unlocked)
             {
                 themeGo.selectedImage.enabled = false;
             }

            themeGo.themeName.text = themesList[i].name;

            for (var j = 0; j < themeGo.samples.Count; j++)
            {
                themeGo.samples[j].color = themesList[i].colors[j];
            }
        }
    }

//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// A partir del tema base del editor, se calcula
    /// la altura que tendrán los botones para los temas
    /// del juego
    /// </summary>
    /// <returns></returns>
    private float CalculateHeightThemes()
    {
        int numElems = themesList.Count;
        if (numElems == 0)
        {
            Debug.LogError("No se han encontrado temas registrados");
        }

        float height = numElems * baseRect.rect.height;
        baseRect.gameObject.SetActive(false);
        return height;
    }
    
    /// <summary>
    /// Redimensiona los elementos de la UI de forma dinámica
    /// </summary>
    private void ResizeUI()
    {
        // 1. Calculo del tamaño que van a ocupar sólo los temas
        var themesHeight = CalculateHeightThemes();
        
        //2. Cálculo del offset que se la aplicará al contentScroll
        var scrollH = contentScroll.rect.height;
        var offset = themesHeight / scrollH;
        var contentAnchor = Vector2.down * offset;

        // 3. Si es premium, se resta el tamaño de premiumSection al
        // contentScroll y se reposicionan los objetos.
        if (isPremium)
        {
            var premiumPos = premiumSection.localPosition;
            var hintsPos = hintsSection.localPosition;
            hintsSection.localPosition = premiumPos;
            themesSection.localPosition = hintsPos;
            
            premiumSection.gameObject.SetActive(false);
        }
        
        // 4. Asignación nueva
        contentScroll.anchorMin = contentAnchor;
        topSection.SetParent(contentScroll);
        premiumSection.SetParent(contentScroll);
        hintsSection.SetParent(contentScroll);
        themesSection.SetParent(contentScroll);
        themesLayout.sizeDelta = Vector2.up * themesHeight;
        themesBound.sizeDelta = Vector2.up * themesHeight;
    }
    
    /// <summary>
    /// Actualiza el color de la tienda en función
    /// del tema actual escogido
    /// </summary>
    private void UpdateShopColor()
    {
        for (var i = 0; i < menu.Count; i++)
        {
            menu[i].color = currTheme.colors[i];
        }
    }

    /// <summary>
    /// Animación del rótulo colorido
    /// </summary>
    public void TitleColor()
    {
        var currColors = currTheme.colors;
        textMeshPro.text = "";
        for (int i = 0; i < textTittle.Length; i++)
        {
            textMeshPro.text += "<color=#" + ColorUtility.ToHtmlStringRGBA(currColors[i + tiendaIndex])
             + ">" + textTittle[i] + "</color>";
        }
        tiendaIndex = tiendaIndex >= textTittle.Length ? 0 : tiendaIndex + 1;
    }
    
    /// <summary>
    /// Oculta el banner de los anuncios
    /// </summary>
    public void HideBanner()
    {
        ads.HideBanner();
    }
    
//--------------------------------------------------------------------------------------------------------------------//

//---------------------------------------------BUTTON-CALLBACKS-------------------------------------------------------//
    public void BackToMainMenu()
    {
        gm.LoadScene((int) GameManager.SceneOrder.MAIN_MENU);
    }
    
    /// <summary>
    /// Añade pistas
    /// </summary>
    /// <param name="numHints">Numero de pistas a añadir</param>
    public void AddHints(int numHints)
    {
        currHints += numHints;
        gm.AddHints(currHints);
        hintsText.text = "¡Te quedan " + currHints + " pistas!";
    }

    /// <summary>
    /// Cambia el tema viejo por el escogido
    /// </summary>
    /// <param name="index">Tema que se escocge</param>
    /// <param name="selectedTheme">GameObject del tema actual seleccionado</param>
    private void ChangeTheme(int index, ThemeFeatures selectedTheme)
    {
        currTheme = themesList[index];
        if (currTheme.unlocked)
        {
            gm.SetTheme(index);
        }
        else
        {
            gm.UnlockTheme(index);
        }
        
        currThemeGo.selectedImage.enabled = false;
        selectedTheme.selectedImage.enabled = true;
        selectedTheme.selectedImage.sprite = selectedSprite;
        currThemeGo = selectedTheme;
        UpdateShopColor();
    }
    
    
    /// <summary>
    /// Desbloquea el premiun y desactiva el marco
    /// </summary>
    public void UnlockPremium()
    {
        gm.UnLockPremium();
        premiumSection.gameObject.SetActive(false);
        gm.LoadScene((int)GameManager.SceneOrder.SHOP);
    }
}