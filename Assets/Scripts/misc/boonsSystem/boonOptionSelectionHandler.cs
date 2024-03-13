using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class boonOptionSelectionHandler : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler , ISelectHandler , IDeselectHandler
{
    [SerializeField] private GameObject scaleTarget;
    [Range(0, 2f), SerializeField] private float scaleAmount;
    [SerializeField] private Image defaultImage;

    private Vector3 startScale;

    private void Start()
    {
        startScale = scaleTarget.transform.localScale;
    }

    private IEnumerator SelectCard(bool startingAnim)
    {
        Vector3 endScale;
        float startAlpha;
        float endAlpha;

        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;

            if (startingAnim)
            {
                endScale = startScale * scaleAmount;
                startAlpha = defaultImage.color.a;
                endAlpha = 0f;
            }
            else
            {
                endScale = startScale;
                startAlpha = defaultImage.color.a;
                endAlpha = 1f;
            }

            Vector3 lerpedScale = Vector3.Lerp(scaleTarget.transform.localScale, endScale, (elapsedTime/0.1f));
            float lerpedAlpha = Mathf.Lerp(startAlpha, endAlpha, (elapsedTime / 0.1f));

            scaleTarget.transform.localScale = lerpedScale;
            defaultImage.color = new Color(1, 1, 1, lerpedAlpha);

            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(SelectCard(true));
        boonsSelectManager.instance.lastSelected = gameObject;

        for (int i = 0; i < boonsSelectManager.instance.options.Length;  i++)
        {
            boonsSelectManager.instance.lastSelectedIndex = i;
            return;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(SelectCard(false));
    }
}
