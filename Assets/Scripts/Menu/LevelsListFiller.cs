using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsListFiller : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;

    [SerializeField] private MenuEventSystemHandler menuEventSystemHandler;

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    [SerializeField] private TMP_Dropdown levelsDropdown;
    [SerializeField] private TMP_Dropdown ghostDropdown;
    [SerializeField] private Button startButton;
    private Dictionary<string, List<string>> ghostListPerLevel = new Dictionary<string, List<string>>();
    private bool firstLevelGhostDropdownSet;

    private string levelName;
    private string ghostName;

    private async UniTaskVoid Start()
    {
        while(!Leaderboard.leaderboardInstance.GetIsInitialized() || !CloudCodeManager.cloudCodeManagerInstance.IsDataRecovered())
        {
            await UniTask.Yield();
        }

        levelsDropdown.ClearOptions();
        ghostDropdown.ClearOptions();
        startButton.onClick.RemoveAllListeners();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            if (sceneName.Contains("Level"))
            {
                levelsDropdown.options.Add(new TMP_Dropdown.OptionData { text = sceneName });

                AddGhostListForLevel(sceneName);

                if (!firstLevelGhostDropdownSet)
                {
                    SetGhostDropdown(sceneName);
                }
            }
        }

        startButton.onClick.AddListener(async () => {
            await StartLevel();
        });

        levelName = levelsDropdown.options[0].text;
        ghostName = ghostDropdown.options[0].text;
    }

    private void AddGhostListForLevel(string levelName)
    {
        List<string> ghostList = new List<string>();

        ghostList = Extensions.PartialMatchKey(globalDataScriptableObject.ghostsDatas, levelName);

        GameManager.LevelDifficulty[] difficulties = (GameManager.LevelDifficulty[])Enum.GetValues(typeof(GameManager.LevelDifficulty));
        for (int a = difficulties.Length - 1; a >= 0; a--)
        {
            ghostList.Insert(0, difficulties[a].ToString());
        }

        ghostListPerLevel.Add(levelName, ghostList);
    }

    private void SetGhostDropdown(string levelName)
    {
        ghostDropdown.ClearOptions();

        foreach (string ghostName in ghostListPerLevel[levelName])
        {
            ghostDropdown.options.Add(new TMP_Dropdown.OptionData { text = ghostName.ToString() });
        }

        ghostDropdown.RefreshShownValue();

        firstLevelGhostDropdownSet = true;
    }

    public void OnChangeLevelDropdown(int levelIndex)
    {
        levelName = levelsDropdown.options[levelIndex].text;
        SetGhostDropdown(levelName);

        ghostName = ghostListPerLevel[levelName][0];
    }

    public void OnChangeGhostDropdown(int ghostIndex)
    {
        levelName = levelsDropdown.options[levelsDropdown.value].text;

        ghostName = ghostDropdown.options[ghostIndex].text;
    }

    public async UniTask StartLevel()
    {
        await LevelLoader.levelLoaderInstance.LoadLevel(levelName, ghostName);
    }
}
