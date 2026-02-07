using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardFiller : MonoBehaviour
{
    [SerializeField] private List<Sprite> medalSprites;

    [SerializeField] private Transform levelsContainer;
    [SerializeField] private TMP_Dropdown levelsDropdown;
    [SerializeField] private TMP_Dropdown ghostDropdown;

    [SerializeField] private GameObject leaderboardDataContainerPrefab;
    [SerializeField] private GameObject levelTabButtonPrefab;

    [SerializeField] private GameObject rankTextPrefab;
    [SerializeField] private GameObject pseudoTextPrefab;
    [SerializeField] private GameObject medalPrefab;
    [SerializeField] private GameObject completionPrefab;
    [SerializeField] private GameObject leaderboardEntryPrefab;

    private List<Unity.Services.Leaderboards.Models.LeaderboardEntry> scoreData;

    private double currentScore = 0;
    private int currentRank = 1;
    private Dictionary<double, int> sameScoreCount = new Dictionary<double, int>();

    private string playerID;

    private Dictionary<string, List<string>> ghostListPerLevel = new Dictionary<string, List<string>>();
    private bool firstLevelGhostDropdownSet;
    private Dictionary<string, GameObject> leaderboardTabPerName = new Dictionary<string, GameObject>();
    private List<GameObject> leaderboardTabs = new List<GameObject>();

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    private class LeaderboardResult
    {
        public int rank;
        public double score;
        public string metadata;
        public string playerID;

        public LeaderboardResult(int _rank, double _score, string _metadata, string _playerID)
        {
            rank = _rank;
            score = _score;
            metadata = _metadata;
            playerID = _playerID;
        }
    }

    private async UniTaskVoid Start()
    {
        while (!Leaderboard.leaderboardInstance.GetIsInitialized() || !CloudCodeManager.cloudCodeManagerInstance.IsDataRecovered())
        {
            await UniTask.Yield();
        }
        playerID = Leaderboard.leaderboardInstance.GetPlayerID();

        levelsContainer.DeleteChildren();
        levelsDropdown.ClearOptions();
        ghostDropdown.ClearOptions();

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

                for (int j = 0; j < ghostListPerLevel[sceneName].Count; j++)
                {
                    if(!firstLevelGhostDropdownSet && j == 0)
                    {
                        firstTabSelected = GetTabName(sceneName, ghostListPerLevel[sceneName][j]);
                        SetGhostDropdown(sceneName);
                    }

                    string levelName = GetTabName(sceneName, ghostListPerLevel[sceneName][j]);

                    try
                    {
                        scoreData = await Leaderboard.leaderboardInstance.GetScoresWithMetadata(levelName);
                        List<LeaderboardResult> levelResults = LeaderboardDataToResults();
                        SetNumberSameScore(levelResults);
                        GameObject levelLeaderboard = Instantiate(leaderboardDataContainerPrefab, levelsContainer);
                        leaderboardTabPerName.Add(levelName, levelLeaderboard);
                        leaderboardTabs.Add(levelLeaderboard);

                        if (levelResults.Count > 0)
                        {
                            currentScore = levelResults[0].score;
                        }

                        foreach (LeaderboardResult result in levelResults)
                        {
                            Leaderboard.ScoreMetadata scoreMetadata = JsonUtility.FromJson<Leaderboard.ScoreMetadata>(result.metadata);

                            GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, levelLeaderboard.GetComponentInChildren<VerticalLayoutGroup>().transform);

                            if (result.score != currentScore)
                            {
                                currentRank += sameScoreCount[currentScore];
                                currentScore = result.score;
                            }
                            GameObject rank = Instantiate(rankTextPrefab, leaderboardEntry.transform);
                            rank.GetComponentInChildren<TextMeshProUGUI>().text = (currentRank).ToString();

                            GameObject pseudo = Instantiate(pseudoTextPrefab, leaderboardEntry.transform);
                            pseudo.GetComponentInChildren<TextMeshProUGUI>().text = scoreMetadata.pseudo;

                            GameObject medal = Instantiate(medalPrefab, leaderboardEntry.transform);
                            switch (GetMedalFromScore(result.score))
                            {
                                case "Bronze":
                                    medal.GetComponentInChildren<Image>().sprite = medalSprites[0];
                                    break;
                                case "Silver":
                                    medal.GetComponentInChildren<Image>().sprite = medalSprites[1];
                                    break;
                                case "Gold":
                                    medal.GetComponentInChildren<Image>().sprite = medalSprites[2];
                                    break;
                            }

                            GameObject completion = Instantiate(completionPrefab, leaderboardEntry.transform);
                            completion.GetComponentInChildren<TextMeshProUGUI>().text = result.score.ToString() + "%";

                            if (playerID != result.playerID)
                            {
                                Color leaderboardColor = leaderboardEntry.GetComponent<Image>().color;
                                Color noAlphaColor = new Color(leaderboardColor.r, leaderboardColor.g, leaderboardColor.b, 0f);
                                leaderboardEntry.GetComponent<Image>().color = noAlphaColor;
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("Getting leaderboard scores failed for " + levelName);
                    }
                }
            }
        }
    
        DisplayLevelGhostLeaderboard(firstTabSelected);
    }

    private void OnDisable()
    {
        levelsContainer.DeleteChildren();
        levelsDropdown.ClearOptions();
        ghostDropdown.ClearOptions();
    }

    private List<LeaderboardResult> LeaderboardDataToResults()
    {
        List<LeaderboardResult> result = new List<LeaderboardResult>();
        foreach (Unity.Services.Leaderboards.Models.LeaderboardEntry leaderboardEntry in scoreData)
        {
            result.Add(new LeaderboardResult(leaderboardEntry.Rank, leaderboardEntry.Score, leaderboardEntry.Metadata, leaderboardEntry.PlayerId));
        }

        return result;
    }

    private string GetTabName(string levelName, string ghostName)
    {
        return levelName + "_" + ghostName;
    }

    private void SetNumberSameScore(List<LeaderboardResult> levelResults)
    {
        foreach (var group in levelResults.GroupBy(i => i.score))
        {
            sameScoreCount.Add(group.Key, group.Count());
        }
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
        string levelName = levelsDropdown.options[levelIndex].text;
        SetGhostDropdown(levelName);

        string firstGhostName = ghostListPerLevel[levelName][0];

        string tabName = GetTabName(levelName, firstGhostName);
        
        DisplayLevelGhostLeaderboard(tabName);
    }

    public void OnChangeGhostDropdown(int ghostIndex)
    {
        string levelName = levelsDropdown.options[levelsDropdown.value].text;

        string ghostName = ghostDropdown.options[ghostIndex].text;

        string tabName = GetTabName(levelName, ghostName);

        DisplayLevelGhostLeaderboard(tabName);
    }

    private void DisplayLevelGhostLeaderboard(string tabName)
    {
        foreach(GameObject leaderboardTab in leaderboardTabs)
        {
            leaderboardTab.SetActive(false);
        }

        leaderboardTabPerName[tabName].SetActive(true);
    }

    private string GetMedalFromScore(double score)
    {
        if (score > 20.0)
        {
            return "Gold";
        }
        else if (score > 100.0)
        {
            return "Silver";
        }
        else if (score >= 30.0)
        {
            return "Bronze";
        }
        else
        {
            return "None";
        }
    }
}
