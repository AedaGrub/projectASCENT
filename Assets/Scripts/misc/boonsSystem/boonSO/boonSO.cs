using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class boonSO : ScriptableObject
{
    [field: SerializeField] public string name { get; set; }

    [field: SerializeField] public Sprite iconSprite { get; set; }
    [field: SerializeField] public Sprite tierSprite { get; set; }

    [field: SerializeField] public string type { get; set; }
    [field: SerializeField] public int tier { get; set; }
}
