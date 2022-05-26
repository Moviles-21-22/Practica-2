using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
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

    ///// <summary>
    ///// Determina si el jugador es premium
    ///// </summary>
    //private bool isPremium;

    ///// <summary>
    ///// Determina si entregar el premio al jugador
    ///// </summary>
    //private bool rewarded = false;

    [Tooltip("Numero de pistas que se dan al ver un video reward")]
    [SerializeField] private uint numRewardVideoHints = 1;

    /// <summary>
    /// Instancia del adsManager
    /// </summary>
    public static AdsManager instance = null;

    /// <summary>
    /// Inicialización del ads manager
    /// </summary>
    /// <param name="premium">Si activar o no los ads</param>
    public void Init()
    {
        if (instance == null) instance = this;

#if UNITY_EDITOR
        testMode = true;
#elif UNITY_ANDROID
        testMode = false;
#endif
        Advertisement.Initialize(androidGameId, testMode, this);
    }

    /// <summary>
    /// Para reproducir un video reward
    /// </summary>
    public void PlayAd()
    {
        Advertisement.Load(rewardAndroidUnit, this);
    }

    public void PlayInterstitial()
    {
        if (!GameManager.instance.isPlayerPremium())
        {
            Advertisement.Load(interstitialAndroidUnit, this);
        }
    }

    /// <summary>
    /// Para mostrar el baner en la parte inferior del juego
    /// </summary>
    public void ShowBanner()
    {
        if (!GameManager.instance.isPlayerPremium())
        {
            Advertisement.Banner.SetPosition(bannerPosition);
            Advertisement.Banner.Show(bannerAndroidUnit);
        }
    }


    //------------------INTERFACES------------------------

    /// <summary>
    /// Cuando los ads se activan
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsReady(string placementId)
    {
        ShowBanner();
    }



    public void ShowInterstitial()
    {
        Advertisement.Show(interstitialAndroidUnit, this);
    }


    /// <summary>
    /// Para eliminar los banners del juego
    /// </summary>
    public void HideBanner()
    {
        if (GameManager.instance.isPlayerPremium()) Advertisement.Banner.Hide();
    }

    /// <summary>
    /// Cuando los ads se han cargado correctamente
    /// </summary>
    public void OnInitializationComplete()
    {
        ShowBanner();
    }

    /// <summary>
    /// Cuando existe un error al cargar los ads
    /// </summary>
    /// <param name="error"></param>
    /// <param name="message"></param>
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError(message);
    }

    /// <summary>
    /// Muestra el ad
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Advertisement.Show(rewardAndroidUnit, this);
    }

    /// <summary>
    /// En caso de que falle la carga del ads
    /// </summary>
    /// <param name="placementId"></param>
    /// <param name="error"></param>
    /// <param name="message"></param>
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError(message);
    }

    /// <summary>
    /// Cuando un ad falla en la reproducción
    /// </summary>
    /// <param name="placementId"></param>
    /// <param name="error"></param>
    /// <param name="message"></param>
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError(message);
        switch (error)
        {
            case UnityAdsShowError.NOT_INITIALIZED:
                break;
            case UnityAdsShowError.NOT_READY:
                break;
            case UnityAdsShowError.VIDEO_PLAYER_ERROR:
                break;
            case UnityAdsShowError.INVALID_ARGUMENT:
                break;
            case UnityAdsShowError.NO_CONNECTION:
                break;
            case UnityAdsShowError.ALREADY_SHOWING:
                break;
            case UnityAdsShowError.INTERNAL_ERROR:
                break;
            case UnityAdsShowError.UNKNOWN:
                break;
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        print("Empieza a verse el video");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        print("Click");
    }

    /// <summary>
    /// Cuando se termina de ver un ads
    /// </summary>
    /// <param name="placementId"></param>
    /// <param name="showCompletionState"></param>
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        switch (showCompletionState)
        {
            case UnityAdsShowCompletionState.SKIPPED:
                Debug.Log("Video skipped");
                break;
            case UnityAdsShowCompletionState.COMPLETED:
                Debug.Log("Reward correctamente reproducido");
                GameManager.instance.AddHints((int)numRewardVideoHints);
                break;
            case UnityAdsShowCompletionState.UNKNOWN:
                Debug.LogError("Error desconocido al visualizar el video");
                break;
            default:
                break;
        }
    }
}