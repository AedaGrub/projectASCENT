using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expireAfter : MonoBehaviour
{
    public void Despawn()
    {
        objectPoolManager.ReturnObjectToPool(gameObject);
    }
}
