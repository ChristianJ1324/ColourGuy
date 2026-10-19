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
    [SerializeField] private GameObject m_pauseButton;
    [SerializeField] private GameObject m_muteButton;
    [SerializeField] private Text m_muteButtonLabel;

    private GameController m_gameController;
    private GameObject m_settingsReturnPanel;
    private bool m_isMuted;
    private bool m_isLoadingGameScene;
    private bool m_startGameWhenLoaded;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        m_isMuted = AudioListener.pause;

        if (m_muteButtonLabel != null)
        {
            m_muteButtonLabel.text = m_isMuted ? "Unmute" : "Mute";
        }

        SetPanelActive(m_pauseButton, false);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseMenuPanel, false);
    }

    private void Start()
    {
        LoadGameScene();
    }

    public void PlayGame()
    {
        m_mainMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(false);
        m_helpMenuPanel.SetActive(false);
        SetPanelActive(m_storeMenuPanel, false);
        SetPanelActive(m_pauseMenuPanel, false);
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
        m_storeMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
    }

    public void OpenPauseMenu()
    {
        if (m_pauseMenuPanel == null)
        {
            return;
        }

        m_pauseMenuPanel.SetActive(true);
        SetPanelActive(m_pauseButton, false);
        SetPanelActive(m_muteButton, true);
        if (m_gameController != null)
        {
            m_gameController.PauseGame();
        }
    }

    public void ResumeGame()
    {
        m_pauseMenuPanel.SetActive(false);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseButton, true);
        if (m_gameController != null)
        {
            m_gameController.ResumeGame();
        }
    }

    public void ReturnToMainMenu()
    {
        m_pauseMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
        SetPanelActive(m_muteButton, false);
        SetPanelActive(m_pauseButton, false);
        if (m_gameController != null)
        {
            m_gameController.ReturnToMenu();
        }

        SceneManager.SetActiveScene(gameObject.scene);
    }

    public void ToggleMute()
    {
        m_isMuted = !m_isMuted;
        AudioListener.pause = m_isMuted;
        m_muteButtonLabel.text = m_isMuted ? "Unmute" : "Mute";
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
        m_gameController = FindFirstObjectByType<GameController>();
        SceneManager.SetActiveScene(gameObject.scene);

        if (m_startGameWhenLoaded && m_gameController != null)
        {
            m_startGameWhenLoaded = false;
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
}
