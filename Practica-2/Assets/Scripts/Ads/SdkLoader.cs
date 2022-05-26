using UnityEngine;
using UnityEngine.Advertisements;

public class SdkLoader : MonoBehaviour , IUnityAdsInitializationListener
{
    [SerializeField] string androidGameId;
    [SerializeField] bool testMode = true;
    [SerializeField] bool enablePerPlacementMode = true;
    private string gameId;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        gameId = androidGameId;
        Advertisement.Initialize(gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}