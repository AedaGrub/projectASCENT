using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDamage : MonoBehaviour
{
    public int attackDamage = 2;
    public int enragedAttackDamage = 4;

    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask playerLayer;
    private enemyDamage enemyDamage;

    private void Awake()
    {
        enemyDamage = GetComponent<enemyDamage>();
    }
    public void Attack()
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, playerLayer);
        if (colInfo != null)
        {
            enemyDamage.DamagePlayer(0);
        }
    }
}
