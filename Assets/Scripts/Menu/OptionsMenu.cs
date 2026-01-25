using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu optionsMenuInstance;

    [SerializeField] private GameObject menuButtonPrefab;
    [SerializeField] private GameObject difficultyList;
    [SerializeField] private TextMeshProUGUI ghostDelayValueText;

    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    [SerializeField] GlobalDataScriptableObject globalDataScriptableObject;

    [SerializeField] TextMeshProUGUI panelText;

    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Toggle ghostBeforeToggle;
    [SerializeField] private Toggle ghostDuringToggle;
    [SerializeField] private Slider ghostDelaySlider;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private TMP_Dropdown resultsModesDropdown;

    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject mainMenuAndQuitButton;

    public static bool isGamePaused;

    private void Start()
    {
        if (optionsMenuInstance == null)
        {
            optionsMenuInstance = this;
            DontDestroyOnLoad(this.gameObject);

            GameManager.CompareMode[] resultsModes = (GameManager.CompareMode[])Enum.GetValues(typeof(GameManager.CompareMode));

            for (int i = 0; i < resultsModes.Length; i++)
            {
                resultsModesDropdown.options.Add(new TMP_Dropdown.OptionData { text = resultsModes[i].ToString() });
            }

            SetUIInitialValues();

            mainMenuButton.onClick.AddListener(async () =>
            {
                Time.timeScale = 1f;
                optionsPanel.SetActive(false);
                backButton.SetActive(false);
                backButton.transform.SetParent(backButton.transform.parent.parent);
                GameManager.gameManagerInstance.UnsubscribeGhostDuring();
                await LevelLoader.levelLoaderInstance.LoadMainMenu();
            });

            quitButton.onClick.AddListener(() =>
            {
                LevelLoader.levelLoaderInstance.QuitGame();
            });
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void SetUIInitialValues()
    {
        ghostBeforeToggle.isOn = globalDataScriptableObject.displayGhostBefore;
        ghostDuringToggle.isOn = globalDataScriptableObject.displayGhostDuring;
        ghostDelaySlider.value = globalDataScriptableObject.frameOffset;

        masterVolumeSlider.value = globalDataScriptableObject.masterVolume;
        musicVolumeSlider.value = globalDataScriptableObject.musicVolume;
        sfxVolumeSlider.value = globalDataScriptableObject.sfxVolume;

        resultsModesDropdown.value = (int)globalDataScriptableObject.resultsMode;
    }

    public void SetGhostBefore(bool value)
    {
        globalDataScriptableObject.displayGhostBefore = value;
    }

    public void SetGhostDuring(bool value)
    {
        globalDataScriptableObject.displayGhostDuring = value;
    }

    public void SetGhostDelay(float sliderValue)
    {
        globalDataScriptableObject.frameOffset = (int)sliderValue;
        ghostDelayValueText.text = "Ghost Delay : " + globalDataScriptableObject.frameOffset.ToString() + " frames";
    }

    public void SetMasterVolume(float volume)
    {
        globalDataScriptableObject.masterVolume = volume;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        globalDataScriptableObject.musicVolume = volume;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetSFXVolume(float volume)
    {
        globalDataScriptableObject.sfxVolume = volume;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetResultsMode(int modeIndex)
    {
        globalDataScriptableObject.resultsMode = (GameManager.CompareMode)modeIndex;
    }

    private async UniTask SelectDifficulty(GameManager.LevelDifficulty difficulty)
    {
        string pseudo = playerDataSaveSystem.LoadPlayerData(playerDataSaveFileSetup, "Pseudo");
        optionsPanel.SetActive(false);
        await LevelLoader.levelLoaderInstance.LoadLevel(SceneManager.GetActiveScene().name, difficulty);
    }

    private void InstantiateDifficultyObject(GameObject difficultyObject, GameManager.LevelDifficulty difficulty)
    {
        GameObject difficultyGO = Instantiate(difficultyObject, difficultyList.transform);
        Button difficultyButton = difficultyGO.GetComponent<Button>();
        difficultyButton.onClick.AddListener(async () => await SelectDifficulty(difficulty));
        difficultyGO.GetComponentInChildren<TextMeshProUGUI>().text = difficulty.ToString();
    }

    public void SetOptionsMenu()
    {
        replayButton.gameObject.SetActive(false);
        panelText.text = "Options";
        difficultyList.SetActive(false);
        mainMenuAndQuitButton.SetActive(false);
    }

    public async UniTask SetPauseMenu()
    {
        backButton.transform.SetParent(optionsPanel.transform);
        replayButton.gameObject.SetActive(true);
        mainMenuAndQuitButton.SetActive(true);
        panelText.text = "Pause";
        difficultyList.SetActive(true);
        optionsPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        while (!Leaderboard.leaderboardInstance.GetIsInitialized())
        {
            await UniTask.Yield();
        }

        difficultyList.transform.DeleteChildren();

        GameManager.LevelDifficulty[] difficulties = (GameManager.LevelDifficulty[])Enum.GetValues(typeof(GameManager.LevelDifficulty));

        InstantiateDifficultyObject(menuButtonPrefab, GameManager.LevelDifficulty.Easy);

        for (int i = 1; i < difficulties.Length; i++)
        {
            GameManager.LevelDifficulty previousLevelDifficulty = difficulties[i - 1];
            GameManager.LevelDifficulty currentLevelDifficulty = difficulties[i];

            string levelName = SceneManager.GetActiveScene().name + "_" + previousLevelDifficulty;
            Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = await Leaderboard.leaderboardInstance.GetPlayerScoreWithMetadata(levelName);

            if (playerEntry != null)
            {
                InstantiateDifficultyObject(menuButtonPrefab, currentLevelDifficulty);
            }
        }

        replayButton.onClick.RemoveAllListeners();
        replayButton.onClick.AddListener(async () =>
        {
            ResumeFromPause().Forget();
            GameManager.gameManagerInstance.UnsubscribeGhostDuring();
            await LevelLoader.levelLoaderInstance.LoadLevel(SceneManager.GetActiveScene().name, globalDataScriptableObject.levelDifficulty);
        });
    }

    public async UniTask PauseFromGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        await MenuManager.menuManagerInstance.DisplayMenu(optionsPanel, null, MenuManager.AnimationType.Scale);
    }

    public async UniTask ResumeFromPause()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        await MenuManager.menuManagerInstance.HideMenu(optionsPanel, null, MenuManager.AnimationType.Scale);
    }
}
