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
    public Material material;
    public bool isAsleep;

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
    [SerializeField] private ParticleSystem playerHurt;
    [SerializeField] private ParticleSystem shieldHurt;
    [SerializeField] private ParticleSystem shieldRefresh;
    #endregion

    #region ANIMATION STRINGS
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
    const string playerSleep = "player_sleep";
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

    private bool hasPlayedLandSFX;
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
        material = GetComponent<SpriteRenderer>().material;
        cameraFollowGO = GameObject.Find("CameraFollowObject");
        CameraFollowObject = cameraFollowGO.GetComponent<cameraFollowObject>();
        dust = dustGO.GetComponent<ParticleSystem>();

        //gameManager.instance.FindReferences();

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

            if (!hasPlayedLandSFX)
            {
                hasPlayedLandSFX = true;
                audioManager.instance.Play("playerLand");
            }

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
            if (!hasPlayedLandSFX)
            {
                hasPlayedLandSFX = true;
                audioManager.instance.Play("playerLand");
            }
            lastOnWallRight = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping && moveInput.x != 0 && gameManager.instance.canWallClimb)
            {
                extraJumpsLeft = 1;
            }
        }

        //LEFT WALL CHECK
        if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, groundCheckLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, groundCheckLayer) && isFacingRight)) && !isWallJumping)
        {
            if (!hasPlayedLandSFX)
            {
                hasPlayedLandSFX = true;
                audioManager.instance.Play("playerLand");
            }
            lastOnWallLeft = coyoteTime;
            //RESET EXTRAJUMPS
            if (!isJumping && moveInput.x != 0 && gameManager.instance.canWallClimb)
            {
                extraJumpsLeft = 1;
            }
        }

        lastOnWall = Mathf.Max(lastOnWallLeft, lastOnWallRight);

        if (lastOnWall != coyoteTime && lastGrounded != coyoteTime)
        {
            hasPlayedLandSFX = false;
        }
        #endregion

        #region JUMP INPUT
        lastJumpInputTime -= Time.deltaTime;

        //WHEN PRESSED JUMP
        if (gameManager.instance.playerEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                lastJumpInputTime = inputBufferTime;
            }
        }

        //WHEN RELEASED JUMP
        if (Input.GetKeyUp(KeyCode.Space))
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
                extraJumpsLeft--;

                //CREATE DUST IF JUMPING OFF GROUND
                CreateDust(0.02f, -0.123f);

                Jump();
            }

            if (!isGrounded && !isJumpCut && rb.velocity.y > 0 && !isWallSliding && currentState != playerUatkI && currentState != playerUatkR)
            {
                ChangeAnimationState(playerJump);
            }
            else if (!isGrounded && rb.velocity.y < 0 && !isWallSliding && currentState != playerDatk && 
                currentState != playerUatkI && currentState != playerUatkR && !isAsleep && currentState != playerStand)
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
                    gameManager.instance.currentChargeValue -= 1;
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

        if (gameManager.instance.canWallClimb && !isGrounded && !isJumping && lastOnWall >= coyoteTime && moveInput.x != 0)
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

        #region HORNS
        if (gameManager.instance.canExtraJump && extraJumpsLeft > 0)
        {
            material.SetColor("_Color", new Color(10,10,10));
        }
        else
        {
            Color colorStart = material.GetColor("_Color");
            Color colorEnd = new Color(0, 0, 0);

            float elapsedTime = 0f;
            if (elapsedTime < 0.01f)
            {
                elapsedTime += Time.deltaTime;
                Color colorNow = Color.Lerp(colorStart, colorEnd, (elapsedTime / 0.01f));

                material.SetColor("_Color", colorNow);
            }
        }
        #endregion

        #region SHIELD SKIN
        if (gameManager.instance.currentShieldValue >= 1)
        {
            material.SetFloat("_SeconTexAlpha", 1);
        }
        else
        {
            material.SetFloat("_SeconTexAlpha", 0);
        }
        #endregion

        #region IDLE AND RUN ANIM
        if (moveInput.x != 0)
        {
            CheckDirectionToFace(moveInput.x > 0);
            if (!isJumping && !isWallJumping && !isDashing && isGrounded)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    ChangeAnimationState(playerUatkR);
                }
                else
                {
                    ChangeAnimationState(playerRun);
                }
            }

            if (isGrounded)
            {
                //CREATE DUST IF MOVING ON GROUND
                CreateDust(0.02f, -0.123f);
            }
        }
        else
        {
            if (isGrounded && !isJumping)
            {
                if (currentState == playerRun)
                {
                    ChangeAnimationState(playerStop);
                }
                else if (currentState != playerStop && !isAsleep)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        ChangeAnimationState(playerUatkI);
                    }
                    else
                    {
                        ChangeAnimationState(playerIdle);
                    }
                }
            }
        }
        #endregion

        #region ASLEEP ANIM
        if (currentState != playerSleep && currentState != playerStand)
        {
            isAsleep = false;
        }

        if (isAsleep && currentState != playerStand)
        {
            ChangeAnimationState(playerSleep);
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

    public void Sleep()
    {
        isAsleep = true;
        ChangeAnimationState(playerSleep);
    }

    public void StandUp()
    {
        ChangeAnimationState(playerStand);
    }

    public void ForcePlayIdle()
    {
        ChangeAnimationState(playerIdle);
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

    public void Footstep()
    {
        audioManager.instance.Play("playerFootstep");
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
        audioManager.instance.Play("playerJump");
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
        audioManager.instance.Play("playerJump");
    }
    #endregion

    #region DASH METHOD
    private IEnumerator StartDash(Vector2 dir)
    {
        lastGrounded = 0;
        lastDashInputTime = 0;
        audioManager.instance.Play("playerDash");

        float startTime = Time.time;
        isDashAttacking = true;
        gameManager.instance.IFrames();

        while (Time.time - startTime <= dashAttackTime)
        {
            rb.velocity = dir.normalized * gameManager.instance.currentDashSpeed;
            yield return null;
        }

        startTime = Time.time;
        isDashAttacking = false;

        rb.velocity = dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= dashEndTime)
        {
            yield return null;
        }

        gameManager.instance.IFrames();
        isDashing = false;
    }
    #endregion

    #region RECOIL METHOD
    public IEnumerator PlayerRecoil(Vector2 dir, float duration)
    {
        lastGrounded = 0;

        float elapsedTime = 0;
        isDashing = true;

        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            rb.velocity = dir * knockbackForce;
            yield return null;
        }

        elapsedTime = 0;
        isJumpCut = true;

        rb.velocity = dashEndSpeed * dir.normalized;

        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        isJumpCut = false;
        isBeingKnockbacked = false;
    }
    #endregion

    #region KNOCKBACK METHOD
    public IEnumerator PlayerKnockback(Vector2 dir)
    {
        lastGrounded = 0;
        float duration = 0.4f;

        float elapsedTime = 0;
        isBeingKnockbacked = true;
        isDashing = true;

        rb.velocity = new Vector2(0, 0);

        rb.AddForce(dir, ForceMode2D.Impulse);


        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
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

    #region ADDITIONAL PS METHODS
    public void PlayerHurtPS()
    {
        playerHurt.Play();
    }

    public void ShieldHurtPS()
    {
        shieldHurt.Play();
    }

    public void ShieldRefreshPS()
    {
        shieldRefresh.Play();
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
        return gameManager.instance.canExtraJump && extraJumpsLeft > 0 &&
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
        return gameManager.instance.canDash && gameManager.instance.currentChargeValue >= 1 &&
            !isDashing;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheck.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheck.position, wallCheckSize);
    }
    #endregion
}
