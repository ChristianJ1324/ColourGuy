using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    private const string c_highScoreKey = "ColourGuy.HighScoreMetres";

    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody2D m_playerBody;
    [SerializeField] private PlayerLose m_playerLose;
    [SerializeField] private AdRewarded m_rewardedAd;
    [SerializeField] private AdInterstitial m_interstitialAd;

    public event Action GameOver;
    public event Action<int> ReviveCountdownChanged;
    public event Action ReviveCountdownFinished;

    private bool m_isGameOver;
    private bool m_hasStartedRun;
    private float m_runDistanceMetres;
    private int m_highScoreMetres;
    private ModuleSpawner m_moduleSpawner;
    private Coroutine m_reviveCountdown;

    public int RunDistanceMetres => Mathf.FloorToInt(m_runDistanceMetres);
    public int HighScoreMetres => m_highScoreMetres;
    public bool HasHighScore => m_highScoreMetres > 0;
    public bool IsRunActive => m_hasStartedRun && !m_isGameOver && Time.timeScale > 0f;

    private void Awake()
    {
        Time.timeScale = 1f;
        m_playerBody.simulated = true;
        m_moduleSpawner = FindFirstObjectByType<ModuleSpawner>();
        m_highScoreMetres = PlayerPrefs.GetInt(c_highScoreKey, 0);
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

    private void Update()
    {
        // Count distance as the modules move along the conveyor.
        if (!IsRunActive)
        {
            return;
        }

        if (m_moduleSpawner == null)
        {
            m_moduleSpawner = FindFirstObjectByType<ModuleSpawner>();
        }

        if (m_moduleSpawner != null)
        {
            m_runDistanceMetres += m_moduleSpawner.CurrentMoveSpeed * Time.deltaTime;
        }
    }

    public void StartGame()
    {
        StopReviveCountdown();
        m_runDistanceMetres = 0f;
        if (m_hasStartedRun)
        {
            m_playerBody.GetComponent<PlayerColourSwitching>()?.StartNewRun();
        }
        m_hasStartedRun = true;
        m_isGameOver = false;
        ResetModules();
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

    public void ReturnToMenu(bool showInterstitial = false)
    {
        StopReviveCountdown();
        if (m_hasStartedRun)
        {
            SaveHighScore();
        }

        m_isGameOver = false;
        m_rewardedAd?.CancelPendingRewardedAd();
        ResetModules();
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
        Time.timeScale = 0f;

        if (showInterstitial && m_hasStartedRun && UnityEngine.Random.Range(0, 5) == 0)
        {
            m_interstitialAd?.ShowInterstitialAd();
        }
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

    public bool ReviveWithAd(Action onRevived, Action onUnavailable)
    {
        if (!m_isGameOver || m_rewardedAd == null)
        {
            return false;
        }

        return m_rewardedAd.ShowRewardedAd(() =>
        {
            if (!m_isGameOver)
            {
                return;
            }

            RevivePlayer();
            onRevived?.Invoke();
        }, onUnavailable);
    }

    private void HandlePlayerDied()
    {
        if (m_isGameOver)
        {
            return;
        }

        m_isGameOver = true;
        SaveHighScore();
        PauseGame();
        GameOver?.Invoke();
    }

    private void RevivePlayer()
    {
        ResetModules();
        m_playerLose.Respawn();
        m_isGameOver = false;
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
        Time.timeScale = 0f;
        m_reviveCountdown = StartCoroutine(ReviveCountdown());
    }

    private IEnumerator ReviveCountdown()
    {
        // Give the player a moment to get ready before movement starts again.
        for (int seconds = 3; seconds > 0; seconds--)
        {
            ReviveCountdownChanged?.Invoke(seconds);
            yield return new WaitForSecondsRealtime(1f);
        }

        m_reviveCountdown = null;
        ReviveCountdownFinished?.Invoke();
        ResumeGame();
    }

    private void StopReviveCountdown()
    {
        if (m_reviveCountdown != null)
        {
            StopCoroutine(m_reviveCountdown);
            m_reviveCountdown = null;
        }
    }

    private void ResetModules()
    {
        // Rebuild the module line so each run starts from the same place.
        m_moduleSpawner = FindFirstObjectByType<ModuleSpawner>();
        m_moduleSpawner?.ResetModules();
    }

    private void SaveHighScore()
    {
        int runDistanceMetres = RunDistanceMetres;
        if (runDistanceMetres <= m_highScoreMetres)
        {
            return;
        }

        m_highScoreMetres = runDistanceMetres;
        PlayerPrefs.SetInt(c_highScoreKey, m_highScoreMetres);
        PlayerPrefs.Save();
    }

}
