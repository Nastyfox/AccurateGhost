using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using System.Threading.Tasks;
using TarodevGhost;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GhostRunner ghostRunner;
    [SerializeField] private ESaveSystem eSaveSystem;
    [SerializeField] private SaveFileSetup runSaveFileSetup;
    [SerializeField] private SaveFileSetup resultsSaveFileSetup;

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

    private float time = 0f;
    private bool startTimer = false;

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

        savedRun = eSaveSystem.LoadRun(runSaveFileSetup);
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
        if(startTimer)
        {
            time += Time.deltaTime;
        }
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
        startTimer = true;
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
            eSaveSystem.SaveRun(currentRun, runSaveFileSetup);
        }
        else
        {
            float score = playback.CompareRuns(currentRun, savedRun, accuracyThreshold, frameThreshold);
            startTimer = false;
            string levelName = SceneManager.GetActiveScene().name;
            string medal = "";
            if(score <= 0.3f)
            {
                medal = "Bronze";
            }
            else if(score <= 0.7f)
            {
                medal = "Silver";
            }
            else
            {
                medal = "Gold";
            }

            string scoreText = ((int)(score * 100)).ToString() + "%";
            textMeshProUGUI.text = ((int)(score * 100)).ToString() + "%";
            textMeshProUGUI.enabled = true;

            int timeInSecondsInt = (int)time;  //We don't care about fractions of a second, so easy to drop them by just converting to an int
            int minutes = timeInSecondsInt / 60;  //Get total minutes
            int seconds = timeInSecondsInt - (minutes * 60);  //Get seconds for display alongside minutes
            int milliSeconds = timeInSecondsInt - (minutes * 60 + seconds);  //Get seconds for display alongside minutes
            string timeText = minutes.ToString("D2") + ":" + seconds.ToString("D2") + ":" + milliSeconds.ToString("D2");

            eSaveSystem.SaveResults(levelName, scoreText, timeText, medal, resultsSaveFileSetup);
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
