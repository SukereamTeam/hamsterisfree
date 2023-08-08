using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DataContainer
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


    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static StageTable StageTable { get; private set; }
    public static SeedTable SeedTable { get; private set; }





    public static void Initialize()
    {
        StageTable = new StageTable();
        ReadCSV(StageTable, "StageTable");
    }


    public static List<Dictionary<string, T>> ReadCSV<T, S>(TableBase<T, S> _TableBase, string _FileName)
    {
        var list = new List<Dictionary<string, T>>();
        var table = Resources.Load<TextAsset>($"Data/csv/{_FileName}");

        var lines = Regex.Split(table.text, LINE_SPLIT_RE);

        if (lines.Length <= 1)
            return list;


        var headers = Regex.Split(lines[1], SPLIT_RE);  //csv의 둘째줄부터 header로 판정
        for (var i = 2; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "")
            {
                continue;
            }

            var tableData = new Dictionary<string, T>();

            for (var j = 0; j < headers.Length && j < values.Length; j++)
            {
                string value = values[j];

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                _TableBase.SetTable(values[0], headers[j], value);
            }

            list.Add(tableData);
        }

        return list;
    }

    // TODO
    // Json 으로 진행상황 저장하는 함수 만들기
    // 진행해야하는 스테이지 넘버 (첫 시작이면 0이란 소리)
    // 탈출문 좌표
    // 게임 시작할 때 여기 저장된 스테이지 넘버로 스테이지 데이터테이블 참조하여 불러옴

}
