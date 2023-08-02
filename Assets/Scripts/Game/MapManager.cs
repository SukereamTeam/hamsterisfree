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
    private GameObject exitPrefab = null;




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
        // 배경 타일에서 하위 Tile은 (1, 0) (2, 0) (3, 0) 식으로 되어 있고
        // 상위 부모님이 y값을 가지고 있음

        // List<List<(int, int)>> intPairs = new List<List<(int, int)>>(3); valueTuple
        // 

    }

    private void SetExitTile()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        var random = UnityEngine.Random.Range(0, outlineTiles.Length);
        var randomPos = new Vector2(outlineTiles[random].transform.localPosition.x, outlineTiles[random].transform.localPosition.y);

        var exitTile = Instantiate<GameObject>(exitPrefab, this.transform);
        var exitScript = exitTile.GetComponent<TileBase>();
        exitScript.Initialize(TileType.Exit, "", randomPos);

        // TODO
        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }

    private void CreateTile()
    {

    }
}