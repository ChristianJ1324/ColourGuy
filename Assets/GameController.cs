using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody2D m_playerBody;

    private void Awake()
    {
        Time.timeScale = 1f;
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
    }

    public void StartGame()
    {
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
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
        Time.timeScale = 0f;
    }
}
