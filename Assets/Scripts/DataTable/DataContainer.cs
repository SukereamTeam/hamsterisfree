using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DataTable;
using System.Threading;

public class DataContainer : GlobalMonoSingleton<DataContainer>
{
    [SerializeField]
    private Table_Stage stageTable;
    public Table_Stage StageTable => stageTable;

    [SerializeField]
    private Table_Seed seedTable;
    public Table_Seed SeedTable => seedTable;
    
    [SerializeField]
    private Table_Monster monsterTable;
    public Table_Monster MonsterTable => monsterTable;

    [SerializeField]
    private Table_Sound soundTable;
    public Table_Sound SoundTable => soundTable;



    private const string RootPathStage = "Images/Map";
    private readonly int _tileSpriteCount = Enum.GetValues(typeof(Define.TileSpriteName)).Length;

    private List<Sprite> _stageSprites;
    public IReadOnlyList<Sprite> StageSprites => _stageSprites;

    private Dictionary<string, Sprite> _seedSprites;
    public IReadOnlyDictionary<string, Sprite> SeedSprites => _seedSprites;
    
    private Dictionary<string, Sprite> _monsterSprites;
    public IReadOnlyDictionary<string, Sprite> MonsterSprites => _monsterSprites;
    
    public Sprite ExitSprite { get; private set; }


    private CancellationTokenSource _cts = null;



    private void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }

        _stageSprites = new List<Sprite>(_tileSpriteCount);

        _cts = new CancellationTokenSource();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _cts?.Cancel();
        _cts?.Dispose();
    }
    
    public async UniTask LoadStageDatas(int stageIndex)
    {
        Debug.Log("LoadStageDatas 시작");

        try
        {
            var item = stageTable.list.FirstOrDefault(x => x.Index == stageIndex);

            if (item != null)
            {
                await LoadStageSprites(item.MapName, _cts);

                ExitSprite = await Resources.LoadAsync<Sprite>($"Images/Map/{item.MapName}/Exit") as Sprite;
                if (ExitSprite == null)
                    Debug.Log("### ERROR ---> ExitSprite is Null ###");

                await LoadSeedSprites(item, _cts);

                await LoadMonsterSprites(item, _cts);
            }
            else
            {
                Debug.Log($"### Error ---> {stageIndex} is Not ContainsKey ###");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"### DataContainer exception occurred: {ex.Message} / {ex.StackTrace}");
        }

        Debug.Log("LoadStageDatas 끝!");
    }


    private async UniTask LoadStageSprites(string mapName, CancellationTokenSource cts)
    {
        _stageSprites.Clear();

        // eg. Images/Map/Forest/Forset_
        var path = $"{RootPathStage}/{mapName}/{mapName}_";

        // Forest_Map 이라는 Sprite 한 장을 Multiple로 Slice해서, 각각 잘린 스프라이트들을 사용할것임
        // var mapSprites = Resources.LoadAll<Sprite>($"{path}Map");

        // TODO : Define.TileSpriteName의 Mask부터 시작하게 해놨는데, 추후 변경 필요
        for (int spriteIndex = 9; spriteIndex < _tileSpriteCount; spriteIndex++)
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            // 바뀐 버전 2 : TileBack 도 한 장을 늘려서 사용. 근데 이러면 안될 것 같음..
            var spriteName = Enum.GetName(typeof(Define.TileSpriteName), spriteIndex);
            var spritePath = $"{path}{spriteName}";
            var resource = await Resources.LoadAsync<Sprite>(spritePath);
            if (resource is Sprite sprite)
            {
                _stageSprites.Add(sprite);
            }
            else
                Debug.Log("### Fail <Sprite> Type Casting ###");
        }


        // TODO : Assetbundle or Addressable 로 변경하기
        /*
        //자산의 모든 하위 오브젝트를 로드하려면 다음 예제 구문을 사용할 수 있습니다.
        Addressables.LoadAssetAsync<IList<Sprite>>("MySpriteSheetAddress");

        //자산의 단일 하위 오브젝트를 로드하려면 다음을 수행할 수 있습니다.
        Addressables.LoadAssetAsync<Sprite>("MySpriteSheetAddress[MySpriteName]");

        // 위의 하위 오브젝트는 모르겠고 폴더를 라벨로 잡고 그 폴더의 내용을 전부 땡겨오는건 이 방법
        var handle = Addressables.LoadAssetsAsync<GameObject>("Label", obj =>
        {
            //Gets called for every loaded asset
            Debug.Log(obj.name);

        });
        yield return handle;
        IList<GameObject> singleKeyResult = handle.Result;
        ...
        ...
        [SerializeField] protected Image m_Image;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return Addressables.InitializeAsync();

            string atlasName = "Atlas_Icon";
            string imageName = "emoticon_001";
            var handle = Addressables.LoadAssetAsync<Sprite>($"{atlasName}[{imageName}]"); // Atlas_Icon[emoticon_001]

            yield return handle;

            m_Image.sprite = handle.Result;

            Addressables.Release(handle);
        }
        */
    }

    private async UniTask LoadSeedSprites(Table_Stage.Param item, CancellationTokenSource cts)
    {
        var seedCount = item.SeedData.Count;
        _seedSprites = new Dictionary<string, Sprite>(seedCount);

        for (int i = 0; i < seedCount; i++)
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            var seedData = seedTable.GetParamFromType(item.SeedData[i].Item1, item.SeedData[i].Item2);

            var sprite = await Resources.LoadAsync<Sprite>(seedData.SpritePath) as Sprite;

            if (sprite != null)
	            _seedSprites.Add(item.SeedData[i].Item1, sprite);
            else
                Debug.Log($"### ERROR LoadSeedSprites ---> {seedData.Type} ###");
        }
    }
    
    private async UniTask LoadMonsterSprites(Table_Stage.Param item, CancellationTokenSource cts)
    {
        var monsterCount = item.MonsterData.Count;
        _monsterSprites = new Dictionary<string, Sprite>(monsterCount);

        for (int i = 0; i < monsterCount; i++)
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            var monsterData = monsterTable.GetParamFromType(item.MonsterData[i].Item1, item.MonsterData[i].Item2);

            var sprite = await Resources.LoadAsync<Sprite>(monsterData.SpritePath) as Sprite;

            if (sprite != null)
            {
	            _monsterSprites.Add(item.MonsterData[i].Item1, sprite);
            }
            else
                Debug.Log($"### ERROR LoadMonsterSprites ---> {monsterData.Type} ###");
        }
    }
}
