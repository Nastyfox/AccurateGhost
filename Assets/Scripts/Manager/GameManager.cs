using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TarodevGhost;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GhostRunner ghostRunner;
    [SerializeField] private ESaveSystem eSaveSystem;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    private int remainingTimeBeforeStart;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        await StartCountdown(3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async UniTask StartCountdown(int countdownValue)
    {
        remainingTimeBeforeStart = countdownValue;
        while (remainingTimeBeforeStart > 0)
        {
            Debug.Log("Countdown: " + remainingTimeBeforeStart);
            await UniTask.Delay(1000);
            remainingTimeBeforeStart--;
        }
        /*
        string savedRun = eSaveSystem.Load();
        Debug.Log(savedRun);
        await UniTask.Delay(1000);
        */
        Debug.Log("GO!");
        await ghostRunner.StartRecordAsync();
        string currentRun = await ghostRunner.EndOfRecordAsync();

        /*
        Recording newRecording = new Recording(currentRun);
        Recording savedRecording = new Recording(savedRun);

        newRecording.CompareRecording(savedRecording, accuracyThreshold, frameThreshold);
        */
    }
}
