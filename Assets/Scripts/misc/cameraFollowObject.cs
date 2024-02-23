using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollowObject : MonoBehaviour
{
    [SerializeField] private float flipYRotationTime;

    private Coroutine turnCoroutine;
    [SerializeField] private Transform PlayerTransform;
    private playerController PlayerController;
    private bool isFacingRight;

    void Awake()
    {
        PlayerTransform = GameObject.FindWithTag("Player").transform;
        PlayerController = GameObject.FindWithTag("Player").GetComponent<playerController>();
        isFacingRight = PlayerController.isFacingRight;
    }

    void Update()
    {
        transform.position = PlayerTransform.position;
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseLinear();
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
        {
            return 0f;
        }
        else
        {
            return 180f;
        }
    }
}
