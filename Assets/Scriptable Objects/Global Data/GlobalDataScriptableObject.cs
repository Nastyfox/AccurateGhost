using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalData", menuName= "ScriptableObjects/GlobalData")]
public class GlobalDataScriptableObject : ScriptableObject
{
    [Header("Player Settings")]
    public string pseudo;

    [Header("Run Settings")]
    public bool displayGhostBefore;
    public bool displayGhostDuring;
    public int frameOffset;
    public bool saveRun;
    public int countdownDuration;
    public GameManager.LevelDifficulty levelDifficulty;
    public GameManager.ResultsMode resultsMode;
    public Vector2 resultsModeValues;

    [Header("Audio Settings")]
    [Range(0.0001f, 1f)] public float masterVolume;
    [Range(0.0001f, 1f)] public float musicVolume;
    [Range(0.0001f, 1f)] public float sfxVolume;
}
