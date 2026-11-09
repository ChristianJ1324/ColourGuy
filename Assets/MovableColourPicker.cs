using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

// This invisible hit area lets the picker move wherever your thumb touches
public class MovableColourPicker : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform m_colourBackground;
    [SerializeField] private RectTransform m_handle;
    [SerializeField, Min(1f)] private float m_movementRange = 100f;
    [SerializeField, InputControl(layout = "Vector2")] private string m_controlPath = "<Joystick>/stick";

    private RectTransform m_pickerArea;
    private Vector2 m_backgroundHome;
    private Vector2 m_handleHome;
    private Vector2 m_touchOrigin;
    private int m_pointerId = int.MinValue;
    private bool m_isInitialized;

    protected override string controlPathInternal
    {
        get => m_controlPath;
        set => m_controlPath = value;
    }

    private void Awake()
    {
        m_pickerArea = (RectTransform)transform;
        m_backgroundHome = m_colourBackground.anchoredPosition;
        m_handleHome = m_handle.anchoredPosition;
        m_isInitialized = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_pointerId != int.MinValue)
        {
            return;
        }

        m_pointerId = eventData.pointerId;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_pickerArea, eventData.position, eventData.pressEventCamera, out m_touchOrigin);

        m_touchOrigin = ClampOrigin(m_touchOrigin);
        // The square stays here for the drag, while the knob follows your thumb
        m_colourBackground.anchoredPosition = m_touchOrigin;
        m_handle.anchoredPosition = m_touchOrigin;
        SendValueToControl(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != m_pointerId)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_pickerArea, eventData.position, eventData.pressEventCamera, out Vector2 pointerPosition);

        Vector2 direction = Vector2.ClampMagnitude(pointerPosition - m_touchOrigin, m_movementRange);
        m_handle.anchoredPosition = m_touchOrigin + direction;
        SendValueToControl(direction / m_movementRange);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != m_pointerId)
        {
            return;
        }

        ResetPicker();
    }

    protected override void OnDisable()
    {
        if (m_isInitialized)
        {
            ResetPicker();
        }

        base.OnDisable();
    }

    private Vector2 ClampOrigin(Vector2 _position)
    {
        // Keep the square on screen if the touch is right at an edge
        Vector2 halfSize = m_colourBackground.rect.size * 0.5f;
        Rect bounds = m_pickerArea.rect;
        return new Vector2(
            Mathf.Clamp(_position.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x),
            Mathf.Clamp(_position.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y));
    }

    private void ResetPicker()
    {
        // Put it back in the corner so its ready for the next touch
        m_pointerId = int.MinValue;
        m_colourBackground.anchoredPosition = m_backgroundHome;
        m_handle.anchoredPosition = m_handleHome;
        SendValueToControl(Vector2.zero);
    }
}
