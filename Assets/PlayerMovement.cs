using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private Rigidbody2D m_playerRigidBody;
    private Collider2D m_playerCollider;
    private Vector3 m_bottom;

    private Vector2 m_startPos;

    public float m_moveSpeed;
    public float m_jumpForce;
    public bool m_isOnGround;
    public float m_groundCheckDistance;
    public LayerMask m_groundLayer;


    private void Awake()
    {
        m_playerRigidBody = GetComponent<Rigidbody2D>();
        m_playerCollider = GetComponent<Collider2D>();

        m_startPos = m_playerRigidBody.position;
    }

    // Update is called once per frame
    void Update()
    {
        m_bottom = new Vector3(transform.position.x, (float)(m_playerCollider.bounds.min.y - 0.01), transform.position.z);
        CheckForGround();


        if (m_startPos.x > m_bottom.x)
        {
            // Gradually move Right
            Vector3 position = transform.position;
            position.x += m_moveSpeed * Time.deltaTime;
            transform.position = position;
        }
    }
        

    public void OnJump()//InputAction.CallbackContext context)
    {
        if (m_isOnGround)
        {
            Debug.Log("jump");
            m_playerRigidBody.linearVelocity = Vector2.up * m_jumpForce;
        }
    }

    private void CheckForGround()
    {
        m_isOnGround = Physics2D.Raycast(m_bottom, Vector2.down, m_groundCheckDistance, m_groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(m_bottom, m_bottom + new Vector3(0f, -m_groundCheckDistance, 0f));
    }
}
