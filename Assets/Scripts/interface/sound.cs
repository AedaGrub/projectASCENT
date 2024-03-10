using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class sound
{
    public string name;
    public string type;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
}
