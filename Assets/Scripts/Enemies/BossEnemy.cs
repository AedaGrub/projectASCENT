using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BossEnemy : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;


    public bool isFlipped = false;

    public void LookAtPlayer()
    {
        if (rb.position.x > player.position.x && isFlipped)
        {
            Flip();
        }
        else if (rb.position.x < player.position.x && !isFlipped)
        {
            Flip();
        }
       
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        isFlipped = !isFlipped;
    }
}
