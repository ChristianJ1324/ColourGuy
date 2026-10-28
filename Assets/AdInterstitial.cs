using GoogleMobileAds.Api;
using UnityEngine;

public class AdInterstitial : MonoBehaviour
{
    private InterstitialAd m_interstitialAd;
    private string m_adUnitID = "ca-app-pub-3940256099942544/1033173712";

    private InitializationStatus m_initStatus;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // init ads
        MobileAds.Initialize(m_initStatus =>
        {
            print("Google ads are initialised");

            // load interstitial ad on start
            LoadInterstitialAd();
        });
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    public void LoadInterstitialAd()
    {
        // if ad exists, delete it
        CleanUp();

        print("loading interstitial ad");

        var adRequest = new AdRequest();

        InterstitialAd.Load(
            m_adUnitID,
            adRequest,
            (InterstitialAd _ad, LoadAdError _err) =>
            {
                if (_err != null || _ad == null)
                {
                    // ad failed
                    print("interstitial ad failed to load. Error: " + _err);
                    return;
                }

                // ad loaded
                print("Interstitial ad loaded with response: " + _ad.GetResponseInfo());

                m_interstitialAd = _ad;

                // register event listeners
                AdInterstitialEventListeners(m_interstitialAd);
            }
        );
    }

    public void DoInterstitialAd()
    {
        ShowInterstitialAd();
    }

    public bool ShowInterstitialAd()
    {
        if (m_interstitialAd != null && m_interstitialAd.CanShowAd())
        {
            Debug.Log("Showing Ad");
            m_interstitialAd.Show();
            return true;
        }
        else
        {
            Debug.Log("Showing Ad failed. ad not ready");
            return false;
        }
    }


    public void AdInterstitialEventListeners(InterstitialAd _ad)
    {
        _ad.OnAdClicked += () =>
        {
            print("ad clicked");
        };
        _ad.OnAdFullScreenContentOpened += () =>
        {
            print("ad opened");
        };
        _ad.OnAdFullScreenContentClosed += () =>
        {
            print("ad closed");

            // reload the ad to show another one asap
            LoadInterstitialAd();
        };
        _ad.OnAdFullScreenContentFailed += (AdError _err) =>
        {
            print("ad failed , error: " + _err);

            // reload the ad to show another one asap
            LoadInterstitialAd();
        };
    }

    private void CleanUp()
    {
        // cleanup
        if (m_interstitialAd != null)
        {
            m_interstitialAd.Destroy();
            m_interstitialAd = null;
        }
    }


}
