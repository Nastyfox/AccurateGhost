using UnityEngine;
using Esper.ESave;

public class ESaveSystem : MonoBehaviour
{
    private SaveFileSetup saveFileSetup;
    private SaveFile saveFile;

    [SerializeField] private GhostRunner ghostRunner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveFileSetup = GetComponent<SaveFileSetup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(string data)
    {
        saveFile = saveFileSetup.GetSaveFile();
        Debug.Log("Save Data: " + data);
        saveFile.AddOrUpdateData("RunSave", data);
        saveFile.Save();
    }

    public string Load()
    {
        saveFile = saveFileSetup.GetSaveFile();
        string testLoad = saveFile.GetData<string>("RunSave");
        return testLoad;
    }
}
