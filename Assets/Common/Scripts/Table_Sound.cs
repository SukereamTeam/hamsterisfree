using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


/// <summary>
/// 프로젝트 내에서 쓰이는 모든 Audio들을 모아놓은 것을 ScriptableObject로 생성
/// Name으로 찾아서 AudioClip 반환하도록 함수 생성
/// </summary>


//[CreateAssetMenu(fileName = "SoundTable", menuName = "ScriptableObjects/SoundDataList")]
public class Table_Sound : ScriptableObject
{
    [SerializeField]
    private List<SoundData> soundList = null;
    public List<SoundData> SoundList => this.soundList;

    [SerializeField]
    private Dictionary<string, AudioClip> soundDic = null;
    public Dictionary<string, AudioClip> SoundDic => this.soundDic;



    private void Awake()
    {
        this.soundDic = this.soundList.ToDictionary(keySelector: x => x.Name, elementSelector: x => x.Clip);
    }

    public AudioClip FindAudioClipWithName(string _Name)
    {
        //return this.soundDic.TryGetValue(_Name, out var clip) ? clip : null;

        SoundData data = this.soundList.FirstOrDefault(data => data.Name.Equals(_Name));
        return data?.Clip;
    }
}

[Serializable]
public class SoundData
{
    public string Name;
    public AudioClip Clip;
}
