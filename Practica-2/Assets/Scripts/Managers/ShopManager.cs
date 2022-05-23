using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private bool isPremium;
    private int currHints;

    public void Init(bool premium, List<Color> theme, int numHints)
    {
        isPremium = premium;
        currHints = numHints;
        
        ads.Init(isPremium);
        title.Init(theme);
        
        InitElements();
    }

    private void InitElements()
    {
        if (isPremium)
        {
            premiumBox.SetActive(false);
        }
        
        hintsText.text = "¡Te quedan " + currHints + " pistas!";
    }

    public void HideBanner()
    {
    }


    /// <summary>
    /// Desbloquea el premiun y desactiva el marco
    /// </summary>
    public void UnlockPremium()
    {
        GameManager.instance.UnLockPremium();
        premiumBox.SetActive(false);
    }
    
    
    /// <summary>
    /// Añade pistas
    /// </summary>
    /// <param name="numHints">Numero de pistas a añadir</param>
    public void AddHints(int numHints)
    {
        currHints += numHints;
        GameManager.instance.AddHints(currHints);
        hintsText.text = "¡Te quedan " + currHints + " pistas!";
    }
}