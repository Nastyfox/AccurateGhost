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
    [SerializeField] private GameObject levelNameTextPrefab;
    [SerializeField] private GameObject levelNameGrid;

    [SerializeField] private GameObject medalPrefab;
    [SerializeField] private GameObject medalGrid;
    [SerializeField] private List<Sprite> medalSprites;

    [SerializeField] private GameObject completionPrefab;
    [SerializeField] private GameObject completionGrid;

    [SerializeField] private GameObject chronoPrefab;
    [SerializeField] private GameObject chronoGrid;

    private List<Unity.Services.Leaderboards.Models.LeaderboardEntry> scoreData;
    [SerializeField] private Leaderboard leaderboard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid OnEnable()
    {   
        await leaderboard.InitializeLeaderboardService();

        scoreData = await leaderboard.GetScoresWithMetadata("AccurateGhost");

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            if (sceneName.Contains("Level"))
            {
                ESaveSystem.Results levelResults = LeaderboardDataToResults();


                GameObject levelName = Instantiate(levelNameTextPrefab, levelNameGrid.transform);
                levelName.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;

                GameObject medal = Instantiate(medalPrefab, medalGrid.transform);
                switch(levelResults.medal)
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

                GameObject completion = Instantiate(completionPrefab, completionGrid.transform);
                completion.GetComponentInChildren<TextMeshProUGUI>().text = levelResults.completion;

                GameObject chrono = Instantiate(chronoPrefab, chronoGrid.transform);
                chrono.GetComponentInChildren<TextMeshProUGUI>().text = levelResults.chrono;
            }
        }
    }

    private ESaveSystem.Results LeaderboardDataToResults()
    {
        foreach (Unity.Services.Leaderboards.Models.LeaderboardEntry leaderboardEntry in scoreData)
        {
            Debug.Log(leaderboardEntry.Rank.ToString());

            Debug.Log(leaderboardEntry.Score.ToString());

            Debug.Log(leaderboardEntry.Metadata.ToString());
        }

        ESaveSystem.Results results = new ESaveSystem.Results("100%", "00:30:00", "Gold", "Player1");
        return results;
    }
}
