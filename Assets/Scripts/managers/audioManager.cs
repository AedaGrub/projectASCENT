using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioManager : MonoBehaviour
{
    public static audioManager instance;

    public AudioMixer masterMixer;
    public List<sound> sounds;
    public List<aSource> sources;

    private AudioSource currentSourceRef;

    void Awake()
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

    public void Play(string name)
    {
        sound s = sounds.Find(sound => sound.name == name);
        aSource aS = sources.Find(aSource => aSource.type == s.type);

        if (aS.type == "Music")
        {
            if (aS.source.clip == null)
            {
                aS.source.clip = s.clip;
                aS.source.volume = s.volume;
                aS.source.Play();
            }
            else if (aS.source.clip != s.clip)
            {
                currentSourceRef = aS.source;
                float v1 = aS.source.volume;
                float v2 = s.volume;
                StartCoroutine(SwitchMusic(v1, s.clip, v2, 1f));
            }
        }
        else
        {
            aS.source.PlayOneShot(s.clip, s.volume);

            if (aS.type == "PitchedSFX")
            {
                aS.source.pitch = Random.Range(0.9f, 1.1f);
            }
        }
    }

    public void Stop(string type)
    {
        aSource aS = sources.Find(aSource => aSource.type == type);
        currentSourceRef = aS.source;
        float v1 = aS.source.volume;

        StartCoroutine(SwitchMusic(v1, null, 0, 0.1f));
    }

    private IEnumerator SwitchMusic(float volume, AudioClip clip, float target, float duration)
    {
        LeanTween.value(volume, 0f, duration).setOnUpdate(UpdateMusicVolume).setEaseLinear();
        yield return new WaitForSeconds(duration);

        currentSourceRef.Stop();
        currentSourceRef.clip = clip;
        currentSourceRef.volume = target;

        if (clip != null)
        {
            currentSourceRef.Play();
        }
    }

    private void UpdateMusicVolume(float value)
    {
        currentSourceRef.volume = value;
    }

    public void LowHealth(bool state)
    {
        float currentLF;
        bool result1 = masterMixer.GetFloat("lowPassFreq", out currentLF);

        float currentRV;
        bool result2 = masterMixer.GetFloat("reverbRoom", out currentRV);

        float currentEQ;
        bool result3 = masterMixer.GetFloat("EQFreqGain", out currentEQ);

        if (state == true)
        {
            LeanTween.value(currentLF, 1500f, 0.1f).setOnUpdate(UpdateLowPass).setEaseOutExpo();
            LeanTween.value(currentRV, -500f, 0.1f).setOnUpdate(UpdateReverb).setEaseOutExpo();
            LeanTween.value(currentEQ, 2f, 0.1f).setOnUpdate(UpdateEQ).setEaseOutExpo();
        }
        else
        {
            LeanTween.value(currentLF, 22000f, 0.5f).setOnUpdate(UpdateLowPass).setEaseOutExpo();
            LeanTween.value(currentRV, -10000f, 0.5f).setOnUpdate(UpdateReverb).setEaseOutExpo();
            LeanTween.value(currentEQ, 1f, 0.5f).setOnUpdate(UpdateEQ).setEaseOutExpo();
        }
    }

    private void UpdateLowPass(float value)
    {
        masterMixer.SetFloat("lowPassFreq", value);
    }

    private void UpdateReverb(float value)
    {
        masterMixer.SetFloat("reverbRoom", value);
    }

    private void UpdateEQ(float value)
    {
        masterMixer.SetFloat("EQFreqGain", value);
    }
}
