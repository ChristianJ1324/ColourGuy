using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private Image m_colourPickerKnob;
    [SerializeField, Min(0f)] private float m_rollSpeed = 360f;
    public Vector2 m_moveDirection;

    public Colour m_currentColour;
    public Collider2D m_hitbox;
    public Collider2D m_detectionbox; // checks to see if player is inside block
    private Colour m_lastStartingColour;
    private bool m_hasStartingColour;

    private int m_touchedBlocks = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_playerInput = GetComponent<PlayerInput>();
        StartNewRun();

        Debug.Log($"Sensor body: {(m_detectionbox.attachedRigidbody != null ? m_detectionbox.attachedRigidbody.name : "NONE")}");
        Debug.Log($"Hitbox body: {(m_hitbox.attachedRigidbody != null ? m_hitbox.attachedRigidbody.name : "NONE")}");
        Debug.Log($"Same body? {m_detectionbox.attachedRigidbody == m_hitbox.attachedRigidbody}");
    }

    // Update is called once per frame
    void Update()
    {
        m_spriteRenderer.transform.Rotate(0f, 0f, -m_rollSpeed * Time.deltaTime);

        // Only Change Colour when NOT inside any Blocks
        if(m_touchedBlocks == 0 )
        {
            if (m_moveDirection == Vector2.zero)
            {
                return;
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

    public void OnMove()
    {
        m_moveDirection = m_playerInput.actions["Move"].ReadValue<Vector2>();
        //Debug.Log(m_moveDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Block>(out var block))
        {
            //Debug.Log($"Touching Block: {block.m_startingColour}");
            m_touchedBlocks++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Block>(out var block))
        {
            //Debug.Log($"Left Block: {block.m_startingColour}");
            m_touchedBlocks--;
        }
    }

    public void StartNewRun()
    {
        Colour startingColour;
        do
        {
            startingColour = (Colour)Random.Range((int)Colour.Red, (int)Colour.Yellow + 1);
        }
        while (m_hasStartingColour && startingColour == m_lastStartingColour);

        m_lastStartingColour = startingColour;
        m_hasStartingColour = true;
        m_moveDirection = Vector2.zero;
        SetColorState(startingColour);
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
            case Colour.White:
            default:
                return;

        }

        m_currentColour = _colorState;

        if (m_colourPickerKnob != null)
        {
            m_colourPickerKnob.color = m_spriteRenderer.color;
        }

    }
}
