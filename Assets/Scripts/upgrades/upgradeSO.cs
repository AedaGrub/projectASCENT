using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class upgradeSO : ScriptableObject
{
    public new string name;
    public int ID;

    public Sprite upgradeSprite;

    public string upgradeType;
    public float upgradeValue;
    public float tierBonus;
}
