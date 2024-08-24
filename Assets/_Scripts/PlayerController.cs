using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Tilemaps;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;

    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    private bool isFacingRight = true;
    private bool isWalking ;
    private bool isGrounded;
    private bool isTouchedWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAtempingToJump;
    private bool checkJumpMutiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;

    private Rigidbody2D rb;
    private Animator anim;

    public int amountOfJump = 2;

    public float jumpForce = 16.0f;
    public float movementSpeed = 10.0f;
    public float wallSlidingSpeed = 1f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float movementForceInAir = 50f;
    public float airDragMultiplayer = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce = 10f;
    public float wallJumpForce = 30f;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float walljumpTimerSet = 0.5f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJump;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    private void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimation();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }

    private void FixedUpdate()
    {
        CheckSurroundings();
        ApplyMovement();
    }
     
    private void UpdateAnimation()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetFloat("yVelocity", rb.velocity.y);
    }
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchedWall))
            {
                NormalJump(); 
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAtempingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchedWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection )
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (!canMove)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMutiplier && !Input.GetButton("Jump"))
        {
            checkJumpMutiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        } 
    }
    private void CheckJump ()
    {
        if(jumpTimer > 0)
        {
            if(!isGrounded && isTouchedWall && movementInputDirection != 0 && movementInputDirection != facingDirection) 
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }

        if(isAtempingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }

        if(wallJumpTimer > 0)
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            } 
            else if (wallJumpTimer <= 0) 
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
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

        if(isGrounded && rb.velocity.x != 0)
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

        isTouchedWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }
    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y <= 0.01)
        {
            amountOfJumpsLeft = amountOfJump;
        }

        if (isTouchedWall)
        {
            canWallJump = true;
        }
        else
        {
            canWallJump = false;
        }

        if (amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }
    private void CheckIfWallSliding()
    {
        if(!isGrounded && isTouchedWall && movementInputDirection == facingDirection && rb.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }
    //Di chuyển 
    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplayer, rb.velocity.y);
        }
        else if (isGrounded && canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }

        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
    }   
    //Lật player khi di chuyển trái phải
    private void Flip()
    {
        if(!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }
    //Nhảy
    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAtempingToJump = false;
            checkJumpMutiplier = true;
            Debug.Log("Jump");
        }
    }
    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJump;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;  
            isAtempingToJump = false;
            checkJumpMutiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = walljumpTimerSet;
            lastWallJumpDirection = -facingDirection;
            Debug.Log("Wall Jump");
        }
    }
    //Vẽ vị trí tiếp xúc
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);//Tiếp xúc với mặt đất

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, 0.0f));// Tiếp xúc với tường
    }
}
