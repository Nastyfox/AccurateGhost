using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsListFiller : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private GameObject levelGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i]);

            if(sceneName.Contains("Level"))
            {
                GameObject levelButton = Instantiate(levelButtonPrefab, levelGrid.transform);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
                levelButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(sceneName));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
