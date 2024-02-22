using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class playerAttack : MonoBehaviour
{
    #region PARAMETERS
    [Header("PLAYER")]
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private float inputBufferTime;
    private playerController PlayerController;
    private Rigidbody2D rb;

    [Header("ATTACK")]
    [SerializeField] private float damageAmount;
    [SerializeField] private Transform attackTransform;
    [SerializeField] private Vector2 attackSize;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private GameObject pf_playerAttackFX;

    [Header("KNOCKBACK")]
    [SerializeField] private Vector2 knockbackAmount;
    private int knockbackDirection;
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
        if (Input.GetMouseButtonDown(0))
        {
            lastAttackInputTime = inputBufferTime;
        }

        //EXECUTE ATTACK
        if (CanAttack() && lastAttackInputTime > 0)
        {
            PlayerController.isWallJumping = true;
            PlayerController.isJumping = false;

            if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow))
            {
                FAttack();
            }
            else
            {
                if (PlayerController.isGrounded)
                {
                    FAttack();
                }
                else
                {
                    //DAttack();
                }
            }
        }
        #endregion
    }

    #region FATTACK METHOD
    void FAttack()
    {
        flipAttack = !flipAttack;
        attackAvailable = false;
        StartCoroutine(nameof(RefillAttack));

        //DETECT TARGET(S) HIT
        hits = Physics2D.BoxCastAll(attackTransform.position, attackSize, 0, Vector2.right, 0, attackableLayer);

        //PER HIT
        for (int i = 0; i < hits.Length; i++)
        {
            //HIT VFX
            Vector3 spawnPos = new (Mathf.Clamp(hits[i].transform.position.x, 0, 10f), hits[i].transform.position.y, -5);
            Instantiate(pf_playerAttackFX, spawnPos, Quaternion.Euler(new Vector3(0, 0, Random.Range(20, -50))));

            //CALCULATE DAMAGE
            IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<healthComponent>();
            if (iDamageable != null)
            {
                iDamageable.Damage(damageAmount);
            }
        }

        //IF HIT VALID, THEN KNOCKBACK
        if (hits.Length > 0)
        {
            Vector2 dir = PlayerController.isFacingRight ? Vector2.left : Vector2.right;
            StartCoroutine(PlayerController.PlayerKnockbacked(dir, knockbackAmount));
        }

        //ANIMATION
        if (flipAttack)
        {
            attackAnimator.Play("Base Layer.player_FAttack1");
        }
        else
        {
            attackAnimator.Play("Base Layer.player_FAttack2");
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackTransform.position, attackSize);
    }
    #endregion
}
