using UnityEngine;

[CreateAssetMenu(fileName = "LevelDataSampleScene", menuName= "ScriptableObjects/LevelData")]
public class LevelDataScriptableObject : ScriptableObject
{
    public string easyRunData;
    public string mediumRunData;
    public string hardRunData;

    public Vector2 playerStartPosition;
}
