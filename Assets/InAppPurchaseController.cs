using UnityEngine;
using UnityEngine.UI;

public class InAppPurchaseController : MonoBehaviour
{
    // keep the demo balance saved on this device
    private const string c_demoCoinBalanceKey = "ColourGuy.DemoCoinBalance";

    [SerializeField, Min(1)] private int m_fakeCoinsPerPurchase = 100;
    [SerializeField] private Button m_buyCoinsButton;
    [SerializeField] private Text m_coinBalanceLabel;

    private static InAppPurchaseController s_instance;

    public static InAppPurchaseController Instance => s_instance;
    public int CoinBalance => PlayerPrefs.GetInt(c_demoCoinBalanceKey, 0);

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            enabled = false;
            return;
        }

        s_instance = this;

        if (m_buyCoinsButton != null)
        {
            m_buyCoinsButton.interactable = true;
        }

        UpdateCoinBalanceLabel();
    }

    public void BuyCoinPack()
    {
        AddDemoCoins(m_fakeCoinsPerPurchase);
    }

    // add coins and save the updated balance
    public static void AddDemoCoins(int amount)
    {
        int currentBalance = PlayerPrefs.GetInt(c_demoCoinBalanceKey, 0);

        if (amount <= 0 || currentBalance > int.MaxValue - amount)
        {
            return;
        }

        PlayerPrefs.SetInt(c_demoCoinBalanceKey, currentBalance + amount);
        PlayerPrefs.Save();
        s_instance?.UpdateCoinBalanceLabel();
    }

    public bool TrySpendCoins(int amount)
    {
        // spend coins only when enough are available
        if (amount <= 0 || CoinBalance < amount)
        {
            return false;
        }

        PlayerPrefs.SetInt(c_demoCoinBalanceKey, CoinBalance - amount);
        PlayerPrefs.Save();
        UpdateCoinBalanceLabel();
        return true;
    }

    private void UpdateCoinBalanceLabel()
    {
        // keep the menu label in sync with the saved balance
        if (m_coinBalanceLabel != null)
        {
            m_coinBalanceLabel.text = $"Demo Coins: {CoinBalance}";
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
