using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager menuManagerInstance;

    [SerializeField] TweenSettings<float> displayAnimationSettings;
    [SerializeField] TweenSettings<float> hideAnimationSettings;

    [SerializeField] Button levelsButton;
    [SerializeField] GameObject levelGrid;

    [SerializeField] Button leaderboardButton;
    [SerializeField] GameObject leaderboardPanel;

    [SerializeField] Button pseudoButton;
    [SerializeField] GameObject pseudoPanel;

    [SerializeField] Button optionsButton;
    [SerializeField] GameObject optionsPanel;

    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject mainMenuGO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (menuManagerInstance == null)
        {
            menuManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        levelsButton.onClick.AddListener(async () => {
            await DisplayMenu(levelGrid, true);
        });

        leaderboardButton.onClick.AddListener(async () => {
            await DisplayMenu(leaderboardPanel, true);
        });

        pseudoButton.onClick.AddListener(async () => {
            await DisplayMenu(pseudoPanel, true);
        });

        optionsButton.onClick.AddListener(async () => {
            OptionsMenu.optionsMenuInstance.SetOptionsMenu();
            await DisplayMenu(optionsPanel, true);
        });
    }

    public async UniTask DisplayMenu(GameObject menuGO, bool mainMenu)
    {
        backButtonGO.SetActive(true);
        backButtonGO.GetComponent<Button>().onClick.RemoveAllListeners();
        backButtonGO.GetComponent<Button>().onClick.AddListener(async () =>
        {
            await HideMenu(menuGO, mainMenu);
        });

        menuGO.SetActive(true);

        if (mainMenu)
        {
            mainMenuGO.SetActive(false);
        }

        await Tween.UIAnchoredPositionX(menuGO.GetComponent<RectTransform>(), displayAnimationSettings);
    }

    public async UniTask HideMenu(GameObject menuGO, bool mainMenu)
    {
        await Tween.UIAnchoredPositionX(menuGO.GetComponent<RectTransform>(), hideAnimationSettings);
        menuGO.SetActive(false);
        backButtonGO.SetActive(false);

        if (mainMenu)
        {
            mainMenuGO.SetActive(true);
        }
    }
}
