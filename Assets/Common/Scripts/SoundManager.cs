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
    private bool isInit = false;

    private CancellationTokenSource cts;

    private List<AudioSource> audioSourcePool;

    private const float FADE_TIME = 5f;

    

    public void Initialize()
    {
        if (this.isInit == true)
            return;

        this.isInit = true;

        this.cts = new CancellationTokenSource();

        this.audioSourcePool = new List<AudioSource>();

        var source = this.GetComponent<AudioSource>();
        source.playOnAwake = false;

        if (source != null)
        {
            this.audioSourcePool.Add(source);
        }
        
    }

    private AudioSource CreateAudioSource()
    {
        var source = this.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;

        this.audioSourcePool.Add(source);

        return source;
    }

    private AudioSource GetPoolingAudioSource()
    {
        foreach (var source in this.audioSourcePool)
        {
            if (source.isPlaying == false && source.loop == false)
            {
                return source;
            }
        }

        return null;
    }

    private AudioSource GetAudioSource()
    {
        var source = GetPoolingAudioSource();

        return source ?? CreateAudioSource();
    }


    public async UniTask PlayOneShot(AudioClip _Clip, float _Volume = 1, System.Action _OnComplete = null)
    {
        var source = GetAudioSource();

        source.PlayOneShot(_Clip, _Volume);

        await UniTask.Delay(TimeSpan.FromSeconds(_Clip.length));

        if (_OnComplete != null)
        {
            _OnComplete();
        }
    }

    public async UniTask Play(AudioClip _Clip, bool _Loop = false, bool _IsVolumeFade = false, float _Volume = 1, System.Action onComplete = null)
    {
        var source = GetAudioSource();
        source.clip = _Clip;
        source.loop = _Loop;

        source.Play();

        if (_IsVolumeFade == true)
        {
            FadeVolume(true, _Volume, source, this.cts).Forget();
        }
        else
        {
            source.volume = _Volume;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_Clip.length));

        if (onComplete != null)
        {
            onComplete();
        }
    }

    public void StopAll()
    {
        this.cts.Cancel();

        foreach (var source in this.audioSourcePool)
        {
            source.Stop();
            source.volume = 1f;
        }
    }

    private async UniTaskVoid FadeVolume(bool _IsFadeIn, float _Volume, AudioSource _AudioSource, CancellationTokenSource _Cts)
    {
        float initVolume = _IsFadeIn ? 0f : _Volume;
        float targetVolume = _IsFadeIn ? _Volume : 0f;

        float timer = 0;
        while (timer < FADE_TIME && _Cts.IsCancellationRequested == false)
        {
            _AudioSource.volume = Mathf.Lerp(initVolume, targetVolume, timer / FADE_TIME);
            timer += Time.deltaTime;

            await UniTask.Yield();
        }

        _AudioSource.volume = targetVolume;
    }
}
