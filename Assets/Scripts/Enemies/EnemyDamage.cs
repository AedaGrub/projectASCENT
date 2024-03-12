using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemyDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private Vector2 knockbackForce = new Vector2(6, 6);

    private GameObject player;
    private playerController PlayerController;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        PlayerController = player.gameObject.GetComponent<playerController>();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player" && !gameManager.instance.isInvincible)
        {
            DamagePlayer(0);
        }
    }

    public void DamagePlayer(float extraDamage)
    {
        //CALCULATE DAMAGE
        gameManager.instance.OnHit(damageAmount + extraDamage);

        //CALCULATE KNOCKBACK
        Vector2 direction = (player.transform.position - transform.position).normalized;
        Vector2 knockback = new(direction.x * knockbackForce.x, knockbackForce.y);

        StartCoroutine(PlayerController.PlayerKnockback(knockback));

    
    }
}
