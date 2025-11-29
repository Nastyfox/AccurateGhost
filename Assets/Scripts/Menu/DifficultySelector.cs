using Esper.ESave;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private List<GameObject> difficulties;
    [SerializeField] private GameObject difficultyList;
    [SerializeField] private ESaveSystem eSaveSystem;
    [SerializeField] private SaveFileSetup resultsFileSetup;
    [SerializeField] private TextMeshProUGUI ghostDelayValueText;
    [SerializeField] private GameObject levelDifficultySelectorScreen;
    [SerializeField] private TextMeshProUGUI pseudoText;

    private bool ghostBefore = true;
    private bool ghostDuring = true;
    private int ghostDelayInFrames = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject difficulty in difficulties)
        {
            DifficultyComponent difficultyComponent = difficulty.GetComponent<DifficultyComponent>();
            GameManager.LevelDifficulty levelDifficulty = difficultyComponent.levelDifficulty;
            string levelName = SceneManager.GetActiveScene().name + "_" + levelDifficulty;
            ESaveSystem.Results savedResult = eSaveSystem.LoadResults(levelName, resultsFileSetup);
            bool validResult = !string.IsNullOrEmpty(savedResult.completion) &&
                               !string.IsNullOrEmpty(savedResult.chrono) &&
                               !string.IsNullOrEmpty(savedResult.medal);

            if (validResult || levelDifficulty == GameManager.LevelDifficulty.Easy)
            {
                GameObject difficultyGO = Instantiate(difficulty, difficultyList.transform);
                Button difficultyButton = difficultyGO.GetComponent<Button>();
                difficultyButton.onClick.AddListener(() => SelectDifficulty(difficultyComponent));
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

    private void SelectDifficulty(DifficultyComponent difficulty)
    {
        string pseudo = pseudoText.text;
        levelDifficultySelectorScreen.SetActive(false);
        gameManager.StartLevel(difficulty.levelDifficulty, ghostBefore, ghostDuring, ghostDelayInFrames, pseudo);
    }
}
