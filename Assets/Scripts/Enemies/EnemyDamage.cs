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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            //CALCULATE DAMAGE
            gameManager.instance.OnHit(damageAmount);

            //CALCULATE KNOCKBACK
            Vector2 direction = (col.transform.position - transform.position).normalized;
            Vector2 knockback = new(direction.x * knockbackForce.x, knockbackForce.y);

            StartCoroutine(PlayerController.PlayerKnockback(knockback));
        }
    }
}
