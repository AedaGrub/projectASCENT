using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class playerAttack : MonoBehaviour
{
    #region PARAMETERS
    [SerializeField] private Animator attackAnimator;
    private playerController PlayerController;

    [SerializeField] private Transform attackTransform;
    [SerializeField] private Vector2 attackSize;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damageAmount;
    [SerializeField] private float inputBufferTime;
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
    }

    void Update()
    {
        lastAttackInputTime -= Time.deltaTime;

        //WHEN PRESSED ATTACK
        if (Input.GetMouseButtonDown(0))
        {
            lastAttackInputTime = inputBufferTime;
        }

        //EXECUTE ATTACK
        if (CanAttack() && lastAttackInputTime > 0)
        {
            Attack();
            StartCoroutine(nameof(RefillAttack));
        }
    }

    #region ATTACK METHOD
    void Attack()
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
