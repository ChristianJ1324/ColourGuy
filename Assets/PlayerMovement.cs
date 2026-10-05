using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody2D playerRigidBody;
    public float speed;
    public float jumpForce;
    public bool isOnGround;

 

    // Update is called once per frame
    void Update()
    {

       
    }

    public void OnJump()//InputAction.CallbackContext context)
    {

        Debug.Log("jump");
        playerRigidBody.linearVelocity = Vector2.up * jumpForce;
    }
}
