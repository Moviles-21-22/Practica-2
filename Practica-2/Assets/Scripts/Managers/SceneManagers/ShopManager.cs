using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
public class ShopManager : MonoBehaviour
{
    [Tooltip("Referencia a AdsManager")] [SerializeField]
    private AdsManager ads;

    [Tooltip("GameObject que muestra la oferta para comprar el premium")] [SerializeField]
    private GameObject premiumBox;

    [Tooltip("GameObject que muestras las pistas que quedan")] [SerializeField]
    private TextMeshProUGUI hintsText;

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
    private List<GameManager.ThemeData> themesList;
    private GameManager.ThemeData currTheme;
    private Image currThemeShop;
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
        ads.HideBanner();
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
        currTheme = themesList[index];
        var selectedImage = themesFeatures[index].selectedImage;

        if (currTheme.unlocked)
        {
            gm.SetTheme(index);
        }
        else
        {
            gm.UnlockTheme(index);
        }

        currThemeShop.enabled = false;
        currThemeShop = selectedImage;
        currThemeShop.sprite = unlockSprite;
        currThemeShop.enabled = true;
        ChangeShopColor();
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

            if (!themesList[i].unlocked)
            {
                themesFeatures[i].selectedImage.enabled = true;
                themesFeatures[i].selectedImage.sprite = lockSprite;
                themesFeatures[i].selectedImage.rectTransform.sizeDelta.Set(50.0f, 50.0f);
            }

            themesFeatures[i].themeName.text = themesList[i].name;

            for (var j = 0; j < themesFeatures[i].samples.Count; j++)
            {
                themesFeatures[i].samples[j].color = themesList[i].colors[j];
            }
        }
    }
}