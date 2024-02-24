using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLift : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite WKey;
    [SerializeField] Sprite UpKey;
    public float fadeTime = 1f;

    public int sceneIndexToLoad;
    private bool canInteract = false;
    private bool interacted = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            spriteRenderer.sprite = WKey;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            spriteRenderer.sprite = UpKey;
        }

        if (canInteract && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            OnInteract();
        }
    }

    private void OnInteract()
    {
        if (!interacted)
        {
            interacted = true;
            levelLoader.instance.LoadNextLevel(sceneIndexToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            StartCoroutine(FadeKey());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            StartCoroutine(FadeKey());
        }
    }

    private IEnumerator FadeKey()
    {
        float startFadeAmount = spriteRenderer.color.a;
        float endFadeAmount = 0f;

        if (canInteract)
        {
            endFadeAmount = 1;
        }
        else
        {
            endFadeAmount = 0;
        }

        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float currentFadeAmount = Mathf.Lerp(startFadeAmount, endFadeAmount, (elapsedTime / fadeTime));

            Color tmp = spriteRenderer.color;
            tmp.a = currentFadeAmount;
            spriteRenderer.color = tmp;

            yield return null;
        }
    }
}
