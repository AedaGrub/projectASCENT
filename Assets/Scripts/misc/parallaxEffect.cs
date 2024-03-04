using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallaxEffect : MonoBehaviour
{
    private float length;
    private Vector2 startPos;
    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxAmount;

    void Start()
    {
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }


    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxAmount));
        float distX = (cam.transform.position.x * parallaxAmount);
        float distY = (cam.transform.position.y * parallaxAmount);

        transform.position = new Vector3(startPos.x + distX, startPos.y + distY, transform.position.z);

        if (temp > startPos.x + length)
        {
            startPos.x += length;
        }
        else if (temp < startPos.x - length)
        {
            startPos.x -= length;
        }
    }
}
