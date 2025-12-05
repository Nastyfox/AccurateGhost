using Cysharp.Threading.Tasks;
using Esper.ESave;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private List<GameObject> difficulties;
    [SerializeField] private GameObject difficultyList;
    [SerializeField] private Leaderboard leaderboard;
    [SerializeField] private TextMeshProUGUI ghostDelayValueText;
    [SerializeField] private GameObject levelDifficultySelectorScreen;

    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    private bool ghostBefore = true;
    private bool ghostDuring = true;
    private int ghostDelayInFrames = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        await leaderboard.InitializeLeaderboardService();

        InstantiateDifficultyObject(difficulties[0], GameManager.LevelDifficulty.Easy);

        for (int i = 1; i < difficulties.Count; i++)
        {
            GameManager.LevelDifficulty previousLevelDifficulty = difficulties[i - 1].GetComponent<DifficultyComponent>().levelDifficulty;
            GameManager.LevelDifficulty currentLevelDifficulty = difficulties[i].GetComponent<DifficultyComponent>().levelDifficulty;

            string levelName = SceneManager.GetActiveScene().name + "_" + previousLevelDifficulty;
            Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = await leaderboard.GetPlayerScoreWithMetadata(levelName);

            if (playerEntry != null)
            {
                InstantiateDifficultyObject(difficulties[i], currentLevelDifficulty);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGhostBefore(bool value)
    {
        ghostBefore = value;
    }

    public void SetGhostDuring(bool value)
    {
        ghostDuring = value;
    }

    public void ChangeGhostDelayText(float sliderValue)
    {
        ghostDelayInFrames = (int)sliderValue;
        ghostDelayValueText.text = "Ghost Delay : " + ghostDelayInFrames.ToString() + " frames";
    }

    private void SelectDifficulty(GameManager.LevelDifficulty difficulty)
    {
        string pseudo = playerDataSaveSystem.LoadPlayerData(playerDataSaveFileSetup, "Pseudo");
        levelDifficultySelectorScreen.SetActive(false);
        gameManager.StartLevel(difficulty, ghostBefore, ghostDuring, ghostDelayInFrames, pseudo);
    }

    private void InstantiateDifficultyObject(GameObject difficultyObject, GameManager.LevelDifficulty difficultyComponent)
    {
        GameObject difficultyGO = Instantiate(difficultyObject, difficultyList.transform);
        Button difficultyButton = difficultyGO.GetComponent<Button>();
        difficultyButton.onClick.AddListener(() => SelectDifficulty(difficultyComponent));
    }
}
