using GoogleMobileAds.Api;
using UnityEngine;

public class Ad : MonoBehaviour
{
    private InterstitialAd m_interstitialAd;
    private string m_adUnitID = "ca-app-pub-3940256099942544/1033173712";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // init ads
        MobileAds.Initialize(m_initStatus =>
        {
            print("Google ads are initialised");
        });
    }

    public void LoadInterstitialAd()
    {
        // if ad exists, delete it
        if(m_interstitialAd == null)
        {
            m_interstitialAd.Destroy();
            m_interstitialAd = null;
        }

        print("loading interstitial ad");

        var adRequest = new AdRequest();

        InterstitialAd.Load(m_adUnitID, adRequest,
            (InterstitialAd _ad, LoadAdError _err) =>
            {
                if (_err != null || _ad == null)
                {
                    // ad failed
                    print("interstitial ad failed to load. Error: " + _err);
                    return;
                }

                // ad loaded
                print("Interstitial ad loaded with resposnce: " + _ad.GetResponseInfo());
                m_interstitialAd = _ad;
            }
        );
    }

    public void ShowInterstitialAd()
    {
        if (m_interstitialAd != null && m_interstitialAd.CanShowAd())
        {
            Debug.Log("Showing Ad");
            m_interstitialAd.Show();
        }
        else
        {
            Debug.Log("Showing Ad failed. ad not ready");
        }
    }
}
