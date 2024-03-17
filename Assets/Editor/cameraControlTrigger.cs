using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

public class cameraControlTrigger : MonoBehaviour
{
    public customInspectorObjects CustomInspectorObjects;
    public bool verticalChange;
    public bool horizontalChange;

    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Vector2 exitDirection = (col.transform.position - coll.bounds.center).normalized;

            if (CustomInspectorObjects.swapCameras && CustomInspectorObjects.negativeCamera != null && CustomInspectorObjects.positiveCamera != null)
            {
                if (verticalChange)
                {
                    cameraManager.instance.SwapCamera(CustomInspectorObjects.negativeCamera, CustomInspectorObjects.positiveCamera, exitDirection.y);
                }
                else
                {
                    cameraManager.instance.SwapCamera(CustomInspectorObjects.negativeCamera, CustomInspectorObjects.positiveCamera, exitDirection.x);
                }
            }
        }
    }
}

[System.Serializable]
public class customInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera negativeCamera;
    [HideInInspector] public CinemachineVirtualCamera positiveCamera;
}

[CustomEditor(typeof(cameraControlTrigger))]
public class MyScriptEditor : Editor
{
    cameraControlTrigger CameraControlTrigger;

    private void OnEnable()
    {
        CameraControlTrigger = (cameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (CameraControlTrigger.CustomInspectorObjects.swapCameras)
        {
            CameraControlTrigger.CustomInspectorObjects.negativeCamera = EditorGUILayout.ObjectField("Negative Camera", CameraControlTrigger.CustomInspectorObjects.negativeCamera,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            CameraControlTrigger.CustomInspectorObjects.positiveCamera = EditorGUILayout.ObjectField("Positive Camera", CameraControlTrigger.CustomInspectorObjects.positiveCamera,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(CameraControlTrigger);
        }
    }
}
