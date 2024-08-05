using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Tilemaps;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private float movementInputDirection;

    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;

    public float jumpForce = 16.0f;
    public float movementSpeed = 7.0f;
    public float groundCheckRadius;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
    }

    private void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void UpdateAnimation()
    {
        anim.SetBool("isWalking", isWalking);
    }
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    private void CheckMovementDirection() 
    {
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }   
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if(rb.velocity.x != 0)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }
    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }
    //Di chuyển 
    private void ApplyMovement()
    {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
    }   
    //Lật player khi di chuyển trái phải
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    //Nhảy
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
