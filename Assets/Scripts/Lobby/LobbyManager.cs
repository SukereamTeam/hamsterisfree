using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using Demo;
using TMPro;

public class LobbyManager : MonoSingleton<LobbyManager>
{
    [SerializeField]
    private float fadeDuration = 0f;

    public float FadeDuration => this.fadeDuration;

    [SerializeField]
    private TextMeshProUGUI rewardText = null;

    [SerializeField]
    private GameObject stagePrefab = null;

    [SerializeField]
    private InitOnStart scrollInit = null;


    private CancellationTokenSource cancellationToken;
    public CancellationTokenSource Cts => this.cancellationToken;


    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();
        
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("# Lobby Initialize #");
        
        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";
        
        this.scrollInit.Initialize(DataContainer.Instance.StageTable.list.Count);
    }
}
