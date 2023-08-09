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

    private List<UniTask> loadingTask = new List<UniTask>();

    private AsyncOperation operation;


    //private void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    private async void Start()
    {
        // Data Load ?

        loadingTask.Add(UniTask.Defer(TestCode));

        loadingTask.Add(UniTask.Defer(LoadScene));

        loadingTask.Add(UniTask.Defer(() => OnSceneLoaded()));

        await RunTasks();

        //await OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private async UniTask OnSceneLoaded()
    {
        Scene_Base baseScene = FindObjectOfType<Scene_Base>();
        if (baseScene != null)
        {
            Debug.Log($"await baseScene.LoadDatas(); 호출 ~~~");
            await baseScene.LoadDatas();

            this.operation.allowSceneActivation = true;
        }
        else
        {
            Debug.Log($"ㅡㅡ");
        }
    }

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


    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;

        SceneManager.LoadScene("Loading");
    }


    private async UniTask LoadScene()
    {
        operation = SceneManager.LoadSceneAsync(nextScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // allowSceneActivation이 false일 때까지 진행률을 0.9까지 제한합니다.
            //loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

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
