using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsMenu : MonoBehaviour
{
    public static ResultsMenu resultsMenuInstance;

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Slider resultsSlider;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] TweenSettings<float> resultsSliderAnimationSettings;

    [SerializeField] private Button rewatchButton;
    [SerializeField] private Button replayButton;

    [SerializeField] GlobalDataScriptableObject globalDataScriptableObject;

    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (resultsMenuInstance == null)
        {
            resultsMenuInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        rewatchButton.onClick.AddListener(async () => {
            resultsPanel.SetActive(false);
            await GameManager.gameManagerInstance.RewatchRun();
            resultsPanel.SetActive(true);
        });

        replayButton.onClick.AddListener(async () => {
            resultsPanel.SetActive(false);
            await LevelLoader.levelLoaderInstance.LoadLevel(SceneManager.GetActiveScene().name, globalDataScriptableObject.levelDifficulty);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public async UniTask ResultsLevel(int result)
    {
        player.SetActive(false);
        await MenuManager.menuManagerInstance.DisplayMenu(resultsPanel, false, MenuManager.AnimationType.Scale);
        resultsSliderAnimationSettings.endValue = result;
        await Tween.UISliderValue(resultsSlider, resultsSliderAnimationSettings);
        resultsText.text = result.ToString() + "%";
    }
}
