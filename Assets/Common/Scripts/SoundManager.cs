using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : GlobalMonoSingleton<SoundManager>
{
    private int index = 0;
    private bool isInit = false;
    private CancellationTokenSource soundCts;
    private Coroutine fadeCoroutine;

    private const float FADE_TIME = 5f;

    public List<AudioSource> AudioSources
    {
        get;
        private set;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }
    }

    public void Initialize()
    {
        if (this.isInit == true)
            return;

        this.isInit = true;

        this.fadeCoroutine = null;

        this.soundCts = new CancellationTokenSource();

        AudioSources = this.GetComponents<AudioSource>().ToList();
    }

    public async UniTask PlayOneShot(string audioPath, float _Volume = 1, Action _OnComplete = null)
    {
        var (audioSource, audioClip) = GetAudioSouceAndClip(audioPath);
        if (audioSource == null || audioClip == null) return;

        audioSource.PlayOneShot(audioClip, _Volume);

        if (_OnComplete != null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(audioClip.length), cancellationToken: this.soundCts.Token);
            _OnComplete();
        }
    }


    public async UniTask Play(string audioPath, bool _Loop = false, float _FadeTime = FADE_TIME, float _Volume = 1f, Action _OnComplete = null)
    {
        var (audioSource, audioClip) = GetAudioSouceAndClip(audioPath);
        if (audioSource == null || audioClip == null) return;

        audioSource.clip = audioClip;
        audioSource.loop = _Loop;
        audioSource.Stop();
        audioSource.Play();

        if (_FadeTime > 0f)
        {
            FadeVolumeStart(true, _Volume, audioSource, _FadeTime);
        }
        else
        {
            audioSource.volume = _Volume;
        }

        if (_OnComplete != null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(audioClip.length));
            _OnComplete();
        }
    }

    public void StopAll()
    {
        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }

        foreach (var source in AudioSources)
        {
            source.Stop();
        }
    }

    public void Stop(string audioPath)
    {
        var audioClip = DataContainer.Instance.SoundTable.FindAudioClipWithName(audioPath);
        if (audioClip == null) return;

        var audioSource = AudioSources.Find((x) => x.clip != null && x.clip.name.Equals(audioClip.name));
        if (audioSource == null ) return;

        audioSource.Stop();

        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }
    }

    public void FadeVolumeStart(bool _IsFadeIn, float _Volume, AudioSource _AudioSource, float _FadeTime, System.Action _OnComplete = null)
    {
        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }

        this.fadeCoroutine = StartCoroutine(FadeVolume(_IsFadeIn, _Volume, _AudioSource, _FadeTime, _OnComplete));
    }

    public IEnumerator FadeVolume(bool _IsFadeIn, float _Volume, AudioSource _AudioSource, float _FadeTime, System.Action _OnComplete)
    {
        float initVolume = _IsFadeIn ? 0f : _Volume;
        float targetVolume = _IsFadeIn ? _Volume : 0f;

        float timer = 0;

        while (timer < _FadeTime)
        {
            _AudioSource.volume = Mathf.Lerp(initVolume, targetVolume, timer / _FadeTime);
            timer += Time.deltaTime;

            yield return null;
        }

        _AudioSource.volume = targetVolume;

        if (_OnComplete != null)
        {
            _OnComplete();
        }
    }

    private AudioSource GetAudioSource()
    {
        int loopCount = 0;
        while (true)
        {
            int nextIndex = this.index + 1 >= AudioSources.Count ? 0 : this.index + 1;
            if (AudioSources[nextIndex].clip == null || AudioSources[nextIndex].isPlaying == false)
                return AudioSources[nextIndex];

            if (++loopCount >= AudioSources.Count)
            {
                var audioSource = this.gameObject.AddComponent<AudioSource>();
                AudioSources.Add(audioSource);
                return audioSource;
            }
        }
    }
    private (AudioSource, AudioClip) GetAudioSouceAndClip(string audioPath)
    {
        var audioClip = DataContainer.Instance.SoundTable.FindAudioClipWithName(audioPath);
        if (audioClip == null) return (null, null);

        var audioSource = GetAudioSource();
        if (audioSource == null) return (null, audioClip);

        return (audioSource, audioClip);
    }
}
