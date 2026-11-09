using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const string c_gameSceneName = "SampleScene";

    [SerializeField] private GameObject m_mainMenuPanel;
    [SerializeField] private GameObject m_settingsMenuPanel;
    [SerializeField] private GameObject m_helpMenuPanel;
    [SerializeField] private GameObject m_storeMenuPanel;
    [SerializeField] private GameObject m_pauseMenuPanel;
    [SerializeField] private GameObject m_gameOverPanel;
    [SerializeField] private GameObject m_pauseButton;
    [SerializeField] private Text m_pauseButtonLabel;
    [SerializeField] private Text m_muteButtonLabel;
    [SerializeField, Min(1)] private int m_reviveCoinCost = 100;
    [SerializeField] private Button m_reviveWithCoinsButton;
    [SerializeField] private Text m_distanceLabel;
    [SerializeField] private GameObject m_distancePanel;
    [SerializeField] private Text m_gameOverStatsLabel;

    private GameController m_gameController;
    private Button m_privacyOptionsButton;
    private Button m_reviveWithAdButton;
    private Text m_reviveWithAdButtonLabel;
    private Text m_pauseTitleLabel;
    private Text m_resumeButtonLabel;
    private Text m_menuButtonLabel;
    private Text m_reviveCountdownLabel;
    private GameObject m_pauseSettingsButton;
    private RectTransform m_resumeButtonRect;
    private RectTransform m_menuButtonRect;
    private GameObject m_settingsReturnPanel;
    private Vector2 m_resumeButtonPosition;
    private Vector2 m_menuButtonPosition;
    private string m_pauseTitleText;
    private string m_resumeButtonText;
    private string m_menuButtonText;
    private int m_lastDistanceShown = -1;
    private bool m_isMuted;
    private bool m_isGamePaused;
    private bool m_isConfirmingQuit;
    private bool m_isLoadingGameScene;
    private bool m_startGameWhenLoaded;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AdsConsentManager.PrivacyOptionsStatusChanged += RefreshPrivacyOptionsButton;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        AdsConsentManager.PrivacyOptionsStatusChanged -= RefreshPrivacyOptionsButton;
        if (m_gameController != null)
        {
            m_gameController.GameOver -= ShowGameOver;
            m_gameController.ReviveCountdownChanged -= ShowReviveCountdown;
            m_gameController.ReviveCountdownFinished -= FinishReviveCountdown;
        }
    }

    private void Awake()
    {
        m_isMuted = AudioListener.pause;
        SetPauseButtonIcon(false);
        CachePauseMenuControls();
        CacheReviveControls();

        if (m_muteButtonLabel != null)
        {
            m_muteButtonLabel.text = m_isMuted ? "Unmute" : "Mute";
        }

        SetPanelActive(m_pauseButton, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_gameOverPanel, false);
    }

    private void Start()
    {
        CreateClearDataButton();
        LoadGameScene();
        RefreshPrivacyOptionsButton();
    }

    private void Update()
    {
        UpdateDistanceLabel();
    }

    public void PlayGame()
    {
        ResetQuitConfirmation();
        SetPanelActive(m_reviveCountdownLabel != null ? m_reviveCountdownLabel.gameObject : null, false);
        m_isGamePaused = false;
        SetPauseButtonIcon(false);
        m_mainMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(false);
        m_helpMenuPanel.SetActive(false);
        SetPanelActive(m_storeMenuPanel, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_gameOverPanel, false);

        if (m_gameController != null)
        {
            m_gameController.StartGame();
            SetPanelActive(m_pauseButton, true);
        }
        else
        {
            m_startGameWhenLoaded = true;
            LoadGameScene();
        }
    }

    public void OpenSettings()
    {
        RefreshPrivacyOptionsButton();
        m_settingsReturnPanel = m_pauseMenuPanel != null && m_pauseMenuPanel.activeSelf
            ? m_pauseMenuPanel
            : m_mainMenuPanel;
        m_mainMenuPanel.SetActive(false);
        SetPanelActive(m_pauseMenuPanel, false);
        m_settingsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        m_settingsMenuPanel.SetActive(false);
        m_settingsReturnPanel.SetActive(true);
    }

    public void OpenHelp()
    {
        m_mainMenuPanel.SetActive(false);
        m_helpMenuPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        m_helpMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
    }

    public void OpenStore()
    {
        m_mainMenuPanel.SetActive(false);
        m_storeMenuPanel.SetActive(true);
    }

    public void CloseStore()
    {
        if (InAppPurchaseController.Instance != null && InAppPurchaseController.Instance.IsPurchaseInProgress)
        {
            return;
        }

        m_storeMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
    }

    public void OpenPauseMenu()
    {
        if (m_pauseMenuPanel == null)
        {
            return;
        }

        ResetQuitConfirmation();
        m_isGamePaused = true;
        m_pauseMenuPanel.SetActive(true);
        SetPauseButtonIcon(true);
        SetPanelActive(m_pauseButton, true);
        if (m_gameController != null)
        {
            m_gameController.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (m_isConfirmingQuit)
        {
            CancelQuitConfirmation();
            return;
        }

        m_isGamePaused = false;
        m_pauseMenuPanel.SetActive(false);
        SetPanelActive(m_settingsMenuPanel, false);
        SetPanelActive(m_pauseButton, true);
        SetPauseButtonIcon(false);
        if (m_gameController != null)
        {
            m_gameController.ResumeGame();
        }
    }

    public void ReturnToMainMenu()
    {
        if (m_isConfirmingQuit)
        {
            CompleteReturnToMainMenu();
            return;
        }

        if (m_isGamePaused && m_pauseMenuPanel != null && m_pauseMenuPanel.activeSelf)
        {
            ShowQuitConfirmation();
            return;
        }

        CompleteReturnToMainMenu();
    }

    private void CompleteReturnToMainMenu()
    {
        bool returningFromRun = m_isGamePaused ||
            (m_gameOverPanel != null && m_gameOverPanel.activeSelf);
        ResetQuitConfirmation();
        m_isGamePaused = false;
        m_pauseMenuPanel.SetActive(false);
        SetPanelActive(m_gameOverPanel, false);
        m_settingsMenuPanel.SetActive(false);
        SetPanelActive(m_reviveCountdownLabel != null ? m_reviveCountdownLabel.gameObject : null, false);
        m_mainMenuPanel.SetActive(true);
        SetPanelActive(m_pauseButton, false);
        SetPauseButtonIcon(false);
        if (m_gameController != null)
        {
            m_gameController.ReturnToMenu(returningFromRun);
        }

        SceneManager.SetActiveScene(gameObject.scene);
    }

    public void ReviveWithCoins()
    {
        if (m_gameController != null && m_gameController.ReviveWithCoins(m_reviveCoinCost))
        {
            CloseGameOverPanel();
        }
    }

    public void ReviveWithAd()
    {
        SetReviveAdButtonState(true, "Loading Ad...");
        if (m_gameController == null || !m_gameController.ReviveWithAd(CloseGameOverPanel, HandleReviveAdUnavailable))
        {
            HandleReviveAdUnavailable();
        }
    }

    public void TogglePause()
    {
        if (m_isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void ToggleMute()
    {
        m_isMuted = !m_isMuted;
        AudioListener.pause = m_isMuted;
        m_muteButtonLabel.text = m_isMuted ? "Unmute" : "Mute";
    }

    private void ShowGameOver()
    {
        m_isGamePaused = false;
        SetPanelActive(m_reviveCountdownLabel != null ? m_reviveCountdownLabel.gameObject : null, false);
        SetPanelActive(m_distanceLabel != null ? m_distanceLabel.gameObject : null, false);
        SetPanelActive(m_distancePanel, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_pauseButton, false);
        SetPauseButtonIcon(false);
        SetPanelActive(m_gameOverPanel, true);

        if (m_gameOverStatsLabel != null && m_gameController != null)
        {
            string stats = $"This run: {m_gameController.RunDistanceMetres} m";
            if (m_gameController.HasHighScore)
            {
                stats += $"\nHigh score: {m_gameController.HighScoreMetres} m";
            }

            m_gameOverStatsLabel.text = stats;
        }

        if (m_reviveWithCoinsButton != null)
        {
            m_reviveWithCoinsButton.interactable = InAppPurchaseController.Instance != null &&
                InAppPurchaseController.Instance.CoinBalance >= m_reviveCoinCost;
        }

        SetReviveAdButtonState(false, "Watch Ad to Revive");
    }

    private void CloseGameOverPanel()
    {
        SetPanelActive(m_gameOverPanel, false);
        SetPanelActive(m_pauseButton, false);
        SetReviveAdButtonState(false, "Watch Ad to Revive");
    }

    private void ShowReviveCountdown(int seconds)
    {
        SetPanelActive(m_gameOverPanel, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_pauseButton, false);

        if (m_reviveCountdownLabel != null)
        {
            m_reviveCountdownLabel.text = seconds.ToString();
            m_reviveCountdownLabel.gameObject.SetActive(true);
        }
    }

    private void FinishReviveCountdown()
    {
        SetPanelActive(m_reviveCountdownLabel != null ? m_reviveCountdownLabel.gameObject : null, false);
        SetPanelActive(m_pauseButton, true);
        SetPauseButtonIcon(false);
    }

    private void HandleReviveAdUnavailable()
    {
        SetReviveAdButtonState(false, "Ad unavailable");
    }

    private void UpdateDistanceLabel()
    {
        if (m_distanceLabel == null)
        {
            return;
        }

        bool isRunActive = m_gameController != null && m_gameController.IsRunActive;
        SetPanelActive(m_distanceLabel.gameObject, isRunActive);
        SetPanelActive(m_distancePanel, isRunActive);
        if (!isRunActive)
        {
            return;
        }

        int distance = m_gameController.RunDistanceMetres;
        if (distance != m_lastDistanceShown)
        {
            m_distanceLabel.text = $"Metres: {distance}";
            m_lastDistanceShown = distance;
        }
    }

    private void SetReviveAdButtonState(bool isBusy, string label)
    {
        if (m_reviveWithAdButton != null)
        {
            m_reviveWithAdButton.interactable = !isBusy;
        }

        if (m_reviveWithAdButtonLabel != null)
        {
            m_reviveWithAdButtonLabel.text = label;
        }
    }

    private void ShowQuitConfirmation()
    {
        m_isConfirmingQuit = true;
        if (m_pauseTitleLabel != null)
        {
            m_pauseTitleLabel.text = "Are you sure you want to quit?";
        }

        if (m_resumeButtonLabel != null)
        {
            m_resumeButtonLabel.text = "No";
        }

        if (m_menuButtonLabel != null)
        {
            m_menuButtonLabel.text = "Yes";
        }

        if (m_resumeButtonRect != null)
        {
            m_resumeButtonRect.anchoredPosition = new Vector2(0f, 55f);
        }

        if (m_menuButtonRect != null)
        {
            m_menuButtonRect.anchoredPosition = new Vector2(0f, -55f);
        }

        SetPanelActive(m_pauseSettingsButton, false);
    }

    private void CancelQuitConfirmation()
    {
        ResetQuitConfirmation();
        SetPanelActive(m_pauseButton, true);
        SetPauseButtonIcon(true);
    }

    private void ResetQuitConfirmation()
    {
        m_isConfirmingQuit = false;
        if (m_pauseTitleLabel != null)
        {
            m_pauseTitleLabel.text = m_pauseTitleText;
        }

        if (m_resumeButtonLabel != null)
        {
            m_resumeButtonLabel.text = m_resumeButtonText;
        }

        if (m_menuButtonLabel != null)
        {
            m_menuButtonLabel.text = m_menuButtonText;
        }

        if (m_resumeButtonRect != null)
        {
            m_resumeButtonRect.anchoredPosition = m_resumeButtonPosition;
        }

        if (m_menuButtonRect != null)
        {
            m_menuButtonRect.anchoredPosition = m_menuButtonPosition;
        }

        SetPanelActive(m_pauseSettingsButton, true);
    }

    private void CachePauseMenuControls()
    {
        if (m_pauseMenuPanel == null)
        {
            return;
        }

        Transform pauseTitle = m_pauseMenuPanel.transform.Find("PauseTitle");
        Transform resumeButton = m_pauseMenuPanel.transform.Find("ResumeButton");
        Transform settingsButton = m_pauseMenuPanel.transform.Find("PauseSettingsButton");
        Transform menuButton = m_pauseMenuPanel.transform.Find("MenuButton");
        m_pauseTitleLabel = pauseTitle != null ? pauseTitle.GetComponent<Text>() : null;
        m_resumeButtonLabel = resumeButton != null ? resumeButton.GetComponentInChildren<Text>(true) : null;
        m_menuButtonLabel = menuButton != null ? menuButton.GetComponentInChildren<Text>(true) : null;
        m_pauseSettingsButton = settingsButton != null ? settingsButton.gameObject : null;
        m_resumeButtonRect = resumeButton as RectTransform;
        m_menuButtonRect = menuButton as RectTransform;
        m_resumeButtonPosition = m_resumeButtonRect != null ? m_resumeButtonRect.anchoredPosition : Vector2.zero;
        m_menuButtonPosition = m_menuButtonRect != null ? m_menuButtonRect.anchoredPosition : Vector2.zero;
        m_pauseTitleText = m_pauseTitleLabel != null ? m_pauseTitleLabel.text : "Paused";
        m_resumeButtonText = m_resumeButtonLabel != null ? m_resumeButtonLabel.text : "Resume";
        m_menuButtonText = m_menuButtonLabel != null ? m_menuButtonLabel.text : "Menu";
    }

    private void CacheReviveControls()
    {
        Transform adButton = m_gameOverPanel != null ? m_gameOverPanel.transform.Find("ReviveWithAdButton") : null;
        m_reviveWithAdButton = adButton != null ? adButton.GetComponent<Button>() : null;
        m_reviveWithAdButtonLabel = adButton != null ? adButton.GetComponentInChildren<Text>(true) : null;
        Transform countdown = transform.Find("ReviveCountdown");
        m_reviveCountdownLabel = countdown != null ? countdown.GetComponent<Text>() : null;
        SetPanelActive(m_reviveCountdownLabel != null ? m_reviveCountdownLabel.gameObject : null, false);
    }

    private void RefreshPrivacyOptionsButton()
    {
        // Add the privacy button when required.
        bool required = AdsConsentManager.PrivacyOptionsRequired;
        if (required && m_privacyOptionsButton == null && m_settingsMenuPanel != null)
        {
            Transform source = m_settingsMenuPanel.transform.Find("SoundEffectsButton");
            if (source != null)
            {
                GameObject privacyButtonObject = Instantiate(source.gameObject, m_settingsMenuPanel.transform, false);
                privacyButtonObject.name = "PrivacyOptionsButton";
                RectTransform privacyRect = privacyButtonObject.GetComponent<RectTransform>();
                privacyRect.anchoredPosition = new Vector2(0f, -355f);

                m_privacyOptionsButton = privacyButtonObject.GetComponent<Button>();
                m_privacyOptionsButton.onClick.RemoveAllListeners();
                m_privacyOptionsButton.onClick.AddListener(AdsConsentManager.ShowPrivacyOptions);

                Text privacyLabel = privacyButtonObject.GetComponentInChildren<Text>(true);
                if (privacyLabel != null)
                {
                    privacyLabel.text = "Privacy Options";
                }
            }
        }

        if (m_privacyOptionsButton != null)
        {
            m_privacyOptionsButton.gameObject.SetActive(required);
        }
    }

    private void CreateClearDataButton()
    {
        if (m_settingsMenuPanel == null || m_settingsMenuPanel.transform.Find("ClearDataButton") != null)
        {
            return;
        }

        Transform source = m_settingsMenuPanel.transform.Find("SoundEffectsButton");
        if (source == null)
        {
            return;
        }

        GameObject clearButtonObject = Instantiate(source.gameObject, m_settingsMenuPanel.transform, false);
        clearButtonObject.name = "ClearDataButton";
        RectTransform clearButtonRect = clearButtonObject.GetComponent<RectTransform>();
        clearButtonRect.anchoredPosition = new Vector2(0f, -150f);

        Button clearButton = clearButtonObject.GetComponent<Button>();
        clearButton.onClick = new Button.ButtonClickedEvent();
        clearButton.onClick.AddListener(ClearLocalData);

        Text clearButtonLabel = clearButtonObject.GetComponentInChildren<Text>(true);
        if (clearButtonLabel != null)
        {
            clearButtonLabel.text = "Clear Data";
        }
    }

    private void ClearLocalData()
    {
        InAppPurchaseController.ClearSavedGameData();
        GameController.ClearSavedHighScore();
        m_gameController?.ResetScoreAfterDataClear();

        DailyRewardController.ClearSavedTimeData();
    }

    private void LoadGameScene()
    {
        Scene gameScene = SceneManager.GetSceneByName(c_gameSceneName);

        if (gameScene.isLoaded)
        {
            OnSceneLoaded(gameScene, LoadSceneMode.Additive);
            return;
        }

        if (!m_isLoadingGameScene)
        {
            m_isLoadingGameScene = true;
            SceneManager.LoadSceneAsync(c_gameSceneName, LoadSceneMode.Additive);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name != c_gameSceneName)
        {
            return;
        }

        m_isLoadingGameScene = false;
        if (m_gameController != null)
        {
            m_gameController.GameOver -= ShowGameOver;
            m_gameController.ReviveCountdownChanged -= ShowReviveCountdown;
            m_gameController.ReviveCountdownFinished -= FinishReviveCountdown;
        }

        m_gameController = FindFirstObjectByType<GameController>();
        if (m_gameController != null)
        {
            m_gameController.GameOver += ShowGameOver;
            m_gameController.ReviveCountdownChanged += ShowReviveCountdown;
            m_gameController.ReviveCountdownFinished += FinishReviveCountdown;
        }

        SceneManager.SetActiveScene(gameObject.scene);

        if (m_startGameWhenLoaded && m_gameController != null)
        {
            m_startGameWhenLoaded = false;
            m_isGamePaused = false;
            SetPauseButtonIcon(false);
            m_gameController.StartGame();
            SetPanelActive(m_pauseButton, true);
        }
        else if (m_gameController != null)
        {
            m_gameController.ReturnToMenu();
        }
    }

    private static void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    private void SetPauseButtonIcon(bool isPaused)
    {
        if (m_pauseButtonLabel != null)
        {
            m_pauseButtonLabel.text = isPaused ? "▶" : "||";
        }
    }
}
