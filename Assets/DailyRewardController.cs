using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// this script uses player pref and device local time. players can cheat by changing local time on their device.
// TODO: use network time to prevent cheating

public class DailyRewardController : MonoBehaviour
{
    public Button m_button;
    public TextMeshProUGUI m_buttonText;
    static string m_lastClaimedRewardTimeKey = "LastClaimedRewardTime";
    [SerializeField] double m_cooldownMinutesTotal = 1;
    [SerializeField] int m_reward = 10;
    TimeSpan m_cooldownTimeRemaining;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //DailyRewardController.ClearSavedTimeData();

        m_button = GetComponent<Button>(); // get button
        m_buttonText = GetComponentInChildren<TextMeshProUGUI>(); // get text from component child

        m_button.onClick.AddListener(ClaimReward);
    }

    private void OnDestroy()
    {
        m_button.onClick.RemoveListener(ClaimReward);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRemainingTime();

        UpdateLogic();
    }

    void UpdateLogic()
    {
        if (PlayerPrefs.HasKey(m_lastClaimedRewardTimeKey))
        {
            DateTime lastClaimTime = DateTime.Parse(PlayerPrefs.GetString(m_lastClaimedRewardTimeKey), null, System.Globalization.DateTimeStyles.RoundtripKind);

            TimeSpan timeSinceLastClaimedReward = DateTime.UtcNow - lastClaimTime;

            if (timeSinceLastClaimedReward.TotalMinutes >= m_cooldownMinutesTotal)
            {
                // if time to claim reward
                EnableButton();
            }
            else
            {
                // if not time to claim reward...
                DisableButton();
            }
        }
        else 
        {
            // if first time claiming reward
            EnableButton();
        }
    }


    void ClaimReward()
    {
        Debug.Log("Daily reward claimed");

        InAppPurchaseController.AddCoins(m_reward);

        // save time of claiming
        PlayerPrefs.SetString(m_lastClaimedRewardTimeKey, DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();

        UpdateRemainingTime();

        // disable button
        DisableButton();

    }

    void EnableButton()
    {
        m_button.interactable = true;
        m_buttonText.text = "Claim Reward";
    }

    void DisableButton()
    {
        m_button.interactable = false;
        m_buttonText.text = m_cooldownTimeRemaining.ToString("hh\\:mm\\:ss");
    }

    // recalculate remaining time for timer
    void UpdateRemainingTime()
    {
        TimeSpan remainingTime = TimeSpan.Zero;

        // if no time exists, set to zero
        if (!PlayerPrefs.HasKey(m_lastClaimedRewardTimeKey))
        {
            m_cooldownTimeRemaining = TimeSpan.Zero;
            return;
        }
        
        // calculate remaining time by subtracting lastclaimed time from current time
        DateTime lastClaimTime = DateTime.Parse(PlayerPrefs.GetString(m_lastClaimedRewardTimeKey));
        TimeSpan timeSinceLastClaimedReward = DateTime.Now - lastClaimTime;
        remainingTime = TimeSpan.FromMinutes(m_cooldownMinutesTotal) - timeSinceLastClaimedReward;

        // clamp to above 0
        remainingTime = (remainingTime <= TimeSpan.Zero) ? TimeSpan.Zero : remainingTime;

        // update variable
        m_cooldownTimeRemaining = remainingTime;
    }

    // clears playerpref data
    public static void ClearSavedTimeData()
    {
        PlayerPrefs.DeleteKey(m_lastClaimedRewardTimeKey);
        PlayerPrefs.Save();
    }
   
}
