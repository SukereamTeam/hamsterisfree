using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class SceneController : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI currentText = null;

    [SerializeField]
    private Slider progressBar = null;

    [SerializeField]
    private Image fadeImage = null;

    [SerializeField]
    private float fadeDuration = 0f;


    private static string nextScene;

    private List<UniTask> loadingTask = new List<UniTask>();



    private async void Start()
    {
        // Data Load ?

        loadingTask.Add(UniTask.Defer(TestCode));

        loadingTask.Add(UniTask.Defer(LoadScene));

        //loadingTask.Add(UniTask.Defer(() => OnSceneLoaded()));

        await RunTasks();

        //await OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    //private async UniTask OnSceneLoaded()
    //{
    //    await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == nextScene);

    //    Scene_Base baseScene = FindObjectOfType<Scene_Base>();
    //    if (baseScene != null)
    //    {
    //        Debug.Log($"await baseScene.LoadDatas(); 호출 ~~~");
    //        await baseScene.LoadDatas();
    //    }
    //}

    private async UniTask RunTasks()
    {
        int i = 0;
        foreach (var task in loadingTask)
        {
            Debug.Log($"{i + 1} 번째 호출");

            await task;

            i++;
        }
    }

    private async UniTask TestCode()
    {
        await UniTask.Delay(3000);      //3초 대기

        currentText.text = "TestCode";
    }

    private async UniTask FadeIn()
    {
        
    }


    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;

        SceneManager.LoadScene("Loading");
    }


    private async UniTask LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        //op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f); // allowSceneActivation이 false일 때까지 진행률을 0.9까지 제한합니다.

            if (progress >= 0.9f)
            {
                Debug.Log("다 됐는디요 행님");

                break;
            }

            await UniTask.Yield(); // 다음 프레임까지 대기
        }
        //await op;

        await UniTask.CompletedTask;
    }

}
