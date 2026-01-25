using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
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

    private ESaveSystem runSaveSystem;
    private SaveFileSetup runSaveFileSetup;

    [Range(0f, 1f)]
    [SerializeField] private float accuracyThreshold;
    [Range(1, 5)]
    [SerializeField] private int frameThreshold;

    private TextMeshProUGUI countdownText;

    [SerializeField] private Playback savePlayback;
    private Playback ghostPlayback;
    private Playback replayPlayback;

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

    [SerializeField] private GameObject ghostPlaybackPrefab;
    [SerializeField] private GameObject replayPlaybackPrefab;
    [SerializeField] private GameObject runSavePrefab;

    private event Action displayGhostDuringDelegate;

    public bool GetIsCameraFollowingGhost()
    {
        return isCameraFollowingGhost;
    }

    public void SetLevel()
    {
        player = Instantiate(playerPrefab, levelDataScriptableObject.playerStartPosition, Quaternion.identity);
        ResultsMenu.resultsMenuInstance.SetPlayer(player);

        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.Target.TrackingTarget = player.transform;

        ghostPlayback = Instantiate(ghostPlaybackPrefab).GetComponent<Playback>();
        replayPlayback = Instantiate(replayPlaybackPrefab).GetComponent<Playback>();
        GameObject runSaveGO = Instantiate(runSavePrefab);
        runSaveSystem = runSaveGO.GetComponent<ESaveSystem>();
        runSaveFileSetup = runSaveGO.GetComponent<SaveFileSetup>();

        playbacks = FindObjectsByType<Playback>(FindObjectsSortMode.None);
        foreach (Playback playback in playbacks)
        {
            playback.SetPlayback(player, cinemachineCamera);
        }

        countdownText = GameObject.Find("Countdown").GetComponent<TextMeshProUGUI>();
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

    public void UnsubscribeGhostDuring()
    {
        PlayerCollisions.StartEvent -= displayGhostDuringDelegate;
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
        countdownText.gameObject.SetActive(true);

        while (remainingTimeBeforeStart > 0)
        {
            countdownText.text = remainingTimeBeforeStart.ToString();
            await MenuManager.menuManagerInstance.CountdownScaleButton(countdownText.gameObject, 0.5f);
            remainingTimeBeforeStart--;
        }

        countdownText.text = "GO !";
        startTimer = true;
        await MenuManager.menuManagerInstance.CountdownScaleButton(countdownText.gameObject, 1f);
        countdownText.gameObject.SetActive(false);
    }

    private void StartRecord()
    {
        savePlayback.SetIsRecording(true);
    }

    private async UniTaskVoid StopRecord()
    {
        AudioManager.audioManagerInstance.StopWalkSFX();

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
        if (globalDataScriptableObject.countdownDuration > 0)
        {
            await StartCountdown(globalDataScriptableObject.countdownDuration);
        }

        EnablePlayerControls();
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
        await (DisplayPlayback(ghostPlayback, false, false, 0, savedRun), DisplayPlayback(replayPlayback, true, false, 0, currentRun));
    }

    public async UniTask StartLevel()
    {
        playerPseudo = globalDataScriptableObject.pseudo;
        displayGhostBefore = globalDataScriptableObject.displayGhostBefore;
        displayGhostDuring = globalDataScriptableObject.displayGhostDuring;
        frameOffset = globalDataScriptableObject.frameOffset;
        saveRun = globalDataScriptableObject.saveRun;
        levelDifficulty = globalDataScriptableObject.levelDifficulty;

        savePlayback.ResetPlayback();

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
            displayGhostDuringDelegate = async () =>
            {
                await DisplayPlayback(ghostPlayback, isCameraFollowingGhost, false, frameOffset, savedRun);
            };

            PlayerCollisions.StartEvent += displayGhostDuringDelegate;
        }
    }
}
