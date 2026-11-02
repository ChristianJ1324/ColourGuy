using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdRewarded : MonoBehaviour
{
    private const string c_testAdUnitID = "ca-app-pub-3940256099942544/5224354917";
    private RewardedAd m_ad;
    private bool m_isLoading;

    public bool IsReady => m_ad != null && m_ad.CanShowAd();

    private void OnEnable()
    {
        AdsConsentManager.AdsInitialized += LoadRewardedAd;
        if (AdsConsentManager.IsInitialized)
        {
            LoadRewardedAd();
        }
    }

    private void OnDisable()
    {
        AdsConsentManager.AdsInitialized -= LoadRewardedAd;
        CancelInvoke(nameof(LoadRewardedAd));
    }

    private void OnDestroy()
    {
        if (m_ad != null)
        {
            m_ad.Destroy();
        }
    }

    public void LoadRewardedAd()
    {
        if (m_isLoading || !AdsConsentManager.IsInitialized)
        {
            return;
        }

        CancelInvoke(nameof(LoadRewardedAd));
        m_ad?.Destroy();
        m_ad = null;
        m_isLoading = true;

        RewardedAd.Load(c_testAdUnitID, new AdRequest(), (ad, error) =>
        {
            m_isLoading = false;
            if (error != null || ad == null)
            {
                Invoke(nameof(LoadRewardedAd), 5f);
                return;
            }

            m_ad = ad;
            m_ad.OnAdFullScreenContentClosed += LoadRewardedAd;
            m_ad.OnAdFullScreenContentFailed += _ => LoadRewardedAd();
        });
    }

    public bool ShowRewardedAd(Action onReward)
    {
        if (!IsReady)
        {
            return false;
        }

        // Grant the reward only when the ad finishes.
        m_ad.Show(_ => onReward?.Invoke());
        return true;
    }
}
