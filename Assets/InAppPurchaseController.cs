using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class InAppPurchaseController : MonoBehaviour
{
    private const string c_productID = "colourguy_coins_100";
    private const string c_dashProductID = "colourguy_dash";
    private const string c_balanceKey = "ColourGuy.CoinBalance";
    private const string c_dashOwnedKey = "ColourGuy.DashOwned";
    private const string c_purchaseKey = "ColourGuy.Purchase.";

    [SerializeField, Min(1)]
    private int m_coinsPerPurchase = 100;
    [SerializeField] private Button m_buyCoinsButton;
    [SerializeField] private Button m_buyDashButton;
    [SerializeField] private Text m_coinBalanceLabel;

    private static InAppPurchaseController s_instance;
    private StoreController m_store;
    private Product m_coinProduct;
    private Product m_dashProduct;
    private bool m_storeConnected;
    private bool m_coinUnavailable;
    private bool m_dashUnavailable;
    private bool m_isBuying;

    public static InAppPurchaseController Instance => s_instance;
    public int CoinBalance => PlayerPrefs.GetInt(c_balanceKey, 0);
    public bool HasDash => PlayerPrefs.GetInt(c_dashOwnedKey, 0) == 1;
    public bool IsPurchaseInProgress => m_isBuying;

    public static void ClearSavedGameData()
    {
        PlayerPrefs.DeleteKey(c_balanceKey);
        PlayerPrefs.DeleteKey(c_dashOwnedKey);
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

    private async void Start()
    {
        var catalog = new CatalogProvider();
        catalog.AddProduct(c_productID, ProductType.Consumable);
        catalog.AddProduct(c_dashProductID, ProductType.NonConsumable);

        m_store = UnityIAPServices.StoreController();
        m_store.OnStoreConnected += OnStoreConnected;
        m_store.OnStoreDisconnected += _ =>
        {
            m_storeConnected = false;
            m_coinUnavailable = true;
            m_dashUnavailable = true;
            UpdateBuyButton();
        };
        m_store.OnProductsFetched += OnProductsFetched;
        m_store.OnProductsFetchFailed += _ =>
        {
            m_coinProduct = null;
            m_dashProduct = null;
            m_coinUnavailable = true;
            m_dashUnavailable = true;
            UpdateBuyButton();
        };
        m_store.OnPurchasePending += OnPurchasePending;
        m_store.OnPurchaseFailed += _ => { m_isBuying = false; UpdateBuyButton(); };
        m_store.OnPurchaseDeferred += _ => { m_isBuying = true; UpdateBuyButton(); };
        m_store.OnPurchaseConfirmed += _ => { m_isBuying = false; UpdateBuyButton(); };
        m_store.OnPurchasesFetched += OnPurchasesFetched;
        m_store.OnPurchasesFetchFailed += _ => UpdateBuyButton();

        try
        {
            await m_store.Connect();
            catalog.FetchProducts(products => m_store.FetchProducts(products));
        }
        catch
        {
            m_storeConnected = false;
            m_coinUnavailable = true;
            m_dashUnavailable = true;
            UpdateBuyButton();
        }
    }

    private void OnStoreConnected()
    {
        m_storeConnected = true;
        m_coinUnavailable = false;
        m_dashUnavailable = false;
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

    public void BuyDash()
    {
        if (!m_storeConnected || m_dashProduct == null || m_isBuying || HasDash)
        {
            return;
        }

#if UNITY_EDITOR
        SetDashOwned();
#else
        m_isBuying = true;
        UpdateBuyButton();
        m_store.PurchaseProduct(c_dashProductID);
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
        m_dashProduct = products.Find(product => product.definition.id == c_dashProductID);
        m_coinUnavailable = m_coinProduct == null;
        m_dashUnavailable = m_dashProduct == null;
        UpdateBuyButton();
        if (m_coinProduct != null || m_dashProduct != null)
        {
            m_store.FetchPurchases();
        }
    }

    private void OnPurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            if (order.CartOrdered == null)
            {
                continue;
            }

            foreach (var item in order.CartOrdered.Items())
            {
                if (item.Product.definition.id == c_dashProductID)
                {
                    SetDashOwned();
                    break;
                }
            }
        }

        UpdateBuyButton();
    }

    private void OnPurchasePending(PendingOrder order)
    {
        string transactionID = order.Info?.TransactionID;
        bool foundCoinPack = false;
        bool foundDash = false;
        foreach (var item in order.CartOrdered.Items())
        {
            string productID = item.Product.definition.id;
            if (productID == c_dashProductID)
            {
                foundDash = true;
                SetDashOwned();
            }
            else if (productID == c_productID)
            {
                foundCoinPack = true;
            }
        }

        if (foundCoinPack)
        {
            if (string.IsNullOrEmpty(transactionID))
            {
                Debug.LogError("Coin purchase could not be fulfilled; the order is still pending.");
                return;
            }

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

        if (!foundCoinPack && !foundDash)
        {
            return;
        }

        m_store.ConfirmPurchase(order);
    }

    private void SetDashOwned()
    {
        if (HasDash)
        {
            return;
        }

        PlayerPrefs.SetInt(c_dashOwnedKey, 1);
        PlayerPrefs.Save();
        UpdateBuyButton();
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
                Mathf.Max(160f, m_coinBalanceLabel.preferredWidth + 48f));
        }
    }

    private void UpdateBuyButton()
    {
        if (m_buyCoinsButton != null)
        {
            m_buyCoinsButton.interactable = m_storeConnected && m_coinProduct != null && !m_isBuying;
            Text coinLabel = m_buyCoinsButton.GetComponentInChildren<Text>();
            if (coinLabel != null)
            {
                if (m_coinProduct == null)
                {
                    coinLabel.text = m_coinUnavailable ? "Coin Pack Unavailable" : "Loading Coin Pack...";
                }
                else
                {
#if UNITY_EDITOR
                    coinLabel.text = $"Buy {m_coinsPerPurchase} Coins";
#else
                    coinLabel.text = $"Buy {m_coinsPerPurchase} Coins - {m_coinProduct.metadata.localizedPriceString}";
#endif
                }
            }
        }

        if (m_buyDashButton != null)
        {
            m_buyDashButton.interactable = m_storeConnected && m_dashProduct != null && !m_isBuying && !HasDash;
            Text dashLabel = m_buyDashButton.GetComponentInChildren<Text>();
            if (dashLabel != null)
            {
                if (HasDash)
                {
                    dashLabel.text = "Dash Unlocked";
                }
                else if (m_dashProduct == null)
                {
                    dashLabel.text = m_dashUnavailable ? "Dash Unavailable" : "Loading Dash...";
                }
                else
                {
#if UNITY_EDITOR
                    dashLabel.text = "Buy Dash - $5.00";
#else
                    dashLabel.text = $"Buy Dash - {m_dashProduct.metadata.localizedPriceString}";
#endif
                }
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
