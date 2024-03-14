using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boonBreakable : MonoBehaviour
{
    [SerializeField] private GameObject boonSquare;
    [SerializeField] private healthComponent hpScript;

    [SerializeField] private Sprite brokenSprite;

    private void Start()
    {
        StartCoroutine(HoverBoon());
    }

    private IEnumerator HoverBoon()
    {
        while (true)
        {
            LeanTween.moveLocal(boonSquare, new Vector3(0, 0.48f, 0), 1f).setEaseInOutSine();
            yield return new WaitForSeconds(1f);
            LeanTween.moveLocal(boonSquare, new Vector3(0, 0.40f, 0), 1f).setEaseInOutSine();
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        if (hpScript.CurrentHealth <= 99 && hpScript.CurrentHealth > 1)
        {
            GetComponent<SpriteRenderer>().sprite = brokenSprite;
            hpScript.currentHealth = 1 + gameManager.instance.currentAttack;
        }
        if (hpScript.CurrentHealth <= 1)
        {
            boonsSelectManager.instance.StartOptionSelection();
            IDamageable iDamageable = GetComponent<healthComponent>();
            if (iDamageable != null)
            {
                iDamageable.OnHit(gameManager.instance.currentAttack, new Vector2(0,0));
            }
        }
    }
}
