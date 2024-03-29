using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAttack : MonoBehaviour
{
    #region PARAMETERS
    [Header("ATTACK")]
    [SerializeField] private float knockbackAmount;
    [SerializeField] private float inputBufferTime;

    [Header("REFERENCES")]
    [SerializeField] private GameObject FattackPivot;
    [SerializeField] private Animator FattackAnimator;
    [SerializeField] private Transform FattackTransform;
    [SerializeField] private Vector2 FattackSize;

    [SerializeField] private GameObject UattackPivot;
    [SerializeField] private Animator UattackAnimator;
    [SerializeField] private Transform UattackTransform;
    [SerializeField] private Vector2 UattackSize;

    [SerializeField] private GameObject DattackPivot;
    [SerializeField] private Animator DattackAnimator;
    [SerializeField] private Transform DattackTransform;
    [SerializeField] private Vector2 DattackSize;

    [SerializeField] private LayerMask attackableLayer;

    [SerializeField] private GameObject pf_playerAttackFX;

    private playerController PlayerController;
    private Rigidbody2D rb;

    [Header("KNOCKBACK")]
    [SerializeField] private float knockbackTime;
    private int knockbackDirection;
    [SerializeField] private float upwardForceTime;
    #endregion

    #region COOLDOWN
    public bool attackAvailable;
    public float attackCooldown;
    private float lastAttackInputTime;
    private bool flipAttack;
    #endregion

    private RaycastHit2D[] hits;

    private void Awake()
    {
        PlayerController = GetComponent<playerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        #region ATTACK INPUT
        lastAttackInputTime -= Time.deltaTime;

        //WHEN PRESSED ATTACK
        if (gameManager.instance.playerEnabled && !PlayerController.isBeingKnockbacked &&
            Input.GetKeyDown(KeyCode.RightShift))
        {
            lastAttackInputTime = inputBufferTime;
        }

        //EXECUTE ATTACK
        if (CanAttack() && lastAttackInputTime > 0)
        {
            PlayerController.isWallJumping = true;
            PlayerController.isJumping = false;

            //RECALCULATE RANGE
            FattackPivot.transform.localScale = new Vector3(1f * gameManager.instance.currentRange, 1, 1);
            FattackSize.x = (3.13f * gameManager.instance.currentRange);
            UattackPivot.transform.localScale = new Vector3(1, 1f * gameManager.instance.currentRange, 1);
            UattackSize.y = (2.8f * gameManager.instance.currentRange);
            DattackPivot.transform.localScale = new Vector3(1, 1f * (1.2f* gameManager.instance.currentRange), 1);
            UattackSize.y = (2.52f * (1.2f * gameManager.instance.currentRange));

            //UPWARD ATTACK
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                Attack(UattackTransform, UattackSize, 1);
            }
            //DOWNWARD ATTACK
            else if(!PlayerController.isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
            {
                Attack(DattackTransform, DattackSize, 2);
            }
            //FORWARD ATTACK
            else
            {
                Attack(FattackTransform, FattackSize, 0);
            }
        }
        #endregion
    }

    #region ATTACK METHOD
    void Attack(Transform hitPos, Vector2 hitSize, int hitType)
    {
        flipAttack = !flipAttack;
        attackAvailable = false;
        StartCoroutine(nameof(RefillAttack));

        //ANIMATION
        if (hitType == 1)
        {
            UattackAnimator.Play("Base Layer.player_UAttack");
            if (PlayerController.moveInput.x != 0)
            {
                PlayerController.ChangeAnimationState("player_uattackR");
            }
            else
            {
                PlayerController.ChangeAnimationState("player_uattackI");
            }

        }
        else if (hitType == 2)
        {
            DattackAnimator.Play("Base Layer.player_DAttack");
            PlayerController.ChangeAnimationState("player_dattack");
        }
        else
        {
            if (flipAttack)
            {
                FattackAnimator.Play("Base Layer.player_FAttack1");
            }
            else
            {
                FattackAnimator.Play("Base Layer.player_FAttack2");
            }
        }

        //CALCULATE DIRECTION OF FORCE AND RECOIL
        Vector2 oppFacing = PlayerController.isFacingRight ? Vector2.left : Vector2.right;
        Vector2 dir;
        float duration;
        Vector2 knockbackForce = new(0, 0);

        if (hitType == 1)
        {
            dir = new Vector2(0, 0);
            duration = 0;
            knockbackForce = new(0, knockbackAmount);
        }
        else if (hitType == 2)
        {
            dir = Vector2.up;
            duration = upwardForceTime;
            PlayerController.extraJumpsLeft = 1;
            knockbackForce = new(0, -knockbackAmount);
        }
        else
        {
            dir = oppFacing;
            duration = knockbackTime;
            knockbackForce = new(knockbackAmount, 0);
        }

        //DETECT TARGET(S) HIT
        hits = Physics2D.BoxCastAll(hitPos.position, hitSize, 0, Vector2.up, 0, attackableLayer);

        //PER HIT
        for (int i = 0; i < hits.Length; i++)
        {
            //CALCULATE DAMAGE AND KNOCKBACK
            IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<healthComponent>();
            if (iDamageable != null)
            {
                Vector2 direction = (hits[i].collider.gameObject.transform.position - transform.position).normalized;
                Vector2 knockback = direction * knockbackForce;
                iDamageable.OnHit(gameManager.instance.currentAttack, knockback);
            }
        }

        //IF HIT VALID, THEN RECOIL
        if (hits.Length > 0)
        {
            audioManager.instance.Play("playerAttackHit");
            StartCoroutine(PlayerController.PlayerRecoil(dir, duration));
        }
        else
        {
            audioManager.instance.Play("playerAttack");
        }

    }
    #endregion

    #region ATTACK COOLDOWN
    private IEnumerator RefillAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        attackAvailable = true;
    }
    #endregion

    #region CHECK METHODS
    private bool CanAttack()
    {
        return attackAvailable && !PlayerController.isDashAttacking;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(FattackTransform.position, FattackSize);
        Gizmos.DrawWireCube(UattackTransform.position, UattackSize);
        Gizmos.DrawWireCube(DattackTransform.position, DattackSize);
    }
    #endregion
}
