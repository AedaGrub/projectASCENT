using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expireAfter : MonoBehaviour
{
    public void Despawn()
    {
        Destroy(gameObject);
    }

    public void DelayedDespawn(float delay)
    {
        Destroy(gameObject, delay);
    }
}
