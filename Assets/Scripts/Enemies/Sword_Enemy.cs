using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Sword_Enemy : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    private float cooldownTracker;
    [SerializeField] private float extraDamage;
    [SerializeField] private float moveSpeed; 
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform hitTransform;
    [SerializeField] private Vector2 hitSize;
    private bool isFacingRight = true;
    private float flipCooldown;
    private bool isAttacking;
    private string currentState;

    private Animator anim;
    private enemyDamage enemyDamage;
    [SerializeField] private GameObject player;
    private Rigidbody2D rb;
    public bool isPlayerInSight = false;


    const string idle = "SwordIdle";
    const string run = "SwordRun";
    const string attack = "SwordAttack";
            
    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyDamage = GetComponent<enemyDamage>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
    }


    private void Update()
    {
        if (Physics2D.OverlapBox(hitTransform.position, hitSize, 0, playerLayer))
        {
            isPlayerInSight = true;
        }

        else
        {
            isPlayerInSight = false;
        }

        if (rb.velocity.x != 0 && !isAttacking)
        {
            ChangeAnimationState(run);

        }

        else if (rb.velocity.x == 0 && !isAttacking)
        {
            ChangeAnimationState(idle);
        }

      
    }

      private IEnumerator Attack()
    {
        isAttacking = true;
        ChangeAnimationState(attack);
        yield return new WaitForSeconds(4f);
    }

    private void notAttacking() { isAttacking = false; }

    private void FixedUpdate()
    {
        if (flipCooldown >= 0)
        {
            flipCooldown -= Time.deltaTime;
        }

        if (!isPlayerInSight && !isAttacking)
        {
            if (transform.position.x < player.transform.position.x && isFacingRight)
            {
                MoveTowardsPlayer(1);
            }
            else if (transform.position.x > player.transform.position.x && !isFacingRight)
            {
                MoveTowardsPlayer(-1);
            }
            else
            {
                MoveTowardsPlayer(0);
            }

            if (transform.position.x < player.transform.position.x && !isFacingRight)
            {
                if (flipCooldown <= 0)
                {
                    isFacingRight = true;
                    flipCooldown = 1f;
                    Flip();
                }
                
            }
            else if (transform.position.x > player.transform.position.x && isFacingRight)
            {
                if (flipCooldown <= 0)
                {
                    isFacingRight = false;
                    flipCooldown = 1f;
                    Flip();
                }
            }
        }
        else if (isPlayerInSight && !isAttacking && cooldownTracker <= 0) 
        {
            cooldownTracker = attackCooldown;
            StartCoroutine(Attack());
        }

        if (cooldownTracker >= 0)
        {
            cooldownTracker -= Time.deltaTime;
        }
    }

   private void Flip()
   {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
   }

    private void MoveTowardsPlayer(float direction)
    {
        float targetSpeed = direction * moveSpeed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, 1);

        float accelRate = ((1 / Time.fixedDeltaTime) * moveSpeed) / moveSpeed;

        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    public void WindUp()
    {
        audioManager.instance.Play("windUp");
    }

    public void Slash()
    {
        audioManager.instance.Play("swordSlash");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hitTransform.position, hitSize);
    }

    private void Damage()
    {
        if (isPlayerInSight && !gameManager.instance.isInvincible)
        {
            enemyDamage.DamagePlayer(0);
        }
    }


    private void ChangeAnimationState(string newState)
    {
        //prevent self interrruption 
        if(currentState == newState) return;

        //play and then reassign 
        anim.Play(newState);
        currentState = newState;
    }
}
