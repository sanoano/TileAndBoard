using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;


    private void Awake()
    {

        musicVolume.onValueChanged.AddListener((v) =>
        {
            musicGroup.audioMixer.SetFloat("MusicVolume", v);
        });
        
        sfxVolume.onValueChanged.AddListener((v) =>
        {
            sfxGroup.audioMixer.SetFloat("SFXVolume", v);
        });


    }
    
    
    
}
