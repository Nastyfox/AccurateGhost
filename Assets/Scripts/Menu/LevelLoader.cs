using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader levelLoaderInstance;

    [SerializeField] private CanvasGroup crossFadeCanvasGroup;

    private GameManager.LevelDifficulty selectedDifficulty;

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levelLoaderInstance == null)
        {
            levelLoaderInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameManager.LevelDifficulty GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }

    public async UniTask LoadLevel(string levelName, string ghostName)
    {
        crossFadeCanvasGroup.gameObject.SetActive(true);

        await MenuAnimationManager.menuManagerInstance.FadeInTransition(crossFadeCanvasGroup);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(levelName);

        while (!sceneLoading.isDone)
        {
            await UniTask.Yield();
        }

        GameManager.gameManagerInstance.SetLevel();

        await OptionsMenu.optionsMenuInstance.SetPauseMenu();

        await MenuAnimationManager.menuManagerInstance.FadeOutTransition(crossFadeCanvasGroup);
        crossFadeCanvasGroup.gameObject.SetActive(false);

        globalDataScriptableObject.ghostName = ghostName;
        globalDataScriptableObject.levelGhostsNames.Clear();
        globalDataScriptableObject.levelGhostsNames = Extensions.PartialMatchKey(globalDataScriptableObject.ghostsDatas, levelName);

        GameManager.LevelDifficulty[] difficulties = (GameManager.LevelDifficulty[])Enum.GetValues(typeof(GameManager.LevelDifficulty));
        for(int i = difficulties.Length - 1; i >= 0; i--)
        {
            globalDataScriptableObject.levelGhostsNames.Insert(0, difficulties[i].ToString());
        }

        try
        {
            selectedDifficulty = (GameManager.LevelDifficulty)System.Enum.Parse(typeof(GameManager.LevelDifficulty), ghostName);
            globalDataScriptableObject.levelDifficulty = selectedDifficulty;
        }
        catch (ArgumentException)
        {
            globalDataScriptableObject.levelDifficulty = GameManager.LevelDifficulty.Easy;
            Debug.Log(ghostName + " is not a member of Level Difficulty enum");
        }

        await GameManager.gameManagerInstance.StartLevel();
    }

    public async UniTask LoadMainMenu()
    {
        crossFadeCanvasGroup.gameObject.SetActive(true);

        await MenuAnimationManager.menuManagerInstance.FadeInTransition(crossFadeCanvasGroup);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("MainMenu");

        while (!sceneLoading.isDone)
        {
            await UniTask.Yield();
        }

        await MenuAnimationManager.menuManagerInstance.FadeOutTransition(crossFadeCanvasGroup);
        crossFadeCanvasGroup.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
