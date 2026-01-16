using Cysharp.Threading.Tasks;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsListFiller : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private GameObject levelGrid;

    private bool isSelected = false;

    private async UniTaskVoid Start()
    {
        while(!Leaderboard.leaderboardInstance.GetIsInitialized())
        {
            await UniTask.Yield();
        }

        while (levelGrid.transform.childCount > 0)
        {
            DestroyImmediate(levelGrid.transform.GetChild(0).gameObject);
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        GameObject myEventSystem = GameObject.Find("EventSystem");

        GameManager.LevelDifficulty[] difficulties = (GameManager.LevelDifficulty[])Enum.GetValues(typeof(GameManager.LevelDifficulty));

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            GameObject levelButton = null;

            if (sceneName.Contains("Level"))
            {
                levelButton = Instantiate(levelButtonPrefab, levelGrid.transform);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName + "\n" + difficulties[0];
                levelButton.GetComponent<Button>().onClick.AddListener(async () => {
                    await LevelLoader.levelLoaderInstance.LoadLevel(sceneName, difficulties[0]);
                });

                if (!isSelected && myEventSystem != null)
                {
                    myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(levelButton);
                    isSelected = true;
                }

                for (int j = 1; j < difficulties.Length; j++)
                {
                    GameManager.LevelDifficulty previousLevelDifficulty = difficulties[j - 1];
                    GameManager.LevelDifficulty currentLevelDifficulty = difficulties[j];

                    string levelName = sceneName + "_" + previousLevelDifficulty;
                    Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = await Leaderboard.leaderboardInstance.GetPlayerScoreWithMetadata(levelName);

                    if (playerEntry != null)
                    {
                        levelButton = Instantiate(levelButtonPrefab, levelGrid.transform);
                        levelButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName + "\n" + difficulties[j];
                        levelButton.GetComponent<Button>().onClick.AddListener(async () => {
                            await LevelLoader.levelLoaderInstance.LoadLevel(sceneName, difficulties[j]);
                        });

                        if (!isSelected && myEventSystem != null)
                        {
                            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(levelButton);
                            isSelected = true;
                        }
                    }
                    
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
