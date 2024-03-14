using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardUpgrade : MonoBehaviour
{
    private bool hasCollided;
    [SerializeField] private int index;
    [SerializeField] private Rigidbody2D rb;

    private void Start()
    {
        audioManager.instance.Play("rewardSpawn");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name == "Player" && !hasCollided)
        {
            hasCollided = true;
            audioManager.instance.Play("rewardCollected");
            Destroy(gameObject);
            switch (index)
            {
                case 0:
                    boonsSelectManager.instance.UnlockBelt0();
                    break;
                case 1:
                    gameManager.instance.canDash = true;
                    boonsSelectManager.instance.UnlockTier1();
                    break;
                case 2:
                    boonsSelectManager.instance.UnlockBelt1();
                    break;
                case 3:
                    gameManager.instance.canExtraJump = true;
                    boonsSelectManager.instance.UnlockTier2();
                    break;
                case 4:
                    boonsSelectManager.instance.UnlockBelt1();
                    break;

            }
            gameManager.instance.canUseBoons = true;
        }
    }
}
