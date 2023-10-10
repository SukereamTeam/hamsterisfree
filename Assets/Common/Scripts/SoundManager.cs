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


    public List<AudioSource> AudioSources
    {
        get;
        private set;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        StopFadeCoroutine();
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

        audioSource.volume = _Volume;
        audioSource.PlayOneShot(audioClip);

        if (_OnComplete != null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(audioClip.length), cancellationToken: this.soundCts.Token);
            _OnComplete();
        }
    }


    public async UniTask Play(string _AudioPath, bool _Loop = false, float _FadeTime = 0f, float _Volume = 1f, Action _OnComplete = null)
    {
        var (audioSource, audioClip) = GetAudioSouceAndClip(_AudioPath);
        if (audioSource == null || audioClip == null) return;

        audioSource.clip = audioClip;
        audioSource.loop = _Loop;
        audioSource.Stop();
        audioSource.Play();

        if (_FadeTime > 0f)
        {
            FadeVolumeStart(audioSource, true, _FadeTime, _Volume);
        }
        else
        {
            audioSource.volume = _Volume;
        }

        if (_OnComplete != null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(audioClip.length), cancellationToken: this.soundCts.Token);
            _OnComplete();
        }
    }

    public void StopAll()
    {
        StopFadeCoroutine();

        foreach (var source in AudioSources)
        {
            if (source.clip != null)
            {
                source.Stop();
            }
        }
    }

    public void Stop(string _AudioPath, float _FadeTime = 0f)
    {
        var audioClip = DataContainer.Instance.SoundTable.FindAudioClipWithName(_AudioPath);
        if (audioClip == null)
        {
            Debug.Log("clip null");
            return;
        }

        var audioSource = AudioSources.Find((x) => x.clip != null && x.clip.name.Equals(audioClip.name));
        if (audioSource == null)
        {
            Debug.Log("source null");
            return;
        }

        if (_FadeTime > 0f)
        {
            FadeVolumeStart(audioSource, false, _FadeTime, audioSource.volume, _OnComplete: () =>
            {
                audioSource.clip = null;
            });
        }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;

            StopFadeCoroutine();
        }


    }

    /// <summary>
    /// 사운드 볼륨이 점점 커지게/작아지게 재생하는 옵션
    /// </summary>
    /// <param name="_AudioPath">사운드 경로</param>
    /// <param name="_IsFadeIn">FadeIn 으로 점점 커지게 할건지(true), FadeOut으로 점점 작아지게 할건지(false)</param>
    /// <param name="_FadeTime"></param>
    /// <param name="_Volume"></param>
    /// <param name="_OnComplete"></param>
    public void FadeVolumeStart(AudioSource _Source, bool _IsFadeIn, float _FadeTime, float _Volume, Action _OnComplete = null)
    {
        StopFadeCoroutine();

        this.fadeCoroutine = StartCoroutine(FadeVolume(_Source, _IsFadeIn, _FadeTime, _Volume, _OnComplete));
    }

    public IEnumerator FadeVolume(AudioSource _AudioSource, bool _IsFadeIn, float _FadeTime, float _Volume, Action _OnComplete)
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
            this.index = this.index + 1 >= AudioSources.Count ? 0 : this.index + 1;
            var audioSource = AudioSources[this.index];
            if (audioSource.clip == null || audioSource.isPlaying == false)
                return audioSource;

            if (++loopCount >= AudioSources.Count)
            {
                audioSource = this.gameObject.AddComponent<AudioSource>();
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

    private void StopFadeCoroutine()
    {
        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }
    }
}
