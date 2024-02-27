using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void OnHit(float damageAmount, Vector2 knockbackAmount);
}
