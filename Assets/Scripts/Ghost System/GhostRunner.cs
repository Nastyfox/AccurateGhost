using TarodevGhost;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GhostRunner : MonoBehaviour
{
    [SerializeField] private Transform recordTarget;
    [SerializeField] private GameObject ghostPrefab;
    [Range(1f, 10f)]
    [SerializeField] private int captureEveryNFrames = 2;

    private ReplaySystem replaySystem;

    private string runData;

    [SerializeField] private int recordDuration;

    private void Awake()
    {
        replaySystem = new ReplaySystem(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async UniTask StartRecordAsync()
    {
        replaySystem.StartRun(recordTarget, captureEveryNFrames);
        replaySystem.PlayRecording(RecordingType.Best, Instantiate(ghostPrefab));
        runData = replaySystem.GetRunData(RecordingType.Best);
    }

    public async UniTask<string> EndOfRecordAsync()
    {
        await UniTask.Delay(recordDuration * 1000);
        replaySystem.FinishRun();
        replaySystem.StopReplay();
        await StartRecordAsync();
        return replaySystem.GetRunData(RecordingType.Best);
    }

    public string GetRunData()
    {
        if(runData != "")
            return runData;
        
        else 
            return "No Run Data";
    }

    public void LoadRunData(string data)
    {
        replaySystem.LoadRunData(data, Instantiate(ghostPrefab));
    }
}
