using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    // TODO
    // 엑셀 데이터 형식 : 0, seed, monster

    // 맵 데이터 형식
    // (스테이지 번호(index), string = "맵 배경테마, seedPos{(0, 0), (3, 0)}, monsterPos{Type, Size, (좌표시작), (좌표끝)}" + 맵 Type, ... 
    // {(startPos, endPos)},
    // "" 면 random 으로 처리 ...
    // endPos - startPos 로 거리 측정해서, 거리만큼 for 문 돌며 생성 ?
    // random 이면 random 값 뽑아내서 만들기
    // 타일에 딱 맞춰 생성하지 말고 , 0.5 정도 random으로 +- 주면서 생성 


    // monster Type
    // 움직이는 속도가 랜덤
    // 랜덤한 좌표에 나타나는 녀석
    // 손가락 위치에 따라 아래위/왼오로 움직이는 애?

    // 맵 Type
    // Timer, Star(반드시 먹어야 하는 해씨의 갯수), Heart(기회 횟수)
    private Dictionary<int, List<string>> mapContainer = new Dictionary<int, List<string>>();

    // csv 읽어오고나서
    // Data Convert


}
