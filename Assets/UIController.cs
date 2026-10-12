using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject m_mainMenuPanel;
    [SerializeField] private GameObject m_settingsMenuPanel;
    [SerializeField] private GameObject m_helpMenuPanel;
    [SerializeField] private GameController m_gameController;

    public void PlayGame()
    {
        m_mainMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(false);
        m_helpMenuPanel.SetActive(false);
        m_gameController.StartGame();
    }

    public void OpenSettings()
    {
        m_mainMenuPanel.SetActive(false);
        m_settingsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        m_settingsMenuPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
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
}