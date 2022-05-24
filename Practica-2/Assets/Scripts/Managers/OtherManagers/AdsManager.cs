using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    [Tooltip("Posición del banner")]
    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    [Tooltip("Unidad de banner para Android")]
    [SerializeField] string bannerAndroidUnit = "Banner_Android";

    [Tooltip("Unidad de interstitial para Android")]
    [SerializeField] string interstitialAndroidUnit = "Interstitial_Android";

    [Tooltip("Unidad de reward para Android")]
    [SerializeField] string rewardAndroidUnit = "Rewarded_Android";

    [Tooltip("Game id de android")]
    [SerializeField] public string androidGameId;

    [Tooltip("Test selectedImage")]
    [SerializeField]  public bool testMode;

    private bool isPremium;
    public void Init(bool premium)
    {
        isPremium = premium;
        if (!isPremium)
        {
            Advertisement.Initialize(androidGameId, testMode);
            ShowBanner();
        }
        else
        {
            print("Es premium");
        }
    }

    public void ShowBanner()
    {
        Advertisement.Banner.SetPosition(bannerPosition);
        Advertisement.Banner.Show(androidGameId);
    }

    public void ShowInterstitial()
    {
        Advertisement.Load(interstitialAndroidUnit);
        Advertisement.Show(androidGameId);
    }

    public void ShowRewardVideo()
    {
        Advertisement.Load(rewardAndroidUnit);
        Advertisement.Show(androidGameId);
    }

    public void HideBanner()
    {
        if (isPremium)
        {
            Advertisement.Banner.Hide();
        }
    }
}