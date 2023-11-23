using System.Collections;
using System.Collections.Generic;
using Resux;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicDataConfig")]
public class MusicDataConfig : ScriptableObject
{
    public List<ChapterDetail> MusicGroups;
}