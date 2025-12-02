using UnityEngine;
using Esper.ESave;
using NUnit.Framework;
using System.Collections.Generic;

public class ESaveSystem : MonoBehaviour
{
    private SaveFile saveFile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public struct Results
    {
        public string completion;
        public string chrono;
        public string medal;
        public string pseudo;

        public Results(string _completion, string _chrono, string _medal, string _pseudo)
        {
            completion = _completion;
            chrono = _chrono;
            medal = _medal;
            pseudo = _pseudo;
        }
    }

    public void SaveRun(string data, GameManager.LevelDifficulty levelDifficulty, string levelName, SaveFileSetup runSaveFileSetup)
    {
        saveFile = runSaveFileSetup.GetSaveFile();
        Debug.Log("Save Data: " + data);
        string levelKey = levelName + "_" + levelDifficulty.ToString();
        saveFile.AddOrUpdateData(levelKey, data);
        saveFile.Save();
    }

    public string LoadRun(SaveFileSetup runSaveFileSetup, GameManager.LevelDifficulty levelDifficulty, string levelName)
    {
        saveFile = runSaveFileSetup.GetSaveFile();
        string levelKey = levelName + "_" + levelDifficulty.ToString();
        string testLoad = saveFile.GetData<string>(levelKey);
        return testLoad;
    }
}
