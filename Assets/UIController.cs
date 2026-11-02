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
    [SerializeField] private GameObject m_muteButton;
    [SerializeField] private Text m_pauseButtonLabel;
    [SerializeField] private Text m_muteButtonLabel;
    [SerializeField, Min(1)] private int m_reviveCoinCost = 100;
    [SerializeField] private Button m_reviveWithCoinsButton;

    private GameController m_gameController;
    private Button m_privacyOptionsButton;
    private GameObject m_settingsReturnPanel;
    private bool m_isMuted;
    private bool m_isGamePaused;
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
        }
    }

    private void Awake()
    {
        m_isMuted = AudioListener.pause;
        SetPauseButtonIcon(false);

        if (m_muteButtonLabel != null)
        {
            m_muteButtonLabel.text = m_isMuted ? "Unmute" : "Mute";
        }

        SetPanelActive(m_pauseButton, false);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_gameOverPanel, false);
    }

    private void Start()
    {
        LoadGameScene();
        RefreshPrivacyOptionsButton();
    }

    public void PlayGame()
    {
        m_isGamePaused = false;
        SetPauseButtonIcon(false);
        m_mainMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(false);
        m_helpMenuPanel.SetActive(false);
        SetPanelActive(m_storeMenuPanel, false);
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_gameOverPanel, false);
        SetPanelActive(m_muteButton, false);

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

        m_isGamePaused = true;
        m_pauseMenuPanel.SetActive(true);
        SetPauseButtonIcon(true);
        SetPanelActive(m_pauseButton, true);
        SetPanelActive(m_muteButton, true);
        if (m_gameController != null)
        {
            m_gameController.PauseGame();
        }
    }

    public void ResumeGame()
    {
        m_isGamePaused = false;
        m_pauseMenuPanel.SetActive(false);
        SetPanelActive(m_settingsMenuPanel, false);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseButton, true);
        SetPauseButtonIcon(false);
        if (m_gameController != null)
        {
            m_gameController.ResumeGame();
        }
    }

    public void ReturnToMainMenu()
    {
        m_isGamePaused = false;
        m_pauseMenuPanel.SetActive(false);
        SetPanelActive(m_gameOverPanel, false);
        m_settingsMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseButton, false);
        SetPauseButtonIcon(false);
        if (m_gameController != null)
        {
            m_gameController.ReturnToMenu();
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
        m_gameController?.ReviveWithAd(CloseGameOverPanel);
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
        SetPanelActive(m_pauseMenuPanel, false);
        SetPanelActive(m_pauseButton, false);
        SetPanelActive(m_muteButton, false);
        SetPauseButtonIcon(false);
        SetPanelActive(m_gameOverPanel, true);

        if (m_reviveWithCoinsButton != null)
        {
            m_reviveWithCoinsButton.interactable = InAppPurchaseController.Instance != null &&
                InAppPurchaseController.Instance.CoinBalance >= m_reviveCoinCost;
        }
    }

    private void CloseGameOverPanel()
    {
        SetPanelActive(m_gameOverPanel, false);
        SetPanelActive(m_pauseButton, true);
        SetPanelActive(m_muteButton, false);
        SetPauseButtonIcon(false);
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
                privacyRect.anchoredPosition = new Vector2(0f, -300f);

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
        }

        m_gameController = FindFirstObjectByType<GameController>();
        if (m_gameController != null)
        {
            m_gameController.GameOver += ShowGameOver;
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
