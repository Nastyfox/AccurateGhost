using TarodevGhost;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class GhostRunner : MonoBehaviour
{
    [SerializeField] private Transform recordTarget;
    [SerializeField] private GameObject ghostPrefab;
    [Range(1f, 10f)]
    [SerializeField] private int captureEveryNFrames = 2;

    private ReplaySystem replaySystem;

    private int remainingTimeBeforeStart;

    private void Awake()
    {
        replaySystem = new ReplaySystem(this);
    }

    private async UniTaskVoid Start()
    {
        await StartCountdown(3);
        await StartRecord();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private async UniTask StartRecord()
    {
        await UniTask.Delay(remainingTimeBeforeStart * 1000); // waits for 1 second
        replaySystem.StartRun(recordTarget, captureEveryNFrames);
        replaySystem.PlayRecording(RecordingType.Saved, Instantiate(ghostPrefab));
        await EndOfRecord();
    }

    private async UniTask EndOfRecord()
    {
        await UniTask.Delay(10000);
        replaySystem.FinishRun();
        replaySystem.StopReplay();
        await StartRecord();
    }

    public async UniTask StartCountdown(int countdownValue)
    {
        remainingTimeBeforeStart = countdownValue;
        while (remainingTimeBeforeStart > 0)
        {
            Debug.Log("Countdown: " + remainingTimeBeforeStart);
            await UniTask.Delay(1000);
            remainingTimeBeforeStart--;
        }
    }
}
