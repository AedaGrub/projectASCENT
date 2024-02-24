using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLift : MonoBehaviour
{
    [SerializeField] private Transform interactSource;
    [SerializeField] private Vector2 interactSize;
    [SerializeField] private LayerMask playerLayer;

    public int sceneIndexToLoad;
    private bool interacted = false;


    void Update()
    {
        if (Physics2D.OverlapBox(interactSource.position, interactSize, 0, playerLayer))
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                OnInteract();
            }
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

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(interactSource.position, interactSize);
    }
    #endregion
}
