using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    [SerializeField] private ESaveSystem eSaveSystem;
    [SerializeField] private SaveFileSetup runSaveFileSetup;
    [SerializeField] private SaveFileSetup resultsSaveFileSetup;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private Playback savePlayback;
    [SerializeField] private Playback ghostPlayback;

    private int remainingTimeBeforeStart;

    [SerializeField] private LevelDifficulty levelDifficulty;

    [SerializeField] private bool saveRun;
    [SerializeField] private bool displayGhostBefore;
    [SerializeField] private bool displayGhostDuring;

    [SerializeField] private InputActionAsset inputAction;
    private InputActionMap playModeActionMap;
    private InputActionMap ghostModeActionMap;

    private string savedRun = "";
    [SerializeField] private RunDataScriptableObject runDataScriptableObject;

    private float time = 0f;
    private bool startTimer = false;

    [Range(0, 100)]
    [SerializeField] private int frameOffset;

    private void OnEnable()
    {
        PlayerCollisions.startEvent += StartRecord;
        PlayerCollisions.endEvent += StopRecord;
        if(displayGhostDuring)
        {
            PlayerCollisions.startEvent += () => DisplayGhost(false, false, frameOffset);
        }

        Playback.playbackDoneEvent += UniTask.Action(StartRun);
    }

    private void OnDisable()
    {
        PlayerCollisions.startEvent -= StartRecord;
        PlayerCollisions.endEvent -= StopRecord;
        if (displayGhostDuring)
        {
            PlayerCollisions.startEvent -= () => DisplayGhost(false, false, frameOffset);
        }

        Playback.playbackDoneEvent -= EnablePlayerControls;
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

    private void StartRecord()
    {
        savePlayback.SetIsRecording(true);
    }

    private void StopRecord()
    {
        savePlayback.SetIsRecording(false);

        string currentRun = savePlayback.GetSavedDatas();

        if (saveRun)
        {
            eSaveSystem.SaveRun(currentRun, levelDifficulty, SceneManager.GetActiveScene().name, runSaveFileSetup);
        }
        else
        {
            float score = savePlayback.CompareRuns(currentRun, savedRun, accuracyThreshold, frameThreshold);
            startTimer = false;
            string levelName = SceneManager.GetActiveScene().name + "_" + levelDifficulty.ToString();
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

    private void StopPlayback()
    {
        savePlayback.SetIsPlaybacking(false);
    }

    private void EnablePlayerControls()
    {
        ghostModeActionMap.Disable();
        playModeActionMap.Enable();
    }

    private async UniTaskVoid StartRun()
    {
        await StartCountdown(3);

        EnablePlayerControls();

        await UniTask.Delay(1000);
        textMeshProUGUI.enabled = false;
    }

    private void DisplayGhost(bool follow, bool startRun, int frameOffset)
    {
        ghostPlayback.SetGhostPlayback(follow, startRun, frameOffset, savedRun);
        ghostPlayback.SetIsPlaybacking(true);
    }

    public void StartLevel(LevelDifficulty difficulty, bool ghostBefore, bool ghostDuring, int ghostDelayInFrames)
    {
        levelDifficulty = difficulty;
        displayGhostBefore = ghostBefore;
        displayGhostDuring = ghostDuring;
        frameOffset = ghostDelayInFrames;


        ghostModeActionMap = inputAction.FindActionMap("GhostMode");
        playModeActionMap = inputAction.FindActionMap("PlayMode");

        if (!saveRun)
        {
            ghostModeActionMap.Enable();
            playModeActionMap.Disable();
        }

        savedRun = eSaveSystem.LoadRun(runSaveFileSetup, levelDifficulty, SceneManager.GetActiveScene().name);
        if (savedRun == null)
        {
            switch (levelDifficulty)
            {
                case LevelDifficulty.Easy:
                    savedRun = runDataScriptableObject.easyRunData;
                    break;
                case LevelDifficulty.Medium:
                    savedRun = runDataScriptableObject.mediumRunData;
                    break;
                case LevelDifficulty.Hard:
                    savedRun = runDataScriptableObject.hardRunData;
                    break;
            }
        }

        if (displayGhostBefore)
        {
            DisplayGhost(true, true, 0);
        }
        else
        {
            StartRun().Forget();
        }
    }
}
