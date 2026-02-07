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
    public bool saveCustomRun;
    public bool saveClassicRun;
    public int countdownDuration;
    public string ghostName;
    public GameManager.LevelDifficulty levelDifficulty;
    public GameManager.ResultsMode resultsMode;
    public Vector2 resultsModeValues;

    [Header("Audio Settings")]
    [Range(0.0001f, 1f)] public float masterVolume;
    [Range(0.0001f, 1f)] public float musicVolume;
    [Range(0.0001f, 1f)] public float sfxVolume;

    [Header("Ghosts datas")]
    public Dictionary<string, string> ghostsDatas;
    public List<string> levelGhostsNames;
}
