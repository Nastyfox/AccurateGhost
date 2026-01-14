using UnityEngine;

[CreateAssetMenu(fileName = "GlobalData", menuName= "ScriptableObjects/GlobalData")]
public class GlobalDataScriptableObject : ScriptableObject
{
    public string pseudo;
    public bool displayGhostBefore;
    public bool displayGhostDuring;
    public int frameOffset;
}
