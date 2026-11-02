using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class AdRewarded : MonoBehaviour
{
    private const string c_testAdUnitID = "ca-app-pub-3940256099942544/5224354917";
    private RewardedAd m_ad;
    private bool m_isLoading;
    private bool m_isShowing;
    private bool m_rewardEarned;
    private Action m_pendingReward;
    private Action m_pendingFailure;

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
        CancelPendingRewardedAd();
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
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                m_isLoading = false;
                if (error != null || ad == null)
                {
                    FailPendingReward();
                    Invoke(nameof(LoadRewardedAd), 5f);
                    return;
                }

                m_ad = ad;
                m_ad.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(HandleAdClosed);
                m_ad.OnAdFullScreenContentFailed += adError =>
                    MobileAdsEventExecutor.ExecuteInUpdate(() => HandleAdFailed(adError));

                if (m_pendingReward != null)
                {
                    ShowLoadedAd();
                }
            });
        });
    }

    public bool ShowRewardedAd(Action onReward, Action onUnavailable)
    {
        if (m_isShowing || m_pendingReward != null)
        {
            return false;
        }

        if (!AdsConsentManager.IsInitialized)
        {
            return false;
        }

        m_pendingReward = onReward;
        m_pendingFailure = onUnavailable;

        if (IsReady)
        {
            ShowLoadedAd();
        }
        else
        {
            LoadRewardedAd();
        }

        return true;
    }

    public bool ShowRewardedAd(Action onReward)
    {
        // Grant the reward only when the ad finishes.
        return ShowRewardedAd(onReward, null);
    }

    public void CancelPendingRewardedAd()
    {
        m_pendingReward = null;
        m_pendingFailure = null;
    }

    private void ShowLoadedAd()
    {
        if (!IsReady)
        {
            FailPendingReward();
            LoadRewardedAd();
            return;
        }

        m_isShowing = true;
        m_rewardEarned = false;
        m_ad.Show(_ => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            m_rewardEarned = true;
        }));
    }

    private void HandleAdClosed()
    {
        m_isShowing = false;
        if (m_rewardEarned)
        {
            Action reward = m_pendingReward;
            CancelPendingRewardedAd();
            reward?.Invoke();
        }
        else
        {
            FailPendingReward();
        }

        m_rewardEarned = false;
        DestroyCurrentAd();
        LoadRewardedAd();
    }

    private void HandleAdFailed(AdError error)
    {
        m_isShowing = false;
        if (m_rewardEarned)
        {
            Action reward = m_pendingReward;
            CancelPendingRewardedAd();
            reward?.Invoke();
        }
        else
        {
            FailPendingReward();
        }
        m_rewardEarned = false;
        DestroyCurrentAd();
        LoadRewardedAd();
    }

    private void FailPendingReward()
    {
        Action failure = m_pendingFailure;
        CancelPendingRewardedAd();
        failure?.Invoke();
    }

    private void DestroyCurrentAd()
    {
        if (m_ad != null)
        {
            m_ad.Destroy();
            m_ad = null;
        }
    }
}
