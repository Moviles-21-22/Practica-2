using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class ShopManager : MonoBehaviour
{
    [Tooltip("Referencia a AdsManager")] [SerializeField]
    private AdsManager ads;

    [Tooltip("Referencia a titleColor")] [SerializeField]
    private TitleColor title;

    [Tooltip("GameObject que muestra la oferta para comprar el premium")] [SerializeField]
    private GameObject premiumBox;

    [Tooltip("GameObject que muestras las pistas que quedan")] [SerializeField]
    private Text hintsText;

    [Header("Atributos relacionados con los temas")]
    [Tooltip("Lista de los atributos de cada tema de la tienda")]
    [SerializeField]
    private List<ThemeFeatures> themesFeatures;

    [Tooltip("Lista de las imágenes del menú que se ven cambiadas por el tema")] [SerializeField]
    private List<Image> menu;

    [Tooltip("Sprite del candado")] [SerializeField]
    private Sprite lockSprite;

    [Tooltip("Sprite del tick")] [SerializeField]
    private Sprite unlockSprite;

    [Tooltip("Lista de los paquetes de temas del juego")] [SerializeField]
    private List<ColorPack> colorThemes;
    
    private bool isPremium;
    private int currHints;
    private List<ColorPack> themesList;
    private ColorPack currTheme;
    private Image currThemeShop;
    private GameManager gm;
    
    public void Init(bool premium, GameManager.ThemeData theme, int numHints)
    {
        //TODO
        // gm = GameManager.instance;
        // isPremium = premium;
        // currHints = numHints;
        // themesList = themes;
        // currTheme = theme;

        ads.Init(isPremium);
        title.Init(currTheme.colors);

        InitElements();
    }

    private void InitElements()
    {
        if (isPremium)
        {
            premiumBox.SetActive(false);
        }

        hintsText.text = "¡Te quedan " + currHints + " pistas!";

        InitThemes();
        ChangeShopColor();
    }

    public void HideBanner()
    {
    }


    /// <summary>
    /// Desbloquea el premiun y desactiva el marco
    /// </summary>
    public void UnlockPremium()
    {
        gm.UnLockPremium();
        premiumBox.SetActive(false);
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

    [System.Serializable]
    public struct ThemeFeatures
    {
        public Image selectedImage;
        public Text themeName;
        public ColorPack theme;
        public List<Image> samples;
    }

    /// <summary>
    /// Cambia el tema viejo por el escogido
    /// </summary>
    /// <param name="index">Tema que se escocge</param>
    public void ChangeTheme(int index)
    {
        currTheme = themesFeatures[index].theme;
        var selectedImage = themesFeatures[index].selectedImage;

        if (currTheme.active)
        {
            gm.SetTheme(currTheme);
        }
        else
        {
            gm.UnlockTheme(currTheme);
        }

        currThemeShop.enabled = false;
        currThemeShop = selectedImage;
        currThemeShop.sprite = unlockSprite;
        currThemeShop.enabled = true;
        ChangeShopColor();
        title.ChangeTheme(currTheme.colors);
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

    /// <summary>
    /// Inicializa la lista de temas de la tienda
    /// </summary>
    private void InitThemes()
    {
        for (var i = 0; i < themesFeatures.Count; i++)
        {
            // Si es el tema actual, entonces se activa la imagen del tick
            if (themesList[i] == currTheme)
            {
                themesFeatures[i].selectedImage.enabled = true;
                currThemeShop = themesFeatures[i].selectedImage;
            }

            if (!themesList[i].active)
            {
                themesFeatures[i].selectedImage.enabled = true;
                themesFeatures[i].selectedImage.sprite = lockSprite;
                themesFeatures[i].selectedImage.rectTransform.sizeDelta.Set(50.0f, 50.0f);
            }

            themesFeatures[i].themeName.text = themesList[i].colorPackName;

            for (var j = 0; j < themesFeatures[i].samples.Count; j++)
            {
                themesFeatures[i].samples[j].color = themesList[i].colors[j];
            }
        }
    }
}