using UnityEngine;
using UnityEngine.Advertisements;

//check Amaro

//  Se encarga de gestionar todos los anuncios
public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Tooltip("Posición del banner")] [SerializeField]
    BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    [Tooltip("Unidad de banner para Android")] [SerializeField]
    string bannerAndroidUnit = "Banner_Android";

    [Tooltip("Unidad de interstitial para Android")] [SerializeField]
    string interstitialAndroidUnit = "Interstitial_Android";

    [Tooltip("Unidad de reward para Android")] [SerializeField]
    string rewardAndroidUnit = "Rewarded_Android";

    [Tooltip("Game id de android")] [SerializeField]
    public string androidGameId;

    [Tooltip("Test selectedImage")] [SerializeField]
    public bool testMode;

    [Tooltip("Numero de pistas que se dan al ver un video reward")] [SerializeField]
    private uint numRewardVideoHints = 1;

    /// <summary>
    /// Instancia del adsManager
    /// </summary>
    public static AdsManager instance = null;

    /// <summary>
    /// Inicialización del ads manager
    /// </summary>
    public void Init()
    {
        if (instance == null)
        {
#if UNITY_EDITOR
            testMode = true;
#elif UNITY_ANDROID
        testMode = false;
#endif
            Advertisement.Initialize(androidGameId, testMode, this);
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se delega la información del gameObject actual para que sea 
            // la instancia estática quien la tenga
            instance.testMode = testMode;
            instance.bannerPosition = bannerPosition;
            instance.bannerAndroidUnit = bannerAndroidUnit;
            instance.interstitialAndroidUnit = this.interstitialAndroidUnit;
            instance.rewardAndroidUnit = rewardAndroidUnit;
            instance.androidGameId = androidGameId;
            
            Advertisement.Initialize(instance.androidGameId, instance.testMode, this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Para reproducir un video reward
    /// </summary>
    public static void PlayAd()
    {
        Advertisement.Load(instance.rewardAndroidUnit, instance);
    }

    public void PlayInterstitial()
    {
        if (!GameManager.instance.IsPlayerPremium())
        {
            Advertisement.Load(interstitialAndroidUnit, this);
        }
    }

    /// <summary>
    /// Para mostrar el baner en la parte inferior del juego
    /// </summary>
    public void ShowBanner()
    {
        if (!GameManager.instance.IsPlayerPremium())
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

    /// <summary>
    /// Muestra un anuncio del tipo interstitial
    /// </summary>
    public void ShowInterstitial()
    {
        Advertisement.Show(interstitialAndroidUnit, this);
    }

    /// <summary>
    /// Para eliminar los banners del juego si el jugador es premium
    /// </summary>
    public void HideBanner()
    {
        if (GameManager.instance.IsPlayerPremium()) Advertisement.Banner.Hide();
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
    /// Cuando se termina de interactuar con un ad
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
                GameManager.instance.AddHints((int) numRewardVideoHints);
                break;
            case UnityAdsShowCompletionState.UNKNOWN:
                Debug.LogError("Error desconocido al visualizar el video");
                break;
            default:
                break;
        }
    }
}