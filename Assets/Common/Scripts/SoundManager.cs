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

    private CancellationTokenSource soundCts;

    private Dictionary<int, AudioSource> audioSourceDic = new Dictionary<int, AudioSource>();
    public List<AudioSource> AudioSourceList
    {
        get => this.audioSourceDic.Values.ToList();
    }

    private Coroutine fadeCoroutine;

    private const float FADE_TIME = 5f;

    

    public void Initialize()
    {
        if (this.isInit == true)
            return;

        this.isInit = true;

        this.fadeCoroutine = null;

        this.soundCts = new CancellationTokenSource();

        this.audioSourceDic = this.GetComponents<AudioSource>()?
            .Select((audioSource, index) =>
            {
                audioSource.playOnAwake = false;
                return new { Index = index, AudioSource = audioSource };
            })
            .ToDictionary(item => item.Index, item => item.AudioSource);
    }

    private void CreateAudioSource(int _Index)
    {
        this.audioSourceDic[_Index] = this.gameObject.AddComponent<AudioSource>();
        this.audioSourceDic[_Index].playOnAwake = false;
    }

    public async UniTask PlayOneShot(AudioClip _Clip, int _Index,  float _Volume = 1, System.Action _OnComplete = null)
    {
        if (this.audioSourceDic.ContainsKey(_Index) == false)
        {
            CreateAudioSource(_Index);
        }

        this.audioSourceDic[_Index].PlayOneShot(_Clip, _Volume);

        await UniTask.Delay(TimeSpan.FromSeconds(_Clip.length), cancellationToken: this.soundCts.Token);

        if (_OnComplete != null)
        {
            _OnComplete();
        }
    }

    public async UniTask Play(AudioClip _Clip, int _Index, bool _Loop = false, bool _IsVolumeFade = false, float _FadeTIme = FADE_TIME, float _Volume = 1f, System.Action _OnComplete = null)
    {
        if (this.audioSourceDic.ContainsKey(_Index) == false)
        {
            CreateAudioSource(_Index);
        }

        this.audioSourceDic[_Index].clip = _Clip;
        this.audioSourceDic[_Index].loop = _Loop;
        this.audioSourceDic[_Index].Stop();
        this.audioSourceDic[_Index].Play();


        if (_IsVolumeFade == true)
        {
            FadeVolumeStart(true, _Volume, this.audioSourceDic[_Index], _FadeTIme);
        }
        else
        {
            this.audioSourceDic[_Index].volume = _Volume;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_Clip.length));

        if (_OnComplete != null)
        {
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

        foreach (var source in this.audioSourceDic)
        {
            source.Value.Stop();
        }
    }

    public void Stop(int _Index)
    {
        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }

        if (this.audioSourceDic.ContainsKey(_Index) == true)
        {
            this.audioSourceDic[_Index].Stop();
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }
    }
}
