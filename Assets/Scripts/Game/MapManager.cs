using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // 맵 생성
    [SerializeField]
    private Transform[] outlineTiles = null;

    [SerializeField]
    private GameObject exitTile = null;

    [SerializeField]
    private GameObject seedTile = null;

    [SerializeField]
    private GameObject monsterTile = null;



    private void Awake()
    {
        ChangeNameOutlineTiles();

        SetExitTile();


    }

    private void ChangeNameOutlineTiles()
    {
        for (int i = 0; i < outlineTiles.Length; i++)
        {
            outlineTiles[i].name = $"Tile ({outlineTiles[i].localPosition.x}, {outlineTiles[i].localPosition.y})";
        }
    }

    private void SetSeedTile()
    {

    }

    private void SetExitTile()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        var random = UnityEngine.Random.Range(0, outlineTiles.Length);
        var randomPos = new Vector2(outlineTiles[random].transform.localPosition.x, outlineTiles[random].transform.localPosition.y);

        var exitTileObj = Instantiate<GameObject>(exitTile, randomPos, Quaternion.identity, this.transform);

        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }
}