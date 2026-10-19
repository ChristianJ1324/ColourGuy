using UnityEngine;
using UnityEngine.InputSystem;
using GoogleMobileAds.Api;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody2D m_playerBody;

    InitializationStatus m_initStatus;

    private void Awake()
    {
        m_playerInput.enabled = false;
        m_playerBody.simulated = false;

    }

    void Start()
    {
        // init ads
        MobileAds.Initialize(m_initStatus =>
        {
            print("Google ads are initialised");
        });
    }

    public void StartGame()
    {
        m_playerBody.simulated = true;
        m_playerInput.enabled = true;
    }
}