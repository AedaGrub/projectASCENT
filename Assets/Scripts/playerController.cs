using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    //COMPONENTS, VARIABLES, & PARAMETERS
    #region PLAYER
    [Header("PLAYER")]
    public Animator animator;
    private Rigidbody2D rb;

    private Vector2 moveInput;
    public bool isFacingRight;

    [SerializeField] private float speed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float inputBufferTime;
    #endregion

    #region JUMP
    [Header("JUMP")]
    public bool isJumping;
    private bool isJumpCut;

    private float lastJumpInputTime;

    public int maxJumps;
    private int extraJumpsLeft;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTimeToApex;
    private float jumpForce;
    #endregion

    #region WALLSLIDE/JUMP
    [Header("WALLSLIDE/JUMP")]
    public bool isWallJumping;
    private float wallJumpStartTime;
    private int lastWallJumpDir;

    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private float wallJumpRunLerp;
    [SerializeField] private float wallJumpTime;
    #endregion

    #region DASH
    [Header("DASH")]
    public bool isDashing;
    private bool isDashAttacking;

    private float lastDashInputTime;
    private Vector2 lastDashDir;
    public bool dashAvailable;

    public float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashAttackTime;
    [SerializeField] private float dashEndTime;
    [SerializeField] private Vector2 dashEndSpeed;
    [SerializeField] private float dashEndRunLerp;
    #endregion

    #region COLLISION CHECKS
    [Header("COLLISION CHECKS")]
    public bool isGrounded;
    public float lastGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private LayerMask groundCheckLayer;
    
    private float lastOnWall;
    private float lastOnWallRight;
    private float lastOnWallLeft;
    [SerializeField] private Transform frontWallCheck;
    [SerializeField] private Transform backWallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    #endregion

    #region GRAVITY AND FORCES
    [Header("GRAVITY AND FORCES")]
    private float gravityStrength;
    private float gravityScale;
    [SerializeField] private float fallGravityMult;
    [SerializeField] private float jumpCutGravityMult;
    [SerializeField] private float maxFallSpeed;
    #endregion


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        #region CALCULATE JUMP
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        gravityScale = gravityStrength / Physics2D.gravity.y;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;
        #endregion
    }

    void FixedUpdate()
    {
        #region MOVEMENT INPUT
        moveInput.x = Input.GetAxisRaw("Horizontal");

        if (moveInput.x != 0)
        {
            CheckDirectionToFace(moveInput.x > 0);
            if (!isJumping && !isWallJumping && !isDashing && rb.velocity.y >= 0)
            {
                animator.SetBool("isRunning", true);
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        if (!isDashing)
        {
            if (isWallJumping)
            {
                Run(wallJumpRunLerp);
            }
            else
            {
                Run(1);
            }
        }
        else if (!isDashAttacking)
        {
            Run(dashEndRunLerp);
        }
        #endregion
    }

    void Update()
    {
        #region COLLISION CHECKS
        lastGrounded -= Time.deltaTime;
        lastOnWall -= Time.deltaTime;
        lastOnWallLeft -= Time.deltaTime;
        lastOnWallRight -= Time.deltaTime;

        //GROUND CHECK
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundCheckLayer))
        {
            isGrounded = true;
            lastGrounded = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping)
            {
                extraJumpsLeft = maxJumps;
            }
        }
        else
        {
            isGrounded = false;
        }

        //RIGHT WALL CHECK
        if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, groundCheckLayer) && isFacingRight)
            || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, groundCheckLayer) && !isFacingRight)) && !isWallJumping)
        {
            lastOnWallRight = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping)
            {
                extraJumpsLeft = maxJumps;
            }
        }

        //LEFT WALL CHECK
        if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, groundCheckLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, groundCheckLayer) && isFacingRight)) && !isWallJumping)
        {
            lastOnWallLeft = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping)
            {
                extraJumpsLeft = maxJumps;
            }
        }

        lastOnWall = Mathf.Max(lastOnWallLeft, lastOnWallRight);
        #endregion

        #region JUMP INPUT
        lastJumpInputTime -= Time.deltaTime;

        //WHEN PRESSED JUMP
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            lastJumpInputTime = inputBufferTime;
        }

        //WHEN RELEASED JUMP
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (CanJumpCut() || CanWallJumpCut())
            {
                isJumpCut = true;
            }
        }

        //EXECUTE JUMP
        if (!isDashing)
        {
            if (CanJump() && lastJumpInputTime > 0)
            {
                isJumping = true;
                isWallJumping = false;
                isJumpCut = false;

                Jump();
            }
            else if (CanWallJump() && lastJumpInputTime > 0)
            {
                isWallJumping = true;
                isJumping = false;
                isJumpCut = false;

                wallJumpStartTime = Time.time;
                lastWallJumpDir = (lastOnWallRight > 0) ? -1 : 1;

                WallJump(lastWallJumpDir);
            }
            else if (CanExtraJump() && lastJumpInputTime > 0)
            {
                isJumping = true;
                isWallJumping = false;
                isJumpCut = false;
                extraJumpsLeft--;

                Jump();
            }
        }
        #endregion

        #region DASH INPUT
        lastDashInputTime -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            lastDashInputTime = inputBufferTime;
            if (CanDash() && lastDashInputTime > 0)
            {
                if (moveInput != Vector2.zero)
                {
                    lastDashDir = moveInput;
                }
                else
                {
                    lastDashDir = isFacingRight ? Vector2.right : Vector2.left;
                }
                isDashing = true;
                isJumping = false;
                isWallJumping = false;
                isJumpCut = false;

                StartCoroutine(nameof(StartDash), lastDashDir);
                StartCoroutine(nameof(RefillDash));
            }
        }
        #endregion

        #region JUMP CHECKS
        if (isJumping && rb.velocity.y < 0)
        {
            isJumping = false;
        }

        if (isWallJumping && Time.time - wallJumpStartTime > wallJumpTime)
        {
            isWallJumping = false;
        }

        if (lastGrounded > 0 && !isJumping && !isWallJumping)
        {
            isJumpCut = false;
        }
        #endregion

        #region GRAVITY
        //IF NOT DASHING
        if (!isDashAttacking)
        {
            if (lastOnWall >= coyoteTime && moveInput.x != 0)
            {
                //IF RUNNING INTO WALL MIDAIR, SLOW DOWN FALL
                SetGravityScale(gravityScale * fallGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            }
            else if (isJumpCut && rb.velocity.y > 0)
            {
                //IF RELEASED JUMP EARLY WHILE RISING
                SetGravityScale(gravityScale * jumpCutGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
            }
            else if (rb.velocity.y < 0) 
            {
                //IF FALLING, CAP MAX FALL SPEED
                SetGravityScale(gravityScale * fallGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
            }
            else
            {
                //DEFAULT GRAVITY
                SetGravityScale(gravityScale);
            }
        }
        else
        {
            //IF DASHING, SUSPEND GRAVITY
            SetGravityScale(0);
        }
        #endregion

        #region ANIMATION
        if (rb.velocity.y == 0)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }

        if (!isGrounded && rb.velocity.y > 0)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }

        else if (!isGrounded && rb.velocity.y < 0)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
        #endregion
    }

    #region FLIP SPRITE METHOD
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight =!isFacingRight;
    }
    #endregion

    #region RUN METHOD
    private void Run(float lerpAmount)
    {
        float targetSpeed = moveInput.x * speed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

        float accelRate = ((1 / Time.fixedDeltaTime) * speed) / speed;

        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
    #endregion

    #region JUMP METHOD
    private void Jump()
    {
        lastGrounded = 0;
        lastJumpInputTime = 0;

        float force = jumpForce;
        if (rb.velocity.y < 0)
        {
            force -= rb.velocity.y;
        }
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    #endregion

    #region WALLJUMP METHOD
    private void WallJump(int dir)
    {
        lastJumpInputTime = 0;
        lastGrounded = 0;
        lastOnWallRight = 0;
        lastOnWallLeft = 0;

        Vector2 force = new Vector2(wallJumpForce.x, wallJumpForce.y);
        force.x *= dir;

        if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
        {
            force.x -= rb.velocity.x;
        }

        if (rb.velocity.y < 0)
        {
            force.y -= rb.velocity.y;
        }

        rb.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion

    #region DASH METHOD
    private IEnumerator StartDash(Vector2 dir)
    {
        lastGrounded = 0;
        lastDashInputTime = 0;

        float startTime = Time.time;
        dashAvailable = false;
        isDashAttacking = true;

        while (Time.time - startTime <= dashAttackTime)
        {
            rb.velocity = dir.normalized * dashSpeed;
            yield return null;
        }

        startTime = Time.time;
        isDashAttacking = false;

        rb.velocity = dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= dashEndTime)
        {
            yield return null;
        }

        isDashing = false;
    }
    #endregion

    #region DASH REFILL METHOD
    private IEnumerator RefillDash()
    {
        yield return new WaitForSeconds(dashCooldown);
        dashAvailable = true;
    }
    #endregion

    #region GRAVITY METHOD
    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != isFacingRight)
        {
            Flip();
        }
    }

    private bool CanJump()
    {
        return lastGrounded > 0 && !isJumping;
    }

    private bool CanJumpCut()
    {
        return isJumping && rb.velocity.y > 0;
    }

    private bool CanExtraJump()
    {
        return !isGrounded && rb.velocity.y != 0 && extraJumpsLeft > 0;
    }

    private bool CanWallJump()
    {
        return lastJumpInputTime > 0 && lastOnWall > 0 && lastGrounded <= 0 && (!isWallJumping ||
            (lastOnWallRight > 0 && lastWallJumpDir == 1) || (lastOnWallLeft > 0 && lastWallJumpDir == -1));
    }

    private bool CanWallJumpCut()
    {
        return isJumping && rb.velocity.y > 0;
    }

    private bool CanDash()
    {
        return dashAvailable;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheck.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheck.position, wallCheckSize);
    }
    #endregion
}
