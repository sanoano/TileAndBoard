using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private bool isMainMenu;//Hides the tutorial+disconnect options if it's the options dialogue for the main menu

    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [SerializeField] private GameObject buttonTutorial;
    [SerializeField] private GameObject buttonDisconnect;

    [SerializeField] private GameObject tutorial;


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

        toggleTutorial(false);

        if (isMainMenu)
        {
            buttonTutorial.SetActive(false);
            buttonDisconnect.SetActive(false);
        }
        else
        {
            buttonTutorial.SetActive(true);
            buttonDisconnect.SetActive(true);
        }
    }

    public void toggleTutorial(bool on)
    {
        if (on)
        {
            gameObject.SetActive(false);
            tutorial.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
            tutorial.SetActive(false);
        }
    }
}
