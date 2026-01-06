using Cysharp.Threading.Tasks;
using Esper.ESave;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardFiller : MonoBehaviour
{
    [SerializeField] private List<Sprite> medalSprites;

    [SerializeField] private Transform levelsContainer;
    [SerializeField] private Transform levelsTabsContainer;

    [SerializeField] private GameObject leaderboardDataContainerPrefab;
    [SerializeField] private GameObject levelTabButtonPrefab;
    private List<GameObject> levelLeaderboardsList = new List<GameObject>();
    private List<GameObject> levelTabsButtonsList = new List<GameObject>();

    [SerializeField] private GameObject rankTextPrefab;
    [SerializeField] private GameObject pseudoTextPrefab;
    [SerializeField] private GameObject medalPrefab;
    [SerializeField] private GameObject completionPrefab;
    [SerializeField] private GameObject leaderboardEntryPrefab;

    private List<Unity.Services.Leaderboards.Models.LeaderboardEntry> scoreData;
    [SerializeField] private Leaderboard leaderboard;

    [SerializeField] private List<DifficultyComponent> difficultyComponents;

    private int currentTabIndex = 0;

    private double currentScore = 0;
    private int currentRank = 1;
    private Dictionary<double, int> sameScoreCount = new Dictionary<double, int>();

    private string playerID;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid OnEnable()
    {
        playerID = await leaderboard.InitializeLeaderboardService();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            if (sceneName.Contains("Level"))
            {
                foreach(DifficultyComponent difficulty in difficultyComponents)
                {
                    string levelName = sceneName + "_" + difficulty.levelDifficulty.ToString();

                    scoreData = await leaderboard.GetScoresWithMetadata(levelName);
                    List<LeaderboardResult> levelResults = LeaderboardDataToResults();
                    SetNumberSameScore(levelResults);
                    GameObject levelLeaderboard = Instantiate(leaderboardDataContainerPrefab, levelsContainer);
                    levelLeaderboardsList.Add(levelLeaderboard);
                    GameObject levelTab = Instantiate(levelTabButtonPrefab, levelsTabsContainer);
                    Button levelTabButton = levelTab.GetComponent<Button>();
                    levelTabButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName + " " + difficulty.levelDifficulty.ToString();
                    int capturedIndex = currentTabIndex;
                    levelTabButton.onClick.AddListener(() => SelectLevelTab(capturedIndex));
                    levelTabsButtonsList.Add(levelTab);
                    currentTabIndex++;

                    currentScore = levelResults[0].score;

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

                        if(playerID != result.playerID)
                        {
                            Color leaderboardColor = leaderboardEntry.GetComponent<Image>().color;
                            Color noAlphaColor = new Color(leaderboardColor.r, leaderboardColor.g, leaderboardColor.b, 0f);
                            leaderboardEntry.GetComponent<Image>().color = noAlphaColor;
                        }
                    }
                }
            }
        }

        currentTabIndex = 0;
        SelectLevelTab(0);
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

    private void SetNumberSameScore(List<LeaderboardResult> levelResults)
    {
        foreach (var group in levelResults.GroupBy(i => i.score))
        {
            sameScoreCount.Add(group.Key, group.Count());
        }
    }

    private string GetMedalFromScore(double score)
    {
        if (score >= 80.0)
        {
            return "Gold";
        }
        else if (score >= 50.0)
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

    private void SelectLevelTab(int index)
    {
        foreach (GameObject levelLeaderboard in levelLeaderboardsList)
        {
            levelLeaderboard.SetActive(false);
            levelTabsButtonsList[index].GetComponent<Image>().color = Color.gray;
        }
        levelLeaderboardsList[index].SetActive(true);
        levelTabsButtonsList[index].GetComponent<Image>().color = Color.white;
    }
}
