using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";
    private const float DefaultVolume = 0.5f;
    private const float MutedVolumeDb = -80f;

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
        float savedMusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume));
        float savedSfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume));

        musicVolume.SetValueWithoutNotify(savedMusicVolume);
        sfxVolume.SetValueWithoutNotify(savedSfxVolume);
        ApplySavedVolumes(musicGroup.audioMixer);

        musicVolume.onValueChanged.AddListener(SetMusicVolume);
        sfxVolume.onValueChanged.AddListener(SetSfxVolume);

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

    private void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        musicGroup.audioMixer.SetFloat(MusicVolumeKey, VolumeToDecibels(volume));
    }

    private void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        sfxGroup.audioMixer.SetFloat(SfxVolumeKey, VolumeToDecibels(volume));
    }

    public static void ApplySavedVolumes(AudioMixer audioMixer)
    {
        float savedMusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume));
        float savedSfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume));

        audioMixer.SetFloat(MusicVolumeKey, VolumeToDecibels(savedMusicVolume));
        audioMixer.SetFloat(SfxVolumeKey, VolumeToDecibels(savedSfxVolume));
    }

    private static float VolumeToDecibels(float volume)
    {
        return volume <= 0f ? MutedVolumeDb : Mathf.Log10(volume) * 20f;
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
