using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemyDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private Vector2 knockbackDir = new Vector2(2, 0.3f);
    [SerializeField] private float knockbackDuration = 0.1f;
    [SerializeField] private float knockbackForce;
    public Vector2 dir;

    private GameObject player;
    private playerController PlayerController;
    private Rigidbody2D playerrb;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        PlayerController = player.gameObject.GetComponent<playerController>();
        playerrb = player.gameObject.GetComponent<Rigidbody2D>();
        dir = knockbackDir;
    }

    private void Update()
    {
        if (transform.position.x > player.transform.position.x)
        {
            dir.x = knockbackDir.x * -1;
        }
        else if (transform.position.x == player.transform.position.x)
        {
            if (PlayerController.isFacingRight != true)
            {
                dir.x = knockbackDir.x * -1;
            }
        }
        else
        {
            dir.x = knockbackDir.x;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            //CALCULATE DAMAGE
            //IDamageable iDamageable = col.gameObject.GetComponent<playerHealth>();
            //if (iDamageable != null)
            //{
            //    iDamageable.Damage(damageAmount);
            //}

            //CALCULATE KNOCKBACK
            //StartCoroutine(PlayerController.PlayerKnockbacked(dir, knockbackDuration));
        }
    }

    private IEnumerator KnockbackArc()
    {
        Vector3 arcCenter = new Vector3(1, 0, 0);
    }
}
