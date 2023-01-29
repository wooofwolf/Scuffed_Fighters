using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; internal set; }

    [SerializeField] private AudioSource _musicSource, _effectsSource;

    private void Awake()
    {
        if (Instance != this && Instance != null) Destroy(Instance.gameObject);

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void PlaySound(AudioClip clip)
    {
        _effectsSource.PlayOneShot(clip,PlayerPrefs.GetFloat("SoundVolume"));
    }
}
