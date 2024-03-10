using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class boonsSelectManager : MonoBehaviour
{
    public static boonsSelectManager instance;

    public GameObject[] options;

    public GameObject[] boons;

    public GameObject lastSelected {  get; set; }
    public int lastSelectedIndex { get; set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(SetSelectedAfterOneFrame());
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            HandleNextOptionSelection(1);
        }

        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            HandleNextOptionSelection(-1);
        }
    }

    private IEnumerator SetSelectedAfterOneFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HandleNextOptionSelection (int addition)
    {
        if (EventSystem.current.currentSelectedGameObject == null && lastSelected != null)
        {
            int newIndex = lastSelectedIndex + addition;
            newIndex = Mathf.Clamp(newIndex, 0, options.Length - 1);
            EventSystem.current.SetSelectedGameObject(options[newIndex]);
        }
    }
}
