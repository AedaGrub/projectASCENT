using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLift : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite WKey;
    [SerializeField] Sprite UpKey;
    private Material material;
    public float fadeTime = 0.1f;

    private Transform liftTransform;

    public int tier0min, tier0max;
    public int tier1min, tier1max;
    public int tier2min, tier2max;
    public int sceneIndexToLoad;
    private bool canInteract = false;
    private bool interacted = false;

    void Awake()
    {
        material = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().material;
        liftTransform = transform.GetChild(0);
    }

    void OnEnable()
    {
        liftTransform.localScale = new Vector2(0f, 3f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.sprite = WKey;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            spriteRenderer.sprite = UpKey;
        }

        if (canInteract && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            OnInteract();
        }

        if (liftTransform.localScale.x < 2)
        {
            float scaleStart = liftTransform.localScale.x;
            float scaleEnd = 2f;
            float elapsedTime = 0f;
            if (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float scaleChange = Mathf.Lerp(scaleStart, scaleEnd, (elapsedTime / fadeTime));

                liftTransform.localScale = new Vector2(scaleChange, 3f);
            }
        }
        else
        {
            liftTransform.localScale = new Vector2(2f, 3f);
        }
    }

    private void OnInteract()
    {
        if (!interacted)
        {
            interacted = true;
            int randomRoom = Random.Range(tier0min, tier0max);
            levelLoader.instance.LoadNextLevel(randomRoom);
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
        float alphaStart = material.GetFloat("_Alpha");
        float alphaEnd;

        if (canInteract)
        {
            alphaEnd = 1;
        }
        else
        {
            alphaEnd = 0;
        }

        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alphaNow = Mathf.Lerp(alphaStart, alphaEnd, (elapsedTime / fadeTime));

            material.SetFloat("_Alpha", alphaNow);

            yield return null;
        }
    }
}
