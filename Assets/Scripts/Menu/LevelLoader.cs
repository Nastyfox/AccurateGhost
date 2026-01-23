using Cysharp.Threading.Tasks;
using PrimeTween;
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

    public async UniTask LoadLevel(string levelName, GameManager.LevelDifficulty difficulty)
    {
        selectedDifficulty = difficulty;

        crossFadeCanvasGroup.gameObject.SetActive(true);

        await MenuManager.menuManagerInstance.FadeInTransition(crossFadeCanvasGroup);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(levelName);

        while (!sceneLoading.isDone)
        {
            await UniTask.Yield();
        }

        GameManager.gameManagerInstance.SetLevel();

        await OptionsMenu.optionsMenuInstance.SetPauseMenu();

        await MenuManager.menuManagerInstance.FadeOutTransition(crossFadeCanvasGroup);
        crossFadeCanvasGroup.gameObject.SetActive(false);

        globalDataScriptableObject.levelDifficulty = difficulty;
        await GameManager.gameManagerInstance.StartLevel();
    }

    public async UniTask LoadMainMenu()
    {
        crossFadeCanvasGroup.gameObject.SetActive(true);

        await MenuManager.menuManagerInstance.FadeInTransition(crossFadeCanvasGroup);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("MainMenu");

        while (!sceneLoading.isDone)
        {
            await UniTask.Yield();
        }

        await MenuManager.menuManagerInstance.FadeOutTransition(crossFadeCanvasGroup);
        crossFadeCanvasGroup.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
