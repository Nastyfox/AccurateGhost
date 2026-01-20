using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
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

    public enum  CompareMode
    {
        Easy,
        Medium,
        Hard
    }

    public static GameManager gameManagerInstance;

    [SerializeField] private ESaveSystem runSaveSystem;
    [SerializeField] private SaveFileSetup runSaveFileSetup;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private Playback savePlayback;
    [SerializeField] private Playback ghostPlayback;
    [SerializeField] private Playback replayPlayback;

    private int remainingTimeBeforeStart;

    [SerializeField] private LevelDifficulty levelDifficulty;

    private bool saveRun;
    private bool displayGhostBefore;
    private bool displayGhostDuring;
    private int frameOffset;

    [SerializeField] private PlayerInput playerInput;

    private string savedRun = "";
    private string currentRun = "";
    [SerializeField] private LevelDataScriptableObject levelDataScriptableObject;

    [SerializeField] GlobalDataScriptableObject globalDataScriptableObject;

    private float time = 0f;
    private bool startTimer = false;

    private string playerPseudo = "";

    [SerializeField] private GameObject playerPrefab;
    private GameObject player;
    private Playback[] playbacks;

    private CinemachineCamera cinemachineCamera;
    private bool isCameraFollowingGhost = false;

    public bool GetIsCameraFollowingGhost()
    {
        return isCameraFollowingGhost;
    }

    public void SetLevel()
    {
        player = Instantiate(playerPrefab, levelDataScriptableObject.playerStartPosition, Quaternion.identity);
        ResultsMenu.resultsMenuInstance.SetPlayer(player);
        playbacks = FindObjectsByType<Playback>(FindObjectsSortMode.None);
        foreach (Playback playback in playbacks)
        {
            playback.SetTarget(player);
        }

        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.Target.TrackingTarget = player.transform;
    }

    private void OnEnable()
    {
        PlayerCollisions.StartEvent += StartRecord;
        PlayerCollisions.EndEvent += UniTask.Action(StopRecord);

        Playback.playbackDoneEvent += UniTask.Action(StartRun);

        if(LevelLoader.levelLoaderInstance != null)
        {
            levelDifficulty = LevelLoader.levelLoaderInstance.GetSelectedDifficulty();
        }
    }

    private void Awake()
    {
        if (gameManagerInstance == null)
        {
            gameManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDisable()
    {
        PlayerCollisions.StartEvent -= StartRecord;
        PlayerCollisions.EndEvent -= UniTask.Action(StopRecord);
        if (displayGhostDuring)
        {
            isCameraFollowingGhost = false;
            PlayerCollisions.StartEvent -= async () => await DisplayPlayback(ghostPlayback, isCameraFollowingGhost, false, frameOffset, savedRun);
        }

        Playback.playbackDoneEvent -= UniTask.Action(StartRun);
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
        Debug.Log("Start Record");
        savePlayback.SetIsRecording(true);
    }

    private async UniTaskVoid StopRecord()
    {
        savePlayback.SetIsRecording(false);

        currentRun = savePlayback.GetSavedDatas();

        if (saveRun)
        {
            runSaveSystem.SaveRun(currentRun, levelDifficulty, SceneManager.GetActiveScene().name, runSaveFileSetup);
        }
        else
        {
            DisablePlayerControls();

            float score = savePlayback.CompareRuns(currentRun, savedRun, accuracyThreshold, frameThreshold);
            startTimer = false;
            string levelName = SceneManager.GetActiveScene().name + "_" + levelDifficulty.ToString();

            int scorePercent = (int)(score * 100);
            await ResultsMenu.resultsMenuInstance.ResultsLevel(scorePercent);

            int timeInSecondsInt = (int)time;  //We don't care about fractions of a second, so easy to drop them by just converting to an int
            int minutes = timeInSecondsInt / 60;  //Get total minutes
            int seconds = timeInSecondsInt - (minutes * 60);  //Get seconds for display alongside minutes
            int milliSeconds = timeInSecondsInt - (minutes * 60 + seconds);  //Get seconds for display alongside minutes
            string timeText = minutes.ToString("D2") + ":" + seconds.ToString("D2") + ":" + milliSeconds.ToString("D2");
            
            await Leaderboard.leaderboardInstance.AddScoreWithMetadata(levelName, scorePercent, timeText, playerPseudo);
        }
    }

    private void StopPlayback()
    {
        savePlayback.SetIsPlaybacking(false);
    }

    private void EnablePlayerControls()
    {
        playerInput.SwitchCurrentActionMap("PlayMode");
    }

    private void DisablePlayerControls()
    {
        playerInput.SwitchCurrentActionMap("MenuMode");

    }

    private async UniTaskVoid StartRun()
    {
        await StartCountdown(3);

        EnablePlayerControls();

        await UniTask.Delay(1000);
        textMeshProUGUI.enabled = false;
    }

    private async UniTask DisplayPlayback(Playback playback, bool follow, bool startRun, int frameOffset, string run)
    {
        playback.SetGhostPlayback(follow, startRun, frameOffset, run);
        playback.SetIsPlaybacking(true);

        while (!playback.IsPlaybackDone())
        {
            await UniTask.Yield();
        }
    }

    public async UniTask RewatchRun()
    {
        DisablePlayerControls();
        isCameraFollowingGhost = true;
        await (DisplayPlayback(ghostPlayback, false, false, 0, savedRun), DisplayPlayback(replayPlayback, true, true, 0, currentRun));
    }

    public async UniTask StartLevel()
    {
        playerPseudo = globalDataScriptableObject.pseudo;
        displayGhostBefore = globalDataScriptableObject.displayGhostBefore;
        displayGhostDuring = globalDataScriptableObject.displayGhostDuring;
        frameOffset = globalDataScriptableObject.frameOffset;
        saveRun = globalDataScriptableObject.saveRun;
        levelDifficulty = globalDataScriptableObject.levelDifficulty;

        if (!saveRun)
        {
            DisablePlayerControls();
        }

        savedRun = runSaveSystem.LoadRun(runSaveFileSetup, levelDifficulty, SceneManager.GetActiveScene().name);
        if (savedRun == null)
        {
            switch (levelDifficulty)
            {
                case LevelDifficulty.Easy:
                    savedRun = levelDataScriptableObject.easyRunData;
                    break;
                case LevelDifficulty.Medium:
                    savedRun = levelDataScriptableObject.mediumRunData;
                    break;
                case LevelDifficulty.Hard:
                    savedRun = levelDataScriptableObject.hardRunData;
                    break;
            }
        }

        if (displayGhostBefore)
        {
            isCameraFollowingGhost = true;
            await DisplayPlayback(ghostPlayback, isCameraFollowingGhost, true, 0, savedRun);
            cinemachineCamera.Target.TrackingTarget = player.transform;
        }
        else
        {
            StartRun().Forget();
        }

        if (displayGhostDuring)
        {
            isCameraFollowingGhost = false;
            PlayerCollisions.StartEvent += async () => await DisplayPlayback(ghostPlayback, isCameraFollowingGhost, false, frameOffset, savedRun);
        }
    }
}
