using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameplayEventSystem : MonoBehaviour
{
    private const string c_menuSceneName = "Menu";

    private EventSystem m_eventSystem;
    private InputSystemUIInputModule m_inputModule;

    private void Awake()
    {
        m_eventSystem = GetComponent<EventSystem>();
        m_inputModule = GetComponent<InputSystemUIInputModule>();
        UpdateEventSystem();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        UpdateEventSystem();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        UpdateEventSystem();
    }

    private void UpdateEventSystem()
    {
        bool isEnabled = !SceneManager.GetSceneByName(c_menuSceneName).isLoaded;

        if (m_eventSystem == null || m_inputModule == null)
        {
            return;
        }

        if (isEnabled)
        {
            m_eventSystem.enabled = true;
            m_inputModule.enabled = true;
        }
        else
        {
            m_inputModule.enabled = false;
            m_eventSystem.enabled = false;
        }
    }
}
