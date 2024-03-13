using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FlyingEnemy : MonoBehaviour
{
    public float speed;
    private GameObject player;
    public bool chase = false;
       
    public float biteRange;
    public float biteCooldown;
    
    private Animator anim;
    private bool isBiting = false;
    private bool isAttacking;
    private float lastBiteTime = 0f;
    private Rigidbody2D rb;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
   

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }

        Chase();

        bool isCloseToGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (!isBiting && Vector2.Distance(transform.position, player.transform.position) <= biteRange)
        {
            Bite();
        }

        Flip();
    }

    
    private void Chase()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, direction.y * speed);
        //rb.AddForce(direction * speed, ForceMode2D.Force);
    }

    private void Bite()
    {
        if (Time.time - lastBiteTime >= biteCooldown)
        {
            anim.SetTrigger("IsAttacking");
            lastBiteTime = Time.time;
        }
    }

    private void Flip()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(7, 7, 1);
        }

        else
        {
            transform.localScale = new Vector3(-7, 7, 1);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
