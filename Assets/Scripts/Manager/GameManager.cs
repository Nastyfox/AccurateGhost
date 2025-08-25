using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TarodevGhost;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GhostRunner ghostRunner;
    [SerializeField] private ESaveSystem eSaveSystem;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

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
        string savedRun = "";

        while (remainingTimeBeforeStart > 0)
        {
            textMeshProUGUI.text = remainingTimeBeforeStart.ToString();
            await UniTask.Delay(1000);
            remainingTimeBeforeStart--;
            if (remainingTimeBeforeStart == 1)
            {
                savedRun = eSaveSystem.Load();
            }
        }

        textMeshProUGUI.text = "GO !";
        await ghostRunner.StartRecordAsync();
        string currentRun = await ghostRunner.EndOfRecordAsync();
        
        //eSaveSystem.Save(currentRun);

        Recording newRecording = new Recording(currentRun);
        Recording savedRecording = new Recording(savedRun);

        float score = newRecording.CompareRecording(savedRecording, accuracyThreshold, frameThreshold);

        textMeshProUGUI.text = ((int)(score * 100)).ToString() + "%";
    }
}
