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
    private Sprite lockSprite;

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

    private bool isPremium;
    private int currHints;
    private List<GameManager.ThemeData> themesList;
    private GameManager.ThemeData currTheme;
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
    }

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

    private void InitElements()
    {
        hintsText.text = "¡Te quedan " + currHints + " pistas!";

        InitThemes();
        ChangeShopColor();
    }

    public void HideBanner()
    {
        ads.HideBanner();
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

    public void BackToMainMenu()
    {
        gm.LoadScene((int) GameManager.SceneOrder.MAIN_MENU);
    }

    /// <summary>
    /// Cambia el tema viejo por el escogido
    /// </summary>
    /// <param name="index">Tema que se escocge</param>
    public void ChangeTheme(int index)
    {
        // currTheme = themesList[index];
        // var selectedImage = themesFeatures[index].selectedImage;
        //
        // if (currTheme.unlocked)
        // {
        //     gm.SetTheme(index);
        // }
        // else
        // {
        //     gm.UnlockTheme(index);
        // }
        //
        // currThemeShop.enabled = false;
        // currThemeShop = selectedImage;
        // currThemeShop.sprite = unlockSprite;
        // currThemeShop.enabled = true;
        // ChangeShopColor();
    }

    /// <summary>
    /// Cambia el color de la tienda
    /// </summary>
    private void ChangeShopColor()
    {
        for (var i = 0; i < menu.Count; i++)
        {
            menu[i].color = currTheme.colors[i];
        }
    }

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
            var premiumAnchor = premiumSection.anchorMin;
            //contentAnchor += premiumAnchor;
            
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
            
            // Si es el tema actual, entonces se activa la imagen del tick
            if (themesList[i] == currTheme)
            {
                themeGo.selectedImage.enabled = true;
            }

            if (!themesList[i].unlocked)
            {
                themeGo.selectedImage.enabled = true;
                themeGo.selectedImage.sprite = lockSprite;
                themeGo.selectedImage.rectTransform.sizeDelta.Set(50.0f, 50.0f);
            }

            themeGo.themeName.text = themesList[i].name;

            for (var j = 0; j < themeGo.samples.Count; j++)
            {
                themeGo.samples[j].color = themesList[i].colors[j];
            }
        }
    }
}