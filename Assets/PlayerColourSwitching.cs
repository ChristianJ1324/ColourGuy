using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlayerColor
{
    Red,
    Blue,
    Yellow,
    Green,
}

public class PlayerColourSwitching : MonoBehaviour
{

    private SpriteRenderer m_spriteRenderer;
    private PlayerInput m_playerInput;
    public Vector2 m_moveDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMove()
    {
        m_moveDirection = m_playerInput.actions["Move"].ReadValue<Vector2>();
        Debug.Log(m_moveDirection);

        if (m_moveDirection.x < 0 && m_moveDirection.y >= 0) // top left
        {
            SetColorState(PlayerColor.Red);
        }
        else if (m_moveDirection.x >= 0 && m_moveDirection.y >= 0) // top right
        {
            SetColorState(PlayerColor.Blue);
        }
        else if (m_moveDirection.x < 0 && m_moveDirection.y < 0) // bottom left
        {
            SetColorState(PlayerColor.Yellow);
        }
        else // bottom right
        {
            SetColorState(PlayerColor.Green);
        }


    }


    public void SetColorState(PlayerColor _colorState)
    {
        switch (_colorState)
        {
            case PlayerColor.Red:
                gameObject.layer = LayerMask.NameToLayer("Red");
                m_spriteRenderer.color = Color.red;
                break;
            case PlayerColor.Blue:
                gameObject.layer = LayerMask.NameToLayer("Blue");
                m_spriteRenderer.color = Color.blue;
                break;
            case PlayerColor.Yellow:
                gameObject.layer = LayerMask.NameToLayer("Yellow");
                m_spriteRenderer.color = Color.yellow;
                break;
            case PlayerColor.Green:
                gameObject.layer = LayerMask.NameToLayer("Green");
                m_spriteRenderer.color = Color.green;
                break;

        }

    }
}
