using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimation : MonoBehaviour
{
    [SerializeField] TweenSettings<float> displayAnimationSettings;
    [SerializeField] TweenSettings<float> hideAnimationSettings;

    [SerializeField] Button levelsButton;
    [SerializeField] GameObject levelGrid;

    [SerializeField] Button leaderboardButton;
    [SerializeField] GameObject leaderboardPanel;

    [SerializeField] Button pseudoButton;
    [SerializeField] GameObject pseudoPanel;

    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject mainMenuGO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelsButton.onClick.AddListener(async () => {
            await DisplayMenu(levelGrid);
        });

        leaderboardButton.onClick.AddListener(async () => {
            await DisplayMenu(leaderboardPanel);
        });

        pseudoButton.onClick.AddListener(async () => {
            await DisplayMenu(pseudoPanel);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask DisplayMenu(GameObject menuGO)
    {
        menuGO.SetActive(true);
        mainMenuGO.SetActive(false);
        await Tween.UIAnchoredPositionX(menuGO.GetComponent<RectTransform>(), displayAnimationSettings);
        backButtonGO.SetActive(true);
        backButtonGO.GetComponent<Button>().onClick.AddListener(async () => {
            await HideMenu(menuGO);
        });
    }

    public async UniTask HideMenu(GameObject menuGO)
    {
        backButtonGO.SetActive(false);
        await Tween.UIAnchoredPositionX(menuGO.GetComponent<RectTransform>(), hideAnimationSettings);
        menuGO.SetActive(false);
        mainMenuGO.SetActive(true);
    }
}
