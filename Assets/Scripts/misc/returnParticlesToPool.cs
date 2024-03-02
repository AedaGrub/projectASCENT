using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnParticlesToPool : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        objectPoolManager.ReturnObjectToPool(gameObject);
    }
}
