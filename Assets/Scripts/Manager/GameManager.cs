using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using TMPro;
using Unity.Cinemachine;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
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

    public enum  ResultsMode
    {
        Easy,
        Medium,
        Hard
    }

    public static GameManager gameManagerInstance;

    private ESaveSystem runSaveSystem;
    private SaveFileSetup runSaveFileSetup;

    private TextMeshProUGUI countdownText;

    [SerializeField] private Playback savePlayback;
    private Playback ghostPlayback;
    private Playback replayPlayback;

    private int remainingTimeBeforeStart;

    private LevelDifficulty levelDifficulty;
    private string ghostName;

    private bool saveClassicRun;
    private bool saveCustomRun;
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

    private GhostSaveBindings ghostSaveBindings;
    private LeaderboardAdmin leaderboardAdmin;

    public bool GetIsCameraFollowingGhost()
    {
        return isCameraFollowingGhost;
    }

    public void SetLevel()
    {
        player = Instantiate(playerPrefab, levelDataScriptableObject.playerStartPosition, Quaternion.identity);

        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.Target.TrackingTarget = player.transform;
        cinemachineCamera.transform.position = player.transform.position;
        
        ResultsMenu.resultsMenuInstance.SetPlayer(player);

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
        PlayerCollisions.EndEvent += UniTask.Action(StopRecord);

        Playback.playbackDoneEvent += UniTask.Action(StartRun);

        if(LevelLoader.levelLoaderInstance != null)
        {
            levelDifficulty = LevelLoader.levelLoaderInstance.GetSelectedDifficulty();
        }

        ghostSaveBindings = new GhostSaveBindings(CloudCodeService.Instance);
        leaderboardAdmin = new LeaderboardAdmin(CloudCodeService.Instance);
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
            await MenuAnimationManager.menuManagerInstance.CountdownScaleButton(countdownText.gameObject, 0.5f);
            remainingTimeBeforeStart--;
        }

        countdownText.text = "GO !";
        startTimer = true;
        EnablePlayerControls();
        StartRecord();

        if (displayGhostDuring && !saveClassicRun && !saveCustomRun)
        {
            isCameraFollowingGhost = false;
            DisplayPlayback(ghostPlayback, isCameraFollowingGhost, false, frameOffset, savedRun).Forget();
        }

        await MenuAnimationManager.menuManagerInstance.CountdownScaleButton(countdownText.gameObject, 1f);
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

        if (saveClassicRun)
        {
            runSaveSystem.SaveRun(currentRun, levelDifficulty, SceneManager.GetActiveScene().name, runSaveFileSetup);
        }
        else if(saveCustomRun)
        {
            string ghostName = SceneManager.GetActiveScene().name + "_" + globalDataScriptableObject.pseudo;

            try
            {
                await ghostSaveBindings.SaveGhostData(currentRun, ghostName);
                Debug.Log("Ghost data saved");
            }
            catch
            {
                Debug.Log("Saving ghost data failed");
            }

            try
            {
                await Leaderboard.leaderboardInstance.GetScoresWithMetadata(ghostName);
                Debug.Log("Leaderboard already exists");
            }
            catch
            {
                try
                {
                    await leaderboardAdmin.CreateLeaderboard(ghostName);
                    Debug.Log("Leaderboad created");
                }
                catch
                {
                    Debug.Log("Leaderboard creation failed");
                }
            }
        }
        else
        {
            DisablePlayerControls();

            int frameThreshold = (int)globalDataScriptableObject.resultsModeValues.x;
            float accuracyThreshold = globalDataScriptableObject.resultsModeValues.y;

            float score = savePlayback.CompareRuns(currentRun, savedRun, accuracyThreshold, frameThreshold);
            startTimer = false;
            string levelName = SceneManager.GetActiveScene().name + "_" + ghostName;

            int scorePercent = (int)(score * 100);
            await ResultsMenu.resultsMenuInstance.ResultsLevel(scorePercent);
            int scoreLeaderboard = scorePercent * ((int)globalDataScriptableObject.resultsMode + 1);

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
        saveClassicRun = globalDataScriptableObject.saveClassicRun;
        saveCustomRun = globalDataScriptableObject.saveCustomRun;
        levelDifficulty = globalDataScriptableObject.levelDifficulty;
        ghostName = globalDataScriptableObject.ghostName;

        savePlayback.ResetPlayback();

        if (!saveClassicRun && !saveCustomRun)
        {
            DisablePlayerControls();

            //savedRun = runSaveSystem.LoadRun(runSaveFileSetup, ghostName, SceneManager.GetActiveScene().name);
            try
            {
                savedRun = globalDataScriptableObject.ghostsDatas[SceneManager.GetActiveScene().name + "_" + ghostName];
            }
            catch
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
                isCameraFollowingGhost = false;
                cinemachineCamera.Target.TrackingTarget = player.transform;
            }
            else
            {
                StartRun().Forget();
            }
        }
        else
        {
            StartRun().Forget();
        }
    }
}
