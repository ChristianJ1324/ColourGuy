using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AdsConsentManager : MonoBehaviour
{
    private static bool s_initializationStarted;
    private static bool s_isInitialized;
    private static bool s_privacyOptionsRequired;

    public static event Action AdsInitialized;
    public static event Action PrivacyOptionsStatusChanged;
    public static bool IsInitialized => s_isInitialized;
    public static bool PrivacyOptionsRequired => s_privacyOptionsRequired;

    private void Start()
    {
#if UNITY_EDITOR
        return;
#else
        // Ask Google for consent before initializing ads.
        InitializeAdsIfAllowed();
        ConsentInformation.Update(new ConsentRequestParameters(), OnConsentInfoUpdated);
#endif
    }

    private void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            SetPrivacyOptionsRequired(false);
            InitializeAdsIfAllowed();
            return;
        }

        SetPrivacyOptionsRequired(
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required);
        ConsentForm.LoadAndShowConsentFormIfRequired(_ => InitializeAdsIfAllowed());
    }

    private static void SetPrivacyOptionsRequired(bool required)
    {
        if (s_privacyOptionsRequired == required)
        {
            return;
        }

        s_privacyOptionsRequired = required;
        PrivacyOptionsStatusChanged?.Invoke();
    }

    private static void InitializeAdsIfAllowed()
    {
        if (s_initializationStarted || !ConsentInformation.CanRequestAds())
        {
            return;
        }

        s_initializationStarted = true;
        MobileAds.Initialize(_ =>
        {
            s_isInitialized = true;
            AdsInitialized?.Invoke();
        });
    }

    public static void ShowPrivacyOptions()
    {
        if (s_privacyOptionsRequired)
        {
            ConsentForm.ShowPrivacyOptionsForm(_ => { });
        }
    }
}
