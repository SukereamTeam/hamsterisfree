using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar = null;

    private void Awake()
    {
        progressBar.value = 0f;
    }

    public void UpdateProgress(float amount)
    {
        if (progressBar == null)
            return;

        float targetAmount = Mathf.Clamp01(amount);
        this.progressBar.DOValue(targetAmount, 0.1f);
    }
}
