using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class playerAttack : MonoBehaviour
{
    #region PARAMETERS
    private Rigidbody2D rb;
    [SerializeField] private Animator attackAnimator;
    private playerController PlayerController;
    [SerializeField] private float inputBufferTime;

    [SerializeField] private Transform attackTransform;
    [SerializeField] private Vector2 attackSize;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damageAmount;
    [SerializeField] private float knockbackAmount;
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
            if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow))
            {
                FAttack();
            }
            else
            {

            }
            StartCoroutine(nameof(RefillAttack));
        }
        #endregion
    }

    #region FATTACK METHOD
    void FAttack()
    {
        attackAvailable = false;
        flipAttack = !flipAttack;

        //DETECT TARGET(S) HIT
        hits = Physics2D.BoxCastAll(attackTransform.position, attackSize, 0, Vector2.up, 0, attackableLayer);

        //CALCULATE DAMAGE PER HIT
        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<healthComponent>();

            if (iDamageable != null)
            {
                iDamageable.Damage(damageAmount);
            }
        }

        //KNOCKBACK !!NEED TO FIX!! UNABLE TO BE KNOCKED BACK WITH FORCE, BUT ABLE TO BE KNOCKED UP
        knockbackDirection = PlayerController.isFacingRight ? -1 : 1;
        Vector2 force = new Vector2(knockbackAmount, 0);
        force.x *= knockbackDirection;

        if (hits.Length > 0)
        {
            print("KNOCKBACK");
            rb.AddForce(force, ForceMode2D.Impulse);
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
