using Cysharp.Threading.Tasks;
using Esper.ESave;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private List<GameObject> difficultiesGO;
    [SerializeField] private GameObject difficultyList;
    [SerializeField] private TextMeshProUGUI ghostDelayValueText;
    [SerializeField] private GameObject levelDifficultySelectorScreen;

    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    [SerializeField] GlobalDataScriptableObject globalDataScriptableObject;

    private bool ghostBefore = true;
    private bool ghostDuring = true;
    private int ghostDelayInFrames = 0;

    private async UniTaskVoid Start()
    {
        while (!Leaderboard.leaderboardInstance.GetIsInitialized())
        {
            await UniTask.Yield();
        }

        while (difficultyList.transform.childCount > 0)
        {
            DestroyImmediate(difficultyList.transform.GetChild(0).gameObject);
        }

        GameManager.LevelDifficulty[] difficulties = (GameManager.LevelDifficulty[])Enum.GetValues(typeof(GameManager.LevelDifficulty));

        InstantiateDifficultyObject(difficultiesGO[0], GameManager.LevelDifficulty.Easy);

        for (int i = 1; i < difficulties.Length; i++)
        {
            GameManager.LevelDifficulty previousLevelDifficulty = difficulties[i - 1];
            GameManager.LevelDifficulty currentLevelDifficulty = difficulties[i];

            string levelName = SceneManager.GetActiveScene().name + "_" + previousLevelDifficulty;
            Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = await Leaderboard.leaderboardInstance.GetPlayerScoreWithMetadata(levelName);

            if (playerEntry != null)
            {
                InstantiateDifficultyObject(difficultiesGO[i], currentLevelDifficulty);
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

    public void SaveSettings()
    {
        globalDataScriptableObject.displayGhostBefore = ghostBefore;
        globalDataScriptableObject.displayGhostDuring = ghostDuring;
        globalDataScriptableObject.frameOffset = ghostDelayInFrames;
    }

    private void SelectDifficulty(GameManager.LevelDifficulty difficulty)
    {
        string pseudo = playerDataSaveSystem.LoadPlayerData(playerDataSaveFileSetup, "Pseudo");
        levelDifficultySelectorScreen.SetActive(false);
        GameManager.gameManagerInstance.StartLevel(difficulty);
    }

    private void InstantiateDifficultyObject(GameObject difficultyObject, GameManager.LevelDifficulty difficulty)
    {
        GameObject difficultyGO = Instantiate(difficultyObject, difficultyList.transform);
        Button difficultyButton = difficultyGO.GetComponent<Button>();
        difficultyButton.onClick.AddListener(() => SelectDifficulty(difficulty));
    }
}
