using GoogleMobileAds.Api;
using UnityEngine;

public class AdInterstitial : MonoBehaviour
{
    private const string c_testAdUnitID = "ca-app-pub-3940256099942544/1033173712";
    private InterstitialAd m_ad;
    private bool m_isLoading;

    private void OnEnable()
    {
        AdsConsentManager.AdsInitialized += LoadInterstitialAd;
        if (AdsConsentManager.IsInitialized)
        {
            LoadInterstitialAd();
        }
    }

    private void OnDisable()
    {
        AdsConsentManager.AdsInitialized -= LoadInterstitialAd;
        CancelInvoke(nameof(LoadInterstitialAd));
    }

    private void OnDestroy()
    {
        if (m_ad != null)
        {
            m_ad.Destroy();
        }
    }

    public void LoadInterstitialAd()
    {
        if (m_isLoading || !AdsConsentManager.IsInitialized)
        {
            return;
        }

        CancelInvoke(nameof(LoadInterstitialAd));
        m_ad?.Destroy();
        m_ad = null;
        m_isLoading = true;

        InterstitialAd.Load(c_testAdUnitID, new AdRequest(), (ad, error) =>
        {
            m_isLoading = false;
            if (error != null || ad == null)
            {
                Invoke(nameof(LoadInterstitialAd), 5f);
                return;
            }

            m_ad = ad;
            // Have the next ad ready after this one closes.
            m_ad.OnAdFullScreenContentClosed += LoadInterstitialAd;
            m_ad.OnAdFullScreenContentFailed += _ => LoadInterstitialAd();
        });
    }

    public bool ShowInterstitialAd()
    {
        if (m_ad == null || !m_ad.CanShowAd())
        {
            return false;
        }

        m_ad.Show();
        return true;
    }
}
