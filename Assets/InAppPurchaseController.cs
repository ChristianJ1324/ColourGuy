using System;
using UnityEngine;
using UnityEngine.UI;

public class InAppPurchaseController : MonoBehaviour
{
    private const string c_balanceKey = "ColourGuy.CoinBalance";

    [SerializeField, Min(1)]
    private int m_coinsPerPurchase = 100;
    [SerializeField] private Button m_buyCoinsButton;
    [SerializeField] private Text m_coinBalanceLabel;

    private static InAppPurchaseController s_instance;

    public static InAppPurchaseController Instance => s_instance;
    public int CoinBalance => PlayerPrefs.GetInt(c_balanceKey, 0);

    public static void ClearSavedGameData()
    {
        PlayerPrefs.DeleteKey(c_balanceKey);
        PlayerPrefs.Save();

        if (s_instance != null)
        {
            s_instance.UpdateBalanceLabel();
            s_instance.UpdateBuyButton();
        }
    }

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            enabled = false;
            return;
        }

        s_instance = this;

        UpdateBalanceLabel();
        UpdateBuyButton();
    }

    public void BuyCoinPack()
    {
        //TODO: Issues with license testers so bypassing in app purchases for now
        AddCoins(m_coinsPerPurchase);
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int balance = PlayerPrefs.GetInt(c_balanceKey, 0);
        if (balance > int.MaxValue - amount)
        {
            return;
        }

        PlayerPrefs.SetInt(c_balanceKey, balance + amount);
        PlayerPrefs.Save();
        s_instance?.UpdateBalanceLabel();
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0 || CoinBalance < amount)
        {
            return false;
        }

        PlayerPrefs.SetInt(c_balanceKey, CoinBalance - amount);
        PlayerPrefs.Save();
        UpdateBalanceLabel();
        return true;
    }

    private void UpdateBalanceLabel()
    {
        if (m_coinBalanceLabel == null)
        {
            return;
        }

        m_coinBalanceLabel.enabled = true;
        m_coinBalanceLabel.resizeTextForBestFit = false;
        m_coinBalanceLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        m_coinBalanceLabel.text = $"Coins: {CoinBalance}";
        RectTransform balanceTab = m_coinBalanceLabel.rectTransform.parent as RectTransform;
        if (balanceTab != null)
        {
            balanceTab.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Max(180f, m_coinBalanceLabel.preferredWidth + 68f));
        }
    }

    private void UpdateBuyButton()
    {
        if (m_buyCoinsButton != null)
        {
            m_buyCoinsButton.interactable = true;
            Text coinLabel = m_buyCoinsButton.GetComponentInChildren<Text>();
            if (coinLabel != null)
            {
                coinLabel.text = $"Add {m_coinsPerPurchase} Coins";
            }
        }
    }

    private void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }
}
