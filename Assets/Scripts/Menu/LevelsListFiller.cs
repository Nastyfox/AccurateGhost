using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class LevelsListFiller : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;

    [SerializeField] private MenuEventSystemHandler menuEventSystemHandler;

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    [SerializeField] private TMP_Dropdown levelsDropdown;
    [SerializeField] private Button saveNewGhostButton;
    private Dictionary<string, List<string>> ghostListPerLevel = new Dictionary<string, List<string>>();
    private bool firstLevelGhostDropdownSet;

    private string levelName;

    [SerializeField] private GameObject ghostGridPrefab;
    [SerializeField] private GameObject ghostButtonPrefab;
    [SerializeField] private Transform ghostListContainer;
    private Dictionary<string, GameObject> ghostListTabPerName = new Dictionary<string, GameObject>();
    private List<GameObject> ghostListTabs = new List<GameObject>();

    private Dictionary<string, Selectable> firstSelectablesInGrids = new Dictionary<string, Selectable>();
    private Selectable currentFirstSelectableInGrid;

    [SerializeField] private GameObject backButton;

    private async UniTaskVoid Start()
    {
        while(!Leaderboard.leaderboardInstance.GetIsInitialized() || !CloudCodeManager.cloudCodeManagerInstance.IsDataRecovered())
        {
            await UniTask.Yield();
        }

        globalDataScriptableObject.saveCustomRun = false;
        globalDataScriptableObject.saveClassicRun = false;

        levelsDropdown.ClearOptions();
        saveNewGhostButton.onClick.RemoveAllListeners();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        string firstTabSelected = "";

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            if (sceneName.Contains("Level"))
            {
                levelsDropdown.options.Add(new TMP_Dropdown.OptionData { text = sceneName });

                AddGhostListForLevel(sceneName);

                GameObject ghostGrid = Instantiate(ghostGridPrefab, ghostListContainer);
                ghostListTabPerName.Add(sceneName, ghostGrid);
                ghostListTabs.Add(ghostGrid);

                if (!firstLevelGhostDropdownSet)
                {
                    firstTabSelected = sceneName;
                    firstLevelGhostDropdownSet = true;
                }

                for (int j = 0; j < ghostListPerLevel[sceneName].Count; j++)
                {
                    string ghostName = ghostListPerLevel[sceneName][j];

                    GameObject ghostButton = Instantiate(ghostButtonPrefab, ghostGrid.transform);
                    ghostButton.GetComponentInChildren<TextMeshProUGUI>().text = ghostName;
                    ghostButton.GetComponent<Button>().onClick.AddListener(async () =>
                    {
                        await LevelLoader.levelLoaderInstance.LoadLevel(sceneName, ghostName);
                    });
                    menuEventSystemHandler.AddSelectable(ghostButton.GetComponent<Selectable>());

                    try
                    {
                        Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = await Leaderboard.leaderboardInstance.GetPlayerScore(sceneName + "_" + ghostName);
                        Extensions.GetComponentOnlyInChildren<Image>(ghostButton.transform).sprite = Leaderboard.leaderboardInstance.GetMedalFromScore(playerEntry.Score);
                    }
                    catch
                    {
                        Debug.Log("No result for player in this leaderboard");
                        Extensions.GetComponentOnlyInChildren<Image>(ghostButton.transform).enabled = false;
                    }

                    if (j == 0)
                    {
                        firstSelectablesInGrids.Add(sceneName, ghostButton.GetComponent<Selectable>());
                    }
                }
            }
        }

        saveNewGhostButton.onClick.AddListener(async () => {
            globalDataScriptableObject.saveCustomRun = true;
            await LevelLoader.levelLoaderInstance.LoadLevel(levelName, "");
        });

        levelName = levelsDropdown.options[0].text;
        DisplayLevelGhostList(levelName);

        Navigation navigation = backButton.GetComponent<Selectable>().navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnDown = currentFirstSelectableInGrid;
        navigation.selectOnRight = levelsDropdown;
        backButton.GetComponent<Selectable>().navigation = navigation;
        
        SetNavigationDown(levelsDropdown);
        SetNavigationDown(saveNewGhostButton);
    }

    private void OnDisable()
    {
        Navigation navigation = backButton.GetComponent<Selectable>().navigation;
        navigation.mode = Navigation.Mode.Automatic;
        backButton.GetComponent<Selectable>().navigation = navigation;
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

    public void OnChangeLevelDropdown(int levelIndex)
    {
        levelName = levelsDropdown.options[levelIndex].text;

        DisplayLevelGhostList(levelName);
    }

    private void DisplayLevelGhostList(string tabName)
    {
        foreach (GameObject ghostList in ghostListTabs)
        {
            ghostList.SetActive(false);
        }

        ghostListTabPerName[tabName].SetActive(true);

        currentFirstSelectableInGrid = firstSelectablesInGrids[tabName];
    }

    private void SetNavigationDown(Selectable selectable)
    {
        Navigation navigation = selectable.navigation;
        navigation.selectOnDown = currentFirstSelectableInGrid;
        selectable.navigation = navigation;
    }
}
