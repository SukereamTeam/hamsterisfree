using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DataTable;
using System.Threading.Tasks;
using Random = System.Random;

public class DataContainer : GlobalMonoSingleton<DataContainer>
{
    [SerializeField]
    private Table_Stage stageTable;
    public Table_Stage StageTable => this.stageTable;

    [SerializeField]
    private Table_Seed seedTable;
    public Table_Seed SeedTable => this.seedTable;
    
    [SerializeField]
    private Table_Monster monsterTable;
    public Table_Monster MonsterTable => this.monsterTable;



    private const string RootPath_Stage = "Images/Map";
    private readonly int Tile_Sprite_Count = Enum.GetValues(typeof(Define.TileSpriteName)).Length;

    private List<Sprite> stageSprites;
    public IReadOnlyList<Sprite> StageSprites => this.stageSprites;

    
    private Sprite exitSprite;
    public Sprite ExitSprite => this.exitSprite;

    private Dictionary<string, Sprite> seedSprites;
    public Dictionary<string, Sprite> SeedSprites => this.seedSprites;
    
    private Dictionary<string, Sprite> monsterSprites;
    public Dictionary<string, Sprite> MonsterSprites => this.monsterSprites;





    private void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }

        this.stageSprites = new List<Sprite>(this.Tile_Sprite_Count);
    }



    // TODO
    // Json 으로 진행상황 저장하는 함수 만들기
    // 진행해야하는 스테이지 넘버 (첫 시작이면 0이란 소리)
    // 탈출문 좌표
    // 스테이지 선택해서 게임 시작할 때 여기 저장된 스테이지 넘버로 스테이지 데이터테이블 참조하여 불러옴



    public async UniTask LoadStageDatas(int _StageIndex)
    {
        Debug.Log("LoadStageDatas 시작");

        try
        {
            var item = stageTable.list.Where(x => x.Index == _StageIndex).FirstOrDefault();

            if (item != null)
            {
                await LoadStageSprites(item.MapName);

                // TODO : Modify
                this.exitSprite = this.stageSprites[0];//await Resources.LoadAsync<Sprite>("Images/Map/Forest/Forest_Center") as Sprite;
                if (this.exitSprite == null)
                    Debug.Log("### ERROR ---> ExitSprite is Null ###");

                await LoadSeedSprites(item);

                await LoadMonsterSprites(item);
            }
            else
            {
                Debug.Log($"### Error ---> {_StageIndex} is Not ContainsKey ###");
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }

        Debug.Log("LoadStageDatas 끝!");
    }


    private async UniTask LoadStageSprites(string _MapName)
    {
        this.stageSprites.Clear();

        // eg. Images/Map/Forest/Forset_
        var path = $"{RootPath_Stage}/{_MapName}/{_MapName}_";

        try
        {
            // Forest_Map 이라는 Sprite 한 장을 Multiple로 Slice해서, 각각 잘린 스프라이트들을 사용할것임
            

            for (int spriteIndex = 0; spriteIndex < this.Tile_Sprite_Count; spriteIndex++)
            {
                if (spriteIndex <= (int)Define.TileSpriteName.BottomRight)
                {
                    var mapSprites = Resources.LoadAll<Sprite>($"{path}Map");
                    
                    var sliceName = Enum.GetName(typeof(Define.TileSpriteName), spriteIndex);

                    if (sliceName == Define.TileSpriteName.Top.ToString() ||
                        sliceName == Define.TileSpriteName.Bottom.ToString() ||
                        sliceName == Define.TileSpriteName.Left.ToString() ||
                        sliceName == Define.TileSpriteName.Right.ToString())
                    {
                        var random = UnityEngine.Random.Range(1, 3);
                        sliceName = $"{sliceName}_{random}";
                    }
                
                    var spriteName = $"{_MapName}_{sliceName}";

                    var sprite = Array.Find(mapSprites, x => x.name == spriteName);

                    if (sprite != null)
                    {
                        this.stageSprites.Add(sprite);
                    }
                }
                else
                {
                    var spriteName = Enum.GetName(typeof(Define.TileSpriteName), spriteIndex);
                    var spritePath = $"{path}{spriteName}";
                    var resource = await Resources.LoadAsync<Sprite>(spritePath);
                    if (resource is Sprite sprite)
                    {
                        this.stageSprites.Add(sprite);
                    }
                    else
                        Debug.Log("### Fail <Sprite> Type Casting ###");
                }
                
            }
            
            // 기존 버전 (Sprite가 여러장이었음)
            // for (int spriteIndex = 0; spriteIndex < this.Tile_Sprite_Count; spriteIndex++)
            // {
            //     var spriteName = Enum.GetName(typeof(Define.TileSpriteName), spriteIndex);
            //
            //     var spritePath = $"{path}{spriteName}";
            //
            //     var resource = await Resources.LoadAsync<Sprite>(spritePath);
            //
            //     if (resource is Sprite sprite)
            //     {
            //         this.stageSprites.Add(sprite);
            //     }
            //     else
            //         Debug.Log("### Fail <Sprite> Type Casting ###");
            
            
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
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }
    }

    private async UniTask LoadSeedSprites(Table_Stage.Param item)
    {
        var seedCount = item.SeedData.Count;

        this.seedSprites = new Dictionary<string, Sprite>(seedCount);

        for (int i = 0; i < seedCount; i++)
        {
            var seedData = this.seedTable.GetParamFromType(item.SeedData[i].Item1, item.SeedData[i].Item2);

            var sprite = await Resources.LoadAsync<Sprite>(seedData.SpritePath) as Sprite;

            if (sprite != null)
            {
                this.seedSprites.Add(item.SeedData[i].Item1, sprite);
            }
            else
                Debug.Log($"### ERROR LoadSeedSprites ---> {seedData.Type} ###");
        }
    }
    
    private async UniTask LoadMonsterSprites(Table_Stage.Param item)
    {
        var monsterCount = item.MonsterData.Count;

        this.monsterSprites = new Dictionary<string, Sprite>(monsterCount);

        for (int i = 0; i < monsterCount; i++)
        {
            var monsterData = this.monsterTable.GetParamFromType(item.MonsterData[i].Item1, item.MonsterData[i].Item2);

            var sprite = await Resources.LoadAsync<Sprite>(monsterData.SpritePath) as Sprite;

            if (sprite != null)
            {
                this.monsterSprites.Add(item.MonsterData[i].Item1, sprite);
            }
            else
                Debug.Log($"### ERROR LoadMonsterSprites ---> {monsterData.Type} ###");
        }
    }
}
