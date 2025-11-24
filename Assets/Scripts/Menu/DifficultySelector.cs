using Esper.ESave;
using System.Collections.Generic;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject difficulty in difficulties)
        {
            DifficultyComponent difficultyComponent = difficulty.GetComponent<DifficultyComponent>();
            GameManager.LevelDifficulty levelDifficulty = difficultyComponent.levelDifficulty;
            string savedRun = eSaveSystem.LoadRun(resultsFileSetup, levelDifficulty, SceneManager.GetActiveScene().name);
            if (savedRun != null)
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

    public void SelectDifficulty(DifficultyComponent difficulty)
    {
        gameManager.SetLevelDifficulty(difficulty.levelDifficulty);
    }
}
