using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager singleton { get; private set; }

    [SerializeField] private AudioMixerGroup sfxGroup; 

    private void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(this);
        }
        else
        {
            singleton = this;
        }
        
        DontDestroyOnLoad(this.gameObject);
    }


    public void PlaySound(AudioClip clip)
    {
        AudioSource source = new GameObject().AddComponent<AudioSource>();
        source.gameObject.transform.SetParent(transform);
        source.clip = clip;
        source.outputAudioMixerGroup = sfxGroup;
        source.Play();
        StartCoroutine(DestroyInstance(source.gameObject, source.clip.length));
    }
    
    static IEnumerator DestroyInstance(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(go);
    }
    
    
    
}
