using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private Rigidbody2D playerRigidBody;
    private Collider2D playerCollider;
    private Vector3 bottom;


    public float speed;
    public float jumpForce;
    public bool isOnGround;
    public float groundCheckDistance;
    public LayerMask groundLayer;


    private void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        bottom = new Vector3(transform.position.x, (float)(playerCollider.bounds.min.y - 0.01), transform.position.z);
        CheckForGround();
    }

    public void OnJump()//InputAction.CallbackContext context)
    {
        if (isOnGround)
        {
            Debug.Log("jump");
            playerRigidBody.linearVelocity = Vector2.up * jumpForce;
        }
    }

    private void CheckForGround()
    {
        isOnGround = Physics2D.Raycast(bottom, Vector2.down, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(bottom, bottom + new Vector3(0f, -groundCheckDistance, 0f));
    }
}
