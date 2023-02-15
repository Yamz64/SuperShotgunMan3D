using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{
    private static AudioScriptableObject sound_table = Resources.Load<AudioScriptableObject>("SFXContainer");
    private static Coroutine music_call;
    
    private static IEnumerator PlayOnce(AudioSource sound)
    {
        sound.Play();
        yield return new WaitUntil(() => sound.isPlaying);
        yield return new WaitUntil(() => !sound.isPlaying);
        MonoBehaviour.Destroy(sound.gameObject);
    }

    private static IEnumerator MusicLoop(AudioSource song, AudioClip new_loop, MonoBehaviour mono)
    {
        song.Play();
        yield return new WaitUntil(() => song.isPlaying);
        yield return new WaitUntil(() => song.time >= song.clip.length);
        song.clip = new_loop;
        song.Stop();
        song.Play();
        mono.StartCoroutine(MusicLoop(song, new_loop, mono));
    }

    //instance something from the SFX table at a point
    public static void InstanceSound(int index, Vector3 position, MonoBehaviour mono, Transform parent = null, bool falloff = false, float volume = 1.0f, float pitch = 1.0f)
    {
        if(index < 0 || index >= sound_table.SFX_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to InstanceSound of AudioUtils is out of range, setting it to 0!");
        }
        //cleanup any audio that has finished
        CleanupSFX();

        Object instantiation_object;
        if (falloff)
            instantiation_object = Resources.Load<Object>("AudioObjectNormal");
        else
            instantiation_object = Resources.Load<Object>("AudioObjectNOFALLOFF");

        GameObject sfx_object = (GameObject)MonoBehaviour.Instantiate(instantiation_object);
        sfx_object.transform.position = position;
        if (parent != null) sfx_object.transform.parent = parent;
        sfx_object.GetComponent<AudioSource>().clip = sound_table.SFX_ARRAY[index];
        sfx_object.GetComponent<AudioSource>().volume = volume * PlayerPrefs.GetFloat("SFX Volume", 1.0f);
        sfx_object.GetComponent<AudioSource>().pitch = pitch;
        mono.StartCoroutine(PlayOnce(sfx_object.GetComponent<AudioSource>()));
    }

    //instance something from the Voice table at a point
    public static void InstanceVoice(int index, Vector3 position, MonoBehaviour mono, Transform parent = null, bool falloff = false, float volume = 1.0f, float pitch = 1.0f)
    {
        if (index < 0 || index >= sound_table.VOICE_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to InstanceVoice of AudioUtils is out of range, setting it to 0!");
        }
        //cleanup any audio that has finished
        CleanupSFX();

        //make sure that there are no active voice objects, destroy any competing objects
        GameObject other = GameObject.Find("Voice_Object");
        if (other != null)
            MonoBehaviour.Destroy(other);

        Object instantiation_object;
        if (falloff)
            instantiation_object = Resources.Load<Object>("AudioObjectNormal");
        else
            instantiation_object = Resources.Load<Object>("AudioObjectNOFALLOFF");

        GameObject sfx_object = (GameObject)MonoBehaviour.Instantiate(instantiation_object);
        sfx_object.name = "Voice_Object";
        sfx_object.transform.position = position;
        if (parent != null) sfx_object.transform.parent = parent;
        sfx_object.GetComponent<AudioSource>().clip = sound_table.VOICE_ARRAY[index];
        sfx_object.GetComponent<AudioSource>().volume = volume * PlayerPrefs.GetFloat("SFX Volume", 0.5f);
        sfx_object.GetComponent<AudioSource>().pitch = pitch;
        mono.StartCoroutine(PlayOnce(sfx_object.GetComponent<AudioSource>()));
    }

    //instance something from the Music table and handle special looping
    public static void PlayMusic(int index, MonoBehaviour mono, float volume = 1.0f)
    {
        if (index < 0 || index >= sound_table.SFX_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to PlayMusic of AudioUtils is out of range, setting it to 0!");
        }
        GameObject music_object = (GameObject)MonoBehaviour.Instantiate(Resources.Load<Object>("AudioObjectMUSIC"));
        music_object.GetComponent<AudioSource>().clip = sound_table.MUSIC_ARRAY[index].song;
        music_object.GetComponent<AudioSource>().loop = true;
        music_object.GetComponent<AudioSource>().volume = volume * PlayerPrefs.GetFloat("Music Volume", 1.0f);
        if (sound_table.MUSIC_ARRAY[index].loop_clip != null)
        {
            music_call = mono.StartCoroutine(MusicLoop(music_object.GetComponent<AudioSource>(), sound_table.MUSIC_ARRAY[index].loop_clip, mono));
        }
    }

    public static void StopMusic(MonoBehaviour mono)
    {
        if (music_call == null)
            return;

        mono.StopCoroutine(music_call);
        Object.Destroy(GameObject.FindGameObjectWithTag("Music"));
    }

    public static void UpdateMusicVolume()
    {
        GameObject[] music_objects = GameObject.FindGameObjectsWithTag("Music");

        for(int i=0; i<music_objects.Length; i++)
        {
            music_objects[i].GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Music Volume", 1.0f);
        }
    }

    public static void UpdateSFXVolume()
    {
        GameObject[] sfx_objects = GameObject.FindGameObjectsWithTag("SFX");

        for (int i = 0; i < sfx_objects.Length; i++)
        {
            sfx_objects[i].GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFX Volume", 1.0f);
        }
    }

    //function cleans up all sound objects that have finished playing
    static void CleanupSFX()
    {
        GameObject[] sfx = GameObject.FindGameObjectsWithTag("SFX");
        for(int i=0; i<sfx.Length; i++)
        {
            if (!sfx[i].GetComponent<AudioSource>().isPlaying)
                MonoBehaviour.Destroy(sfx[i]);
        }
    }
}
