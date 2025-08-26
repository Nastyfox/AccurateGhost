using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TarodevGhost;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GhostRunner ghostRunner;
    [SerializeField] private ESaveSystem eSaveSystem;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private Playback playback;

    private int remainingTimeBeforeStart;

    [SerializeField] private bool saveRun;
    [SerializeField] private bool displayGhost;

    [SerializeField] private InputActionAsset inputAction;
    private InputActionMap playModeActionMap;
    private InputActionMap ghostModeActionMap;

    private string savedRun = "";
    [SerializeField] private RunDataScriptableObject runDataScriptableObject;

    private void OnEnable()
    {
        PlayerCollisions.startEvent += StartRecord;
        PlayerCollisions.endEvent += StopRecord;
        Playback.playbackDoneEvent += UniTask.Action(EnablePlayerControls);
    }

    private void OnDisable()
    {
        PlayerCollisions.startEvent -= StartRecord;
        PlayerCollisions.endEvent -= StopRecord;
        Playback.playbackDoneEvent -= UniTask.Action(EnablePlayerControls);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        ghostModeActionMap = inputAction.FindActionMap("GhostMode");
        playModeActionMap = inputAction.FindActionMap("PlayMode");

        if (!saveRun)
        {
            ghostModeActionMap.Enable();
            playModeActionMap.Disable();
        }

        savedRun = eSaveSystem.Load();
        if(savedRun == null)
        {
            savedRun = runDataScriptableObject.runData;
        }

        if (displayGhost)
        {
            playback.LoadDatas(savedRun);
            playback.SetIsPlaybacking(true);
        }
        else
        {
            EnablePlayerControls().Forget();
        }
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
            textMeshProUGUI.text = remainingTimeBeforeStart.ToString();
            await UniTask.Delay(1000);
            remainingTimeBeforeStart--;
        }

        textMeshProUGUI.text = "GO !";
    }

    private async UniTask TarodevGhost(string savedRun)
    {
        await ghostRunner.StartRecordAsync();
        string currentRun = await ghostRunner.EndOfRecordAsync();

        Recording newRecording = new Recording(currentRun);
        Recording savedRecording = new Recording(savedRun);

        float score = newRecording.CompareRecording(savedRecording, accuracyThreshold, frameThreshold);

        textMeshProUGUI.text = ((int)(score * 100)).ToString() + "%";
    }

    private void StartRecord()
    {
        playback.SetIsRecording(true);
    }

    private void StopRecord()
    {
        playback.SetIsRecording(true);
        string currentRun = playback.SaveDatas();

        if(saveRun)
        {
            eSaveSystem.Save(currentRun);
        }
        else
        {
            float score = playback.CompareRuns(currentRun, savedRun, accuracyThreshold, frameThreshold);
            textMeshProUGUI.text = ((int)(score * 100)).ToString() + "%";
            textMeshProUGUI.enabled = true;
        }
    }

    private async UniTaskVoid EnablePlayerControls()
    {
        await StartCountdown(3);
        ghostModeActionMap.Disable();
        playModeActionMap.Enable();
        await UniTask.Delay(1000);
        textMeshProUGUI.enabled = false;
    }
}
