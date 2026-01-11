using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader levelLoaderInstance;

    [SerializeField] private CanvasGroup crossFadeCanvasGroup;
    [SerializeField] private TweenSettings<float> fadeInAnimationSettings;
    [SerializeField] private TweenSettings<float> fadeOutAnimationSettings;

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

    public async UniTask LoadLevel(string levelName)
    {
        crossFadeCanvasGroup.gameObject.SetActive(true);

        await Tween.Alpha(crossFadeCanvasGroup, fadeInAnimationSettings);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(levelName);

        while (!sceneLoading.isDone)
        {
            await UniTask.Yield();
        }

        await Tween.Alpha(crossFadeCanvasGroup, fadeOutAnimationSettings);

        crossFadeCanvasGroup.gameObject.SetActive(false);
    }
}
