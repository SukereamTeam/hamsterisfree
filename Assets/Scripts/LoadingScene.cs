using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar = null;

    private float currentValue;
    private float targetValue;

    private void Update()
    {
        if (SceneController.Instance.TaskCount > 0)
        {
            var result = ((float)SceneController.Instance.CompleteCount / (float)SceneController.Instance.TaskCount);
            this.targetValue = Mathf.Round(result * 100) / 100;    // 소수점 둘째자리까지 반올림
        }

        // 일정한 속도로 progressBar 움직이게
        this.currentValue = Mathf.MoveTowards(this.currentValue, this.targetValue, Time.deltaTime * 1f);

        progressBar.value = this.currentValue;

        if (this.currentValue >= 1f)
        {
            SceneController.Instance.LoadingDone = true;
        }
    }
    

}
