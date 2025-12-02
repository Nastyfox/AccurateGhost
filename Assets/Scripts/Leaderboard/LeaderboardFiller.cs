using Cysharp.Threading.Tasks;
using Esper.ESave;
using System.Collections.Generic;
using System.IO;
using TMPro;
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

    private List<Unity.Services.Leaderboards.Models.LeaderboardEntry> scoreData;
    [SerializeField] private Leaderboard leaderboard;

    [SerializeField] private List<DifficultyComponent> difficultyComponents;

    private int currentTabIndex = 0;

    private class LeaderboardResult
    {
        public int rank;
        public double score;
        public string metadata;

        public LeaderboardResult(int _rank, double _score, string _metadata)
        {
            rank = _rank;
            score = _score;
            metadata = _metadata;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid OnEnable()
    {   
        await leaderboard.InitializeLeaderboardService();

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
                    GameObject levelLeaderboard = Instantiate(leaderboardDataContainerPrefab, levelsContainer);
                    levelLeaderboardsList.Add(levelLeaderboard);
                    LeaderboardDataContainer leaderboardDataElements = levelLeaderboard.GetComponent<LeaderboardDataContainer>();
                    GameObject levelTab = Instantiate(levelTabButtonPrefab, levelsTabsContainer);
                    Button levelTabButton = levelTab.GetComponent<Button>();
                    levelTabButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName + " " + difficulty.levelDifficulty.ToString();
                    int capturedIndex = currentTabIndex;
                    levelTabButton.onClick.AddListener(() => SelectLevelTab(capturedIndex));
                    levelTabsButtonsList.Add(levelTab);
                    currentTabIndex++;

                    foreach (LeaderboardResult result in levelResults)
                    {
                        Leaderboard.ScoreMetadata scoreMetadata = JsonUtility.FromJson<Leaderboard.ScoreMetadata>(result.metadata);

                        GameObject rank = Instantiate(rankTextPrefab, leaderboardDataElements.rankGrid.transform);
                        rank.GetComponentInChildren<TextMeshProUGUI>().text = (result.rank + 1).ToString();

                        GameObject pseudo = Instantiate(pseudoTextPrefab, leaderboardDataElements.pseudoGrid.transform);
                        pseudo.GetComponentInChildren<TextMeshProUGUI>().text = scoreMetadata.pseudo;

                        GameObject medal = Instantiate(medalPrefab, leaderboardDataElements.medalGrid.transform);
                        switch (GetMedalFromScore(result.score))
                        {
                            case "Bronze":
                                medal.GetComponent<Image>().sprite = medalSprites[0];
                                break;
                            case "Silver":
                                medal.GetComponent<Image>().sprite = medalSprites[1];
                                break;
                            case "Gold":
                                medal.GetComponent<Image>().sprite = medalSprites[2];
                                break;
                        }

                        GameObject completion = Instantiate(completionPrefab, leaderboardDataElements.completionGrid.transform);
                        completion.GetComponentInChildren<TextMeshProUGUI>().text = result.score.ToString() + "%";
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
            result.Add(new LeaderboardResult(leaderboardEntry.Rank, leaderboardEntry.Score, leaderboardEntry.Metadata));
        }

        return result;
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
