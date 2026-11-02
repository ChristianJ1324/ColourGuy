using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class InAppPurchaseController : MonoBehaviour
{
    private const string c_productID = "colourguy_coins_100";
    private const string c_balanceKey = "ColourGuy.CoinBalance";
    private const string c_purchaseKey = "ColourGuy.Purchase.";

    [SerializeField, Min(1)]
    private int m_coinsPerPurchase = 100;
    [SerializeField] private Button m_buyCoinsButton;
    [SerializeField] private Text m_coinBalanceLabel;

    private static InAppPurchaseController s_instance;
    private StoreController m_store;
    private Product m_coinProduct;
    private bool m_storeConnected;
    private bool m_storeUnavailable;
    private bool m_isBuying;

    public static InAppPurchaseController Instance => s_instance;
    public int CoinBalance => PlayerPrefs.GetInt(c_balanceKey, 0);
    public bool IsPurchaseInProgress => m_isBuying;

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

    private async void Start()
    {
        var catalog = new CatalogProvider();
        catalog.AddProduct(c_productID, ProductType.Consumable);

        m_store = UnityIAPServices.StoreController();
        m_store.OnStoreConnected += OnStoreConnected;
        m_store.OnStoreDisconnected += _ =>
        {
            m_storeConnected = false;
            m_storeUnavailable = true;
            UpdateBuyButton();
        };
        m_store.OnProductsFetched += OnProductsFetched;
        m_store.OnProductsFetchFailed += _ =>
        {
            m_coinProduct = null;
            m_storeUnavailable = true;
            UpdateBuyButton();
        };
        m_store.OnPurchasePending += OnPurchasePending;
        m_store.OnPurchaseFailed += _ => { m_isBuying = false; UpdateBuyButton(); };
        m_store.OnPurchaseDeferred += _ => { m_isBuying = true; UpdateBuyButton(); };
        m_store.OnPurchaseConfirmed += _ => { m_isBuying = false; UpdateBuyButton(); };
        m_store.OnPurchasesFetched += _ => UpdateBuyButton();
        m_store.OnPurchasesFetchFailed += _ => UpdateBuyButton();

        try
        {
            await m_store.Connect();
            catalog.FetchProducts(products => m_store.FetchProducts(products));
        }
        catch
        {
            m_storeConnected = false;
            m_storeUnavailable = true;
            UpdateBuyButton();
        }
    }

    private void OnStoreConnected()
    {
        m_storeConnected = true;
        m_storeUnavailable = false;
        UpdateBuyButton();
    }

    public void BuyCoinPack()
    {
        if (!m_storeConnected || m_coinProduct == null || m_isBuying)
        {
            return;
        }

#if UNITY_EDITOR
        AddCoins(m_coinsPerPurchase);
#else
        m_isBuying = true;
        UpdateBuyButton();
        m_store.PurchaseProduct(c_productID);
#endif
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

    private void OnProductsFetched(List<Product> products)
    {
        m_coinProduct = products.Find(product => product.definition.id == c_productID);
        m_storeUnavailable = m_coinProduct == null;
        UpdateBuyButton();
        if (m_coinProduct != null)
        {
            m_store.FetchPurchases();
        }
    }

    private void OnPurchasePending(PendingOrder order)
    {
        string transactionID = order.Info?.TransactionID;
        if (string.IsNullOrEmpty(transactionID))
        {
            Debug.LogError("Coin purchase could not be fulfilled; the order is still pending.");
            return;
        }

        bool foundCoinPack = false;
        foreach (var item in order.CartOrdered.Items())
        {
            if (item.Product.definition.id != c_productID)
            {
                continue;
            }

            foundCoinPack = true;
            string purchaseKey = c_purchaseKey + transactionID;
            if (!PlayerPrefs.HasKey(purchaseKey))
            {
                int balance = CoinBalance;
                if (balance > int.MaxValue - m_coinsPerPurchase)
                {
                    Debug.LogError("Coin purchase could not be fulfilled; the order is still pending.");
                    return;
                }

                // Save coins before confirming the purchase.
                PlayerPrefs.SetInt(c_balanceKey, balance + m_coinsPerPurchase);
                PlayerPrefs.SetInt(purchaseKey, 1);
                PlayerPrefs.Save();
                UpdateBalanceLabel();
            }
        }

        if (!foundCoinPack)
        {
            Debug.LogError("Coin purchase could not be fulfilled; the order is still pending.");
            return;
        }

        m_store.ConfirmPurchase(order);
    }

    private void UpdateBalanceLabel()
    {
        if (m_coinBalanceLabel != null)
        {
            m_coinBalanceLabel.text = $"Coins: {CoinBalance}";
        }
    }

    private void UpdateBuyButton()
    {
        if (m_buyCoinsButton == null)
        {
            return;
        }

        m_buyCoinsButton.interactable = m_storeConnected && m_coinProduct != null && !m_isBuying;
        Text label = m_buyCoinsButton.GetComponentInChildren<Text>();
        if (label != null)
        {
            if (m_coinProduct == null)
            {
                label.text = m_storeUnavailable ? "Coin Pack Unavailable" : "Loading Coin Pack...";
            }
            else
            {
#if UNITY_EDITOR
                label.text = $"Buy {m_coinsPerPurchase} Coins";
#else
                label.text = $"Buy {m_coinsPerPurchase} Coins - {m_coinProduct.metadata.localizedPriceString}";
#endif
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
