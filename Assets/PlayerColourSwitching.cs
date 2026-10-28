using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Colour
{
    White,
    Red,
    Green,
    Blue,
    Yellow,
}


public class PlayerColourSwitching : MonoBehaviour
{

    private SpriteRenderer m_spriteRenderer;
    private PlayerInput m_playerInput;
    public Vector2 m_moveDirection;

    public Colour m_currentColour;
    public Collider2D m_hitbox;

    public Collider2D m_sensor;
    public int m_touchedBlocks = 0;
    public bool m_canSwitchColour = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        m_canSwitchColour = m_touchedBlocks == 0;
        if (m_canSwitchColour) // Only allow colour change when not inside a block
        {
            if (m_moveDirection.x == 0 && m_moveDirection.y == 0)
            {
                SetColorState(Colour.White); // default
            }
            else if (m_moveDirection.x < 0 && m_moveDirection.y >= 0) // top left
            {
                SetColorState(Colour.Red);
            }
            else if (m_moveDirection.x >= 0 && m_moveDirection.y >= 0) // top right
            {
                SetColorState(Colour.Blue);
            }
            else if (m_moveDirection.x < 0 && m_moveDirection.y < 0) // bottom left
            {
                SetColorState(Colour.Yellow);
            }
            else // bottom right
            {
                SetColorState(Colour.Green);
            }
        }

    }
    void Start()
    {
        Debug.Log($"Sensor body: {(m_sensor.attachedRigidbody != null ? m_sensor.attachedRigidbody.name : "NONE")}");
        Debug.Log($"Hitbox body: {(m_hitbox.attachedRigidbody != null ? m_hitbox.attachedRigidbody.name : "NONE")}");
        Debug.Log($"Same body? {m_sensor.attachedRigidbody == m_hitbox.attachedRigidbody}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Block>(out var block))
        {
            Debug.Log($"Block Entered: {block.m_startingColour}");
            m_touchedBlocks++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Block>(out var block))
        {
            Debug.Log($"Block Exit: {block.m_startingColour}");
            m_touchedBlocks--;
        }
    }

    public void OnMove()
    {
        m_moveDirection = m_playerInput.actions["Move"].ReadValue<Vector2>();
        //Debug.Log(m_moveDirection);



    }


    public void SetColorState(Colour _colorState)
    {
        switch (_colorState)
        {
            case Colour.Red:
                m_hitbox.gameObject.layer = LayerMask.NameToLayer("Red");
                m_spriteRenderer.color = Color.red;
                break;
            case Colour.Blue:
                m_hitbox.gameObject.layer = LayerMask.NameToLayer("Blue");
                m_spriteRenderer.color = Color.blue;
                break;
            case Colour.Yellow:
                m_hitbox.gameObject.layer = LayerMask.NameToLayer("Yellow");
                m_spriteRenderer.color = Color.yellow;
                break;
            case Colour.Green:
                m_hitbox.gameObject.layer = LayerMask.NameToLayer("Green");
                m_spriteRenderer.color = Color.green;
                break;
            default:
                m_hitbox.gameObject.layer = LayerMask.NameToLayer("Default");
                m_spriteRenderer.color = Color.white;
                break;

        }

    }
}
