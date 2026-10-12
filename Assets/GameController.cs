using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody2D m_playerBody;

    private void Awake()
    {
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;
    }

    public void StartGame()
    {
        m_playerBody.simulated = true;
        m_playerInput.enabled = true;
    }
}