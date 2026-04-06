using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager singleton { get; private set; }

    [SerializeField] private AudioMixerGroup sfxGroup; 

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(this);
        }
        
        DontDestroyOnLoad(this.gameObject);
    }


    public void PlaySound(string name, bool randomisePitch)
    {
        AudioClip clip = null;    
        
        clip = Resources.Load<AudioClip>($"Audio/SFX/{name}");

        if (clip == null)
        {
            Debug.LogWarning("Sound not found! Check name is correct.");
            return;
        }
        
        AudioSource source = new GameObject().AddComponent<AudioSource>();
        source.gameObject.transform.SetParent(transform);
        source.clip = clip;
        source.outputAudioMixerGroup = sfxGroup;
        if (randomisePitch)
        {
            source.pitch = Random.Range(0.8f, 1.2f);
        }
        source.Play();
        StartCoroutine(DestroyInstance(source.gameObject, source.clip.length));
    }
    
    static IEnumerator DestroyInstance(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(go);
    }
    
    
    
}
