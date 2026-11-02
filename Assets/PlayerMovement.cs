using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private Rigidbody2D playerRigidBody;
    private Collider2D playerCollider;
    private RectTransform m_dashButtonRect;
    private PlayerInput m_playerInput;
    private InputAction m_jumpAction;
    private Vector3 bottom;
    private bool canJump;
    private bool m_isChargingJump;
    private bool m_isDashing;
    private bool m_dashButtonVisible;
    private float m_dashEndTime;
    private float m_nextDashTime;
    private float m_jumpStartTime;
    private float m_jumpStartY;

    private const float c_blockHeight = 1f;

    [SerializeField] private GameObject m_dashButton;
    [SerializeField] private RectTransform m_dashCooldownMask;
    [SerializeField, Min(0.01f)] private float m_dashDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float m_dashSpeed = 6f;
    [SerializeField, Min(0.1f)] private float m_dashCooldown = 5f;
    [SerializeField, Min(0f)] private float m_twoBlockHoldTime = 0.1f;
    [SerializeField, Min(0f)] private float m_threeBlockHoldTime = 0.25f;


    public float speed;
    public float jumpForce;
    public bool isOnGround;
    public float groundCheckDistance;
    public LayerMask groundLayer;


    private void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        m_playerInput = GetComponent<PlayerInput>();
        m_dashButtonRect = m_dashButton != null ? m_dashButton.GetComponent<RectTransform>() : null;
        if (m_dashCooldownMask != null)
        {
            m_dashCooldownMask.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckForGround();
        UpdateDashButton();
        UpdateDashCooldownVisual();

        if (m_isChargingJump)
        {
            if (!playerRigidBody.simulated)
            {
                m_isChargingJump = false;
            }
            else if (!IsJumpPressed())
            {
                FinishJump();
            }
        }

        if (m_isDashing && !playerRigidBody.simulated)
        {
            m_isDashing = false;
        }
        else if (m_isDashing && Time.time >= m_dashEndTime)
        {
            m_isDashing = false;
            playerRigidBody.linearVelocity = new Vector2(0f, playerRigidBody.linearVelocity.y);
        }
    }

    public void OnJump()//InputAction.CallbackContext context)
    {
        if (m_isChargingJump || !IsJumpPressed() || !playerRigidBody.simulated)
        {
            return;
        }

        CheckForGround();
        if (canJump)
        {
            m_isChargingJump = true;
            m_jumpStartTime = Time.time;
            m_jumpStartY = transform.position.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y) * playerRigidBody.gravityScale;
            float threeBlockVelocity = Mathf.Sqrt(2f * gravity * c_blockHeight * 3f);
            playerRigidBody.linearVelocity = Vector2.up * Mathf.Min(jumpForce, threeBlockVelocity);
        }
    }

    private void FinishJump()
    {
        float holdTime = Time.time - m_jumpStartTime;
        float targetBlocks = holdTime < m_twoBlockHoldTime ? 1f :
            holdTime < m_threeBlockHoldTime ? 2f : 3f;
        float gravity = Mathf.Abs(Physics2D.gravity.y) * playerRigidBody.gravityScale;
        float heightReached = Mathf.Max(0f, transform.position.y - m_jumpStartY);
        float heightRemaining = Mathf.Max(0f, targetBlocks * c_blockHeight - heightReached);
        float releaseVelocity = Mathf.Sqrt(2f * gravity * heightRemaining);

        if (playerRigidBody.linearVelocity.y > releaseVelocity)
        {
            playerRigidBody.linearVelocity = new Vector2(0f, releaseVelocity);
        }

        m_isChargingJump = false;
    }

    private bool IsJumpPressed()
    {
        if (m_jumpAction == null && m_playerInput != null && m_playerInput.actions != null)
        {
            m_jumpAction = m_playerInput.actions.FindAction("Jump", false);
        }

        return m_jumpAction != null && m_jumpAction.enabled && m_jumpAction.IsPressed();
    }

    public void OnDash()
    {
        if (InAppPurchaseController.Instance == null || !InAppPurchaseController.Instance.HasDash ||
            !playerRigidBody.simulated || Time.time < m_nextDashTime)
        {
            return;
        }

        m_isDashing = true;
        m_dashEndTime = Time.time + m_dashDuration;
        m_nextDashTime = Time.time + m_dashCooldown;
        playerRigidBody.linearVelocity = new Vector2(m_dashSpeed, playerRigidBody.linearVelocity.y);
        UpdateDashCooldownVisual();
    }

    private void UpdateDashButton()
    {
        if (m_dashButton == null)
        {
            return;
        }

        bool hasDash = InAppPurchaseController.Instance != null && InAppPurchaseController.Instance.HasDash;
        if (m_dashButtonVisible != hasDash)
        {
            m_dashButtonVisible = hasDash;
            m_dashButton.SetActive(hasDash);
        }
    }

    private void UpdateDashCooldownVisual()
    {
        if (m_dashCooldownMask == null || m_dashButtonRect == null)
        {
            return;
        }

        float remaining = Mathf.Max(0f, m_nextDashTime - Time.time);
        float progress = m_dashCooldown > 0f ? Mathf.Clamp01(remaining / m_dashCooldown) : 0f;
        m_dashCooldownMask.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            m_dashButtonRect.rect.width * progress);
        m_dashCooldownMask.gameObject.SetActive(progress > 0f);
    }

    private void CheckForGround()
    {
        Bounds bounds = playerCollider.bounds;
        bottom = new Vector3(bounds.center.x, bounds.min.y - 0.01f, transform.position.z);
        float inset = Mathf.Min(0.05f, bounds.extents.x * 0.5f);

        isOnGround = Physics2D.Raycast(bottom, Vector2.down, groundCheckDistance, groundLayer) ||
                     Physics2D.Raycast(bottom + Vector3.left * (bounds.extents.x - inset), Vector2.down, groundCheckDistance, groundLayer) ||
                     Physics2D.Raycast(bottom + Vector3.right * (bounds.extents.x - inset), Vector2.down, groundCheckDistance, groundLayer);
        canJump = isOnGround;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(bottom, bottom + new Vector3(0f, -groundCheckDistance, 0f));
    }
}
