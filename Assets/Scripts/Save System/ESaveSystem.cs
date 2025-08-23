using UnityEngine;
using Esper.ESave;

public class ESaveSystem : MonoBehaviour
{
    private SaveFileSetup saveFileSetup;
    private SaveFile saveFile;
    private string saveData;

    [SerializeField] private GhostRunner ghostRunner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveFileSetup = GetComponent<SaveFileSetup>();
        saveData = "Save Data";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save()
    {
        saveFile = saveFileSetup.GetSaveFile();
        saveData = ghostRunner.GetRunData();
        Debug.Log("Save Data: " + saveData);
        saveFile.AddOrUpdateData("TestSave", saveData);
        saveFile.Save();
    }

    public string Load()
    {
        saveFile = saveFileSetup.GetSaveFile();
        string testLoad = saveFile.GetData<string>("TestSave");
        ghostRunner.LoadRunData(testLoad);
        return testLoad;
    }
}
