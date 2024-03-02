using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    //COMPONENTS, VARIABLES, & PARAMETERS
    #region PLAYER
    [Header("PLAYER")]
    public Animator animator;
    private string currentState;
    private Rigidbody2D rb;

    [HideInInspector] public Vector2 moveInput;
    public bool isFacingRight;

    [SerializeField] private float speed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float inputBufferTime;

    [SerializeField] private GameObject cameraFollowGO;
    private cameraFollowObject CameraFollowObject;

    [SerializeField] private GameObject dustGO;
    private ParticleSystem dust;
    [SerializeField] private ParticleSystem dustBurst;
    [SerializeField] private ParticleSystem dustLand;

    public bool isAttacking;

    const string playerIdle = "player_idle";
    const string playerRun = "player_run";
    const string playerStop = "player_stop";
    const string playerJump = "player_jump";
    const string playerFall = "player_fall";
    const string playerWall = "player_wall";
    const string playerStand = "player_stand";
    const string playerDatk = "player_dattack";
    const string playerUatkR = "player_uattackR";
    const string playerUatkI = "player_uattackI";
    #endregion

    #region JUMP
    [Header("JUMP")]
    public bool isJumping;
    private bool isJumpCut;

    private float lastJumpInputTime;

    public int extraJumpsLeft;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTimeToApex;
    private float jumpForce;
    #endregion

    #region WALLSLIDE/JUMP
    [Header("WALLSLIDE/JUMP")]
    public bool isWallJumping;
    private float wallJumpStartTime;
    private int lastWallJumpDir;
    public bool isWallSliding;

    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private float wallJumpRunLerp;
    [SerializeField] private float wallJumpTime;
    #endregion

    #region DASH
    [Header("DASH")]
    public bool isDashing;
    [HideInInspector] public bool isDashAttacking;

    private float lastDashInputTime;
    private Vector2 lastDashDir;

    public float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashAttackTime;
    [SerializeField] private float dashEndTime;
    [SerializeField] private Vector2 dashEndSpeed;
    [SerializeField] private float dashEndRunLerp;
    #endregion

    #region KNOCKBACK
    [Header("KNOCKBACK")]
    public bool isBeingKnockbacked;
    [SerializeField] private float knockbackForce;
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


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraFollowGO = GameObject.Find("CameraFollowObject");
        CameraFollowObject = cameraFollowGO.GetComponent<cameraFollowObject>();
        dust = dustGO.GetComponent<ParticleSystem>();

        #region CALCULATE JUMP
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        gravityScale = gravityStrength / Physics2D.gravity.y;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;
        #endregion
    }

    void FixedUpdate()
    {
        #region MOVEMENT INPUT
        if (gameManager.instance.playerEnabled)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            moveInput.x = 0;
        }

        if (moveInput.x != 0)
        {
            CheckDirectionToFace(moveInput.x > 0);
            if (!isJumping && !isWallJumping && !isDashing && isGrounded && !isAttacking)
            {
                ChangeAnimationState(playerRun);
            }

            if (isGrounded)
            {
                //CREATE DUST IF MOVING ON GROUND
                CreateDust(0.02f, -0.123f);
            }
        }
        else
        {
            if (isGrounded && !isJumping && !isAttacking)
            {
                if (currentState == playerRun)
                {
                    ChangeAnimationState(playerStop);
                }
                else if (currentState != playerStop)
                {
                    ChangeAnimationState(playerIdle);
                }
            }
        }

        if (!isDashing && !isBeingKnockbacked)
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
        else if (!isDashAttacking && !isBeingKnockbacked)
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
                extraJumpsLeft = 1;
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
            if (!isJumping && moveInput.x != 0)
            {
                extraJumpsLeft = 1;
            }
        }

        //LEFT WALL CHECK
        if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, groundCheckLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, groundCheckLayer) && isFacingRight)) && !isWallJumping)
        {
            lastOnWallLeft = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping && moveInput.x != 0)
            {
                extraJumpsLeft = 1;
            }
        }

        lastOnWall = Mathf.Max(lastOnWallLeft, lastOnWallRight);
        #endregion

        #region JUMP INPUT
        lastJumpInputTime -= Time.deltaTime;

        //WHEN PRESSED JUMP
        if (gameManager.instance.playerEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Space) ^ Input.GetKeyDown(KeyCode.C))
            {
                lastJumpInputTime = inputBufferTime;
            }
        }

        //WHEN RELEASED JUMP
        if (Input.GetKeyUp(KeyCode.Space) ^ Input.GetKeyUp(KeyCode.C))
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

                //CREATE DUST IF JUMPING OFF GROUND
                CreateDust(0.02f, -0.123f);
                CreateDustBurst();

                Jump();
            }
            else if (CanWallJump() && lastJumpInputTime > 0)
            {
                isWallJumping = true;
                isJumping = false;
                isJumpCut = false;

                wallJumpStartTime = Time.time;
                lastWallJumpDir = (lastOnWallRight > 0) ? -1 : 1;

                //CREATE DUST IF JUMPING OFF WALL
                CreateDust(0.065f, -0.085f);

                WallJump(lastWallJumpDir);
            }
            else if (CanExtraJump() && lastJumpInputTime > 0)
            {
                isJumping = true;
                isWallJumping = false;
                isJumpCut = false;
                gameManager.instance.currentChargeValue -= 20;

                //CREATE DUST IF JUMPING OFF GROUND
                CreateDust(0.02f, -0.123f);

                Jump();
            }

            if (!isGrounded && !isJumpCut && rb.velocity.y > 0 && !isAttacking)
            {
                ChangeAnimationState(playerJump);
            }
            else if (!isGrounded && rb.velocity.y < 0 && !isWallSliding && !isAttacking)
            {
                ChangeAnimationState(playerFall);
            }
        }
        #endregion

        #region DASH INPUT
        lastDashInputTime -= Time.deltaTime;

        if (gameManager.instance.playerEnabled)
        {
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
                    gameManager.instance.currentChargeValue -= 20;
                }
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

        if (gameManager.instance.canWallClimb && !isGrounded && !isJumping && lastOnWall >= coyoteTime && moveInput.x != 0 && !isAttacking)
        {
            isWallSliding = true;
            ChangeAnimationState(playerWall);
        }
        else
        {
            isWallSliding = false;
        }
        #endregion

        #region GRAVITY
        //IF NOT DASHING
        if (!isDashAttacking)
        {
            if (isWallSliding)
            {
                //IF RUNNING INTO WALL MIDAIR, SLOW DOWN FALL
                SetGravityScale(gravityScale * fallGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
                CreateDust(0.065f, -0.085f); ;
            }
            else if (isBeingKnockbacked)
            {
                //DEFAULT GRAVITY DURING KNOCKBACK
                SetGravityScale(gravityScale);
            }
            else if (!isBeingKnockbacked && isJumpCut && rb.velocity.y > 0)
            {
                //IF RELEASED JUMP EARLY WHILE RISING
                SetGravityScale(gravityScale * jumpCutGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
            }
            else if (!isBeingKnockbacked && rb.velocity.y < 0) 
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

        #region CAMERA
        if (rb.velocity.y < -5 && !cameraManager.instance.isLerpingYDamping && !cameraManager.instance.lerpedFromPlayerFalling)
        {
            cameraManager.instance.LerpYDamping(true);
        }
        else if (rb.velocity.y >= 0 && !cameraManager.instance.isLerpingYDamping && cameraManager.instance.lerpedFromPlayerFalling)
        {
            cameraManager.instance.lerpedFromPlayerFalling = false;
            cameraManager.instance.LerpYDamping(false);
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
        CameraFollowObject.CallTurn();
    }
    #endregion

    #region ANIMATION METHOD
    public void ChangeAnimationState(string newState)
    {
        //PREVENT SELF INTERRUPTION
        if (currentState == newState) return;

        //PLAY AND REASSIGN
        animator.Play(newState);
        currentState = newState;
    }

    public void NotIsAttacking()
    {
        isAttacking = false;
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

    #region RECOIL METHOD
    public IEnumerator PlayerRecoil(Vector2 dir, float duration)
    {
        lastGrounded = 0;

        float startTime = Time.time;
        isDashing = true;

        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        while (Time.time - startTime <= duration)
        {
            rb.velocity = dir * knockbackForce;
            yield return null;
        }

        startTime = Time.time;
        isJumpCut = true;

        rb.velocity = dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= duration)
        {
            yield return null;
        }

        isDashing = false;
        isJumpCut = false;
    }
    #endregion

    #region KNOCKBACK METHOD
    public IEnumerator PlayerKnockback(Vector2 dir)
    {
        lastGrounded = 0;
        float duration = 0.4f;

        float startTime = Time.time;
        isBeingKnockbacked = true;
        isDashing = true;

        rb.velocity = new Vector2(0, 0);

        rb.AddForce(dir, ForceMode2D.Impulse);

        while (Time.time - startTime <= duration)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -5, 5), rb.velocity.y);
            yield return null;
        }

        isBeingKnockbacked = false;
        isDashing = false;
    }
    #endregion

    #region GRAVITY METHOD
    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }
    #endregion

    #region DUST METHOD
    private void OnTriggerEnter2D (Collider2D col)
    {
        if (col.gameObject.layer == 6)
        {
            var vel = Mathf.Clamp(rb.velocity.y, -3, -2);
            CreateDustLand(vel);
        }
    }

    private void CreateDust(float x, float y)
    {
        dustGO.transform.localPosition = new Vector2(x, y);
        dust.Play();
    }

    private void CreateDustBurst()
    {
        dustBurst.Play();
    }

    private void CreateDustLand(float x)
    {
        var velocity = dustLand.velocityOverLifetime;

        AnimationCurve curveMin = new AnimationCurve();
        curveMin.AddKey(0f, -x);
        curveMin.AddKey(1f, 0f);

        AnimationCurve curveMax = new AnimationCurve();
        curveMax.AddKey(0f, x);
        curveMax.AddKey(1f, 0f);

        velocity.x = new ParticleSystem.MinMaxCurve(1, curveMin, curveMax);

        dustLand.Play();
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
        return !isBeingKnockbacked && isJumping && rb.velocity.y > 0;
    }

    private bool CanExtraJump()
    {
        return gameManager.instance.canExtraJump && gameManager.instance.currentChargeValue >= 20 &&
            !isGrounded && rb.velocity.y != 0;
    }

    private bool CanWallJump()
    {
        return gameManager.instance.canWallClimb &&
            lastJumpInputTime > 0 && lastOnWall > 0.1 && lastGrounded <= 0 && (!isWallJumping ||
            (lastOnWallRight > 0 && lastWallJumpDir == 1) || (lastOnWallLeft > 0 && lastWallJumpDir == -1));
    }

    private bool CanWallJumpCut()
    {
        return isJumping && rb.velocity.y > 0;
    }

    private bool CanDash()
    {
        return gameManager.instance.canDash && gameManager.instance.currentChargeValue >= 20 &&
            !isDashing;
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
