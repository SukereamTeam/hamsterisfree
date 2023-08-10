using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar = null;

    private float progress = 0f;


    private void Update()
    {
        if (progress <= 0.9f)
        {
            if (SceneController.Operation != null)
            {
                progress = Mathf.Clamp01(SceneController.Operation.progress);
            }
        }
        else
        {
            progress = 1f;
        }

        progressBar.value = progress;
    }
    

}
