using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneL : MonoBehaviour
{
    private string nextSceneName = "YourNextScene"; // 다음으로 로드할 씬의 이름
    private bool isDataLoaded = false; // 데이터 로드 완료 여부

    public AsyncOperation op;

    private async void Start()
    {
        // 데이터 로드 비동기 작업 시작
        await LoadData();

        // 씬 로드 비동기 작업 시작
        op = SceneManager.LoadSceneAsync("Lobby");
        op.allowSceneActivation = false; // 씬 전환을 막음

        // 데이터 로드가 완료될 때까지 대기
        while (!isDataLoaded)
        {
            await UniTask.Yield();
        }

        // 데이터 로드 완료 후 씬 전환
        await Test();
    }



    private async UniTask Test()
    {
        op.allowSceneActivation = true;

        await UniTask.CompletedTask;
    }

    private async UniTask LoadData()
    {
        // 데이터 로드 작업 수행 (예시)
        await UniTask.Delay(2000); // 가상의 데이터 로드 시간

        // 데이터 로드 완료 처리
        isDataLoaded = true;
    }
}