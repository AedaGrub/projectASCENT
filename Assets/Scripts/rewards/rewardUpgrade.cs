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
        //audioManager.instance.Play("rewardSpawn");
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
                    boonsSelectManager.instance.UnlockBelt1();
                    break;
                case 1:
                    gameManager.instance.defaultHealth++;
                    gameManager.instance.ResetStats();
                    break;
                case 2:
                    gameManager.instance.canDash = true;
                    baseManager.instance.DashHint();
                    boonsSelectManager.instance.UnlockTier1();
                    break;
                case 3:
                    boonsSelectManager.instance.UnlockBelt2();
                    break;
                case 4:
                    gameManager.instance.defaultHealth++;
                    boonsSelectManager.instance.UnlockTier2();
                    gameManager.instance.ResetStats();
                    break;
                case 5:
                    gameManager.instance.canExtraJump = true;
                    baseManager.instance.DoubleJumpHint();
                    break;
                case 6:
                    boonsSelectManager.instance.UnlockBelt3();
                    break;

            }
            gameManager.instance.canUseBoons = true;
        }
    }
}
