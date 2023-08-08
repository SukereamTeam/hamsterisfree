using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText = null;

    [SerializeField]
    private TextMeshProUGUI currentText = null;

    private static string nextScene;
    private float progress;

    List<UniTask> loadingTask = new List<UniTask>();


    private async void Start()
    {
        // Data Load ?

        loadingTask.Add(UniTask.Defer(TestCode1));
        loadingTask.Add(UniTask.Defer(TestCode2));
        loadingTask.Add(UniTask.Defer(LoadScene));

        //

        await Test();

        // ----------- 밑에 있는게 되는 코드(쓰려면 위에 다 주석)
        //await LoadScene();
    }


    private async UniTask Test()
    {
        int i = 0;
        foreach (var task in loadingTask)
        {
            await task;

            i++;

            var percent = (loadingTask.Count / i) * 100;
            loadingText.text = $"{percent} %";
        }
    }

    private async UniTask TestCode1()
    {
        await UniTask.Delay(3000);      //3초 대기

        currentText.text = "TestCode111";
    }

    private async UniTask TestCode2()
    {
        await UniTask.Delay(3000);      //3초 대기

        currentText.text = "TestCode222";
    }


    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;

        SceneManager.LoadScene("Loading");
    }

    


    private async UniTask LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f); // allowSceneActivation이 false일 때까지 진행률을 0.9까지 제한합니다.
            //loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            if (progress >= 1.0f)
            {
                op.allowSceneActivation = true;
            }

            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }

}
