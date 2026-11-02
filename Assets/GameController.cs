using System;
using UnityEngine;
using UnityEngine.InputSystem;
using GoogleMobileAds.Api;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody2D m_playerBody;
    [SerializeField] private PlayerLose m_playerLose;
    [SerializeField] private AdRewarded m_rewardedAd;
    [SerializeField] private AdInterstitial m_interstitialAd;

    public event Action GameOver;

    private bool m_isGameOver;

    InitializationStatus m_initStatus;

    private void Awake()
    {
        Time.timeScale = 1f;
        m_playerBody.simulated = true;
    }

    private void OnEnable()
    {
        if (m_playerLose != null)
        {
            m_playerLose.PlayerDied += HandlePlayerDied;
        }
    }

    private void OnDisable()
    {
        if (m_playerLose != null)
        {
            m_playerLose.PlayerDied -= HandlePlayerDied;
        }
    }

    void Start()
    {
        m_playerInput.enabled = true;
    }

    public void StartGame()
    {
        m_isGameOver = false;
        m_playerLose.Respawn();
        Time.timeScale = 1f;
        m_playerBody.simulated = true;
        m_playerInput.enabled = true;
    }

    public void PauseGame()
    {
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        m_playerBody.simulated = true;
        m_playerInput.enabled = true;
    }

    public void ReturnToMenu()
    {
        m_isGameOver = false;
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
        Time.timeScale = 0f;
    }

    public bool ReviveWithCoins(int coinCost)
    {
        if (!m_isGameOver || InAppPurchaseController.Instance == null ||
            !InAppPurchaseController.Instance.TrySpendCoins(coinCost))
        {
            return false;
        }

        RevivePlayer();
        return true;
    }

    public bool ReviveWithAd(Action onRevived)
    {
        if (!m_isGameOver || m_rewardedAd == null)
        {
            return false;
        }

        return m_rewardedAd.ShowRewardedAd(() =>
        {
            RevivePlayer();
            onRevived?.Invoke();
        });
    }

    private void HandlePlayerDied()
    {
        if (m_isGameOver)
        {
            return;
        }

        m_isGameOver = true;
        PauseGame();
        GameOver?.Invoke();
        m_interstitialAd?.ShowInterstitialAd();
    }

    private void RevivePlayer()
    {
        m_playerLose.Respawn();
        m_isGameOver = false;
        ResumeGame();
    }
}
