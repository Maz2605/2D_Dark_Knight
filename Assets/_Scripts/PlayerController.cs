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
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAtempingToJump;
    private bool checkJumpMutiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected; 

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

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

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;

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
        CheckLedgeClimb();
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
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchingWall))
            {
                NormalJump(); 
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAtempingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection )
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)
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
            if(!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection) 
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

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(isTouchingWall && !isTouchingLedge && !ledgeDetected )
        {
            ledgeDetected = true;
            ledgePosBot = ledgeCheck.position;
        }

    }
    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y <= 0.01)
        {
            amountOfJumpsLeft = amountOfJump;
        }

        if (isTouchingWall)
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
        if(!isGrounded && isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void CheckLedgeClimb()
    {
        if(ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if(isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.y + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
              
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
               
            }

            canMove = false;
            canFlip = false;

            Debug.Log("Climb");
            anim.SetBool("canClimbLedge", canClimbLedge);
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

        Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + wallCheckDistance, ledgeCheck.position.y, 0.0f));// Tiếp xúc với mép
    }

    public void FinishClimbLedge()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        Debug.Log("Done Climb");
        anim.SetBool("canClimbLedge", canClimbLedge);
    }
}
