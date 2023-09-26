using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


/// <summary>
/// 프로젝트 내에서 쓰이는 모든 Audio들을 모아놓은 것을 ScriptableObject로 생성
/// Name으로 찾아서 AudioClip 반환하도록 함수 생성
/// </summary>


[CreateAssetMenu(fileName = "SoundTable", menuName = "ScriptableObjects/SoundDataList")]
public class SoundDataList : ScriptableObject
{
    [SerializeField]
    private List<SoundData> soundList = null;

    public List<SoundData> SoundList => this.soundList;


    public AudioClip FindAudioClipWithName(string _Name)
    {
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
