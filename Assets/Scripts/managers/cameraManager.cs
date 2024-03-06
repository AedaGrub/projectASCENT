using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class cameraManager : MonoBehaviour
{
    public static cameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;

    [SerializeField] private float fallPanAmount;
    [SerializeField] private float fallPanTime;

    public bool isLerpingYDamping { get; private set; }
    public bool lerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

    [SerializeField] private float globalShakeForce;
    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];

                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normYPanAmount = framingTransposer.m_YDamping;

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }
    
    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            lerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0;
        while (elapsedTime < fallPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallPanTime));
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        isLerpingYDamping = false;
    }

    public void SwapCamera(CinemachineVirtualCamera positiveCamera, CinemachineVirtualCamera negativeCamera, float triggerExitDirection)
    {
        if (currentCamera == positiveCamera && triggerExitDirection > 0f)
        {
            negativeCamera.enabled = true;
            positiveCamera.enabled = false;

            currentCamera = negativeCamera;

            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        else if (currentCamera == negativeCamera && triggerExitDirection < 0f)
        {
            positiveCamera.enabled = true;
            negativeCamera.enabled = false;

            currentCamera = positiveCamera;

            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    public void CameraShake()
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
