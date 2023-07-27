using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    private static bool IsReady
    {
        get
        {
            return instance != null && instance.audioSource != null;
        }
    }

    public static float Volume
    {
        get
        {
            if (instance != null)
            {
                return instance.volume;
            }

            return 1;
        }

        set
        {
            if (instance != null)
            {
                instance.volume = value;

                foreach (var audio in instance.audioSource)
                {
                    audio.Value.volume = instance.volume;
                }
            }
        }
    }

    public static bool Mute
    {
        get
        {
            if (instance != null)
            {
                return instance.mute;
            }

            return false;
        }

        set
        {
            if (instance != null)
            {
                instance.mute = value;

                foreach (var audio in instance.audioSource)
                {
                    audio.Value.mute = instance.mute;
                }
            }
        }
    }

    private Dictionary<int, AudioSource> audioSource = new Dictionary<int, AudioSource>();

    protected AudioSource[] AudioSource
    {
        get
        {
            return instance.audioSource.Values.ToArray();
        }
    }

    private bool mute = false;
    private int maxIndex = 100;
    private float volume = 1;
    private Coroutine fadeRoutine = null;

    public static AudioSource FindAudioSource(int index)
    {
        if (IsReady)
        {
            if (instance.audioSource.ContainsKey(index))
            {
                return instance.audioSource[index];
            }
        }

        return null;
    }

    public static void PlayOneShot(AudioClip clip, float volume = 1, int sourceIdx = 0, System.Action onComplete = null)
    {
        if (IsReady)
        {
            if (!instance.audioSource.ContainsKey(sourceIdx))
            {
                instance.CreateAudioSource(sourceIdx);
            }

            instance.audioSource[sourceIdx].PlayOneShot(clip, volume);

            instance.WaitForSeconds(clip.length, onComplete);
        }
        else
        {
            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public static void Play(AudioClip clip, bool loop = false, float volume = 1, int sourceIdx = 0, System.Action onComplete = null)
    {
        if (IsReady && clip != null)
        {
            if (!instance.audioSource.ContainsKey(sourceIdx))
            {
                instance.CreateAudioSource(sourceIdx);
            }

            instance.audioSource[sourceIdx].clip = clip;
            instance.audioSource[sourceIdx].loop = loop;
            instance.audioSource[sourceIdx].volume = volume;
            instance.audioSource[sourceIdx].Stop();
            instance.audioSource[sourceIdx].Play();

            instance.WaitForSeconds(clip.length, onComplete);
        }
        else
        {
            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public static void StartFade(int sourceIdx, float duration, float volume, System.Action onComplete = null)
    {
        if (IsReady)
        {
            instance.StopFade();
            instance.fadeRoutine = instance.StartCoroutine(instance.Fade(sourceIdx, duration, volume, onComplete));
        }
    }

    public static void Stop(int sourceIdx = 0)
    {
        if (IsReady)
        {
            instance.StopFade();

            if (instance.audioSource.ContainsKey(sourceIdx))
            {
                instance.audioSource[sourceIdx].volume = instance.volume;
                instance.audioSource[sourceIdx].Stop();
            }
        }
    }

    public static void AllStop()
    {
        if (IsReady)
        {
            instance.StopFade();

            foreach (var audio in instance.audioSource)
            {
                audio.Value.volume = instance.volume;
                audio.Value.Stop();
            }

            instance.StopAllCoroutines();
        }
    }

    public static int FindIndex()
    {
        if (IsReady)
        {
            foreach (var audio in instance.audioSource)
            {
                if (!audio.Value.isPlaying)
                {
                    return audio.Key;
                }
            }

            return instance.NewIndex();
        }

        return 0;
    }

    protected int NewIndex()
    {
        for (int i = 0; i < this.maxIndex; i++)
        {
            if (!instance.audioSource.ContainsKey(i))
            {
                return i;
            }
        }

        return 0;
    }

    protected IEnumerator Fade(int sourceIdx, float duration, float volume, System.Action onComplete = null)
    {
        var audio = FindAudioSource(sourceIdx);
        if (audio != null)
        {
            float startVolume = audio.volume;

            float t = 0;
            while (t <= 1)
            {
                t += Time.deltaTime / duration;
                audio.volume = Mathf.Lerp(startVolume, volume, t);

                yield return null;
            }

            audio.volume = volume;

            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    protected void StopFade()
    {
        if (instance.fadeRoutine != null)
        {
            instance.StopCoroutine(instance.fadeRoutine);
            instance.fadeRoutine = null;
        }
    }

    protected virtual void CreateAudioSource(int sourceIdx)
    {
        audioSource[sourceIdx] = gameObject.AddComponent<AudioSource>();
        audioSource[sourceIdx].playOnAwake = false;
    }

    protected virtual void Awake()
    {
        instance = this;
        var audioSourceLIst = GetComponents<AudioSource>();
        for (int i = 0; i < audioSourceLIst.Length; i++)
        {
            audioSource[i] = audioSourceLIst[i];
            audioSource[i].playOnAwake = false;
        }
    }
}