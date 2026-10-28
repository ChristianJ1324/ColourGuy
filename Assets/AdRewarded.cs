using GoogleMobileAds.Api;
using UnityEngine;

public class AdRewarded : MonoBehaviour
{
    private RewardedAd m_rewardedAd;
    private string m_adUnitID = "ca-app-pub-3940256099942544/5224354917";

    private InitializationStatus m_initStatus;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

       MobileAds.Initialize(m_initStatus =>
       {
           print("Google ads are initialised");

           // load rewarded ad on start
           LoadRewardedAd();
       });
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    public void LoadRewardedAd()
    {
        // if ad exists, delete it
        CleanUp();

        print("loading Rewarded ad");

        var adRequest = new AdRequest();

        RewardedAd.Load(
            m_adUnitID,
            adRequest,
            (RewardedAd _ad, LoadAdError _err) =>
            {
                if (_err != null || _ad == null)
                {
                    // ad failed
                    print("Rewarded ad failed to load. Error: " + _err);
                    return;
                }

                // ad loaded
                print("Rewarded ad loaded with response: " + _ad.GetResponseInfo());

                m_rewardedAd = _ad;

                // register event listeners
                AdRewardedEventListeners(m_rewardedAd);
            }
        );
    }

    public void DoRewardedAd()
    {
        ShowRewardedAd();
    }

    public bool ShowRewardedAd()
    {
        if (m_rewardedAd != null && m_rewardedAd.CanShowAd())
        {
            Debug.Log("Showing Ad");
            m_rewardedAd.Show((Reward _reward) =>
            {
                // reard the user
                print($"Ad reward user: type: {_reward.Type}, amount: {_reward.Amount}");
            });
            return true;
        }
        else
        {
            Debug.Log("Showing Ad failed. ad not ready");
            return false;
        }
    }


    public void AdRewardedEventListeners(RewardedAd _ad)
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
            LoadRewardedAd();
        };
        _ad.OnAdFullScreenContentFailed += (AdError _err) =>
        {
            print("ad failed , error: " + _err);

            // reload the ad to show another one asap
            LoadRewardedAd();
        };
    }

    private void CleanUp()
    {
        // cleanup
        if (m_rewardedAd != null)
        {
            m_rewardedAd.Destroy();
            m_rewardedAd = null;
        }
    }


}
