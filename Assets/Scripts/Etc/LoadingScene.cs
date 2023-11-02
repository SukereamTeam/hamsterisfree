using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar = null;

    private Tween tween;

    private void Awake()
    {
        progressBar.value = 0f;
    }

    private void OnDestroy()
    {
        this.tween?.Kill(true);
    }

    public void UpdateProgress(float amount)
    {
        if (progressBar == null)
            return;

        float targetAmount = Mathf.Clamp01(amount);
        this.tween = this.progressBar.DOValue(targetAmount, 0.5f);
    }
}
