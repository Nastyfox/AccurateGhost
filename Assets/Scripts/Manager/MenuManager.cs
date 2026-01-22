using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static MenuManager;

public class MenuManager : MonoBehaviour
{
    public enum AnimationType
    {
        Position,
        Scale
    }

    public static MenuManager menuManagerInstance;

    [Header("Position Animation Settings")]
    [SerializeField] TweenSettings<float> positionDisplayAnimationSettings;
    [SerializeField] TweenSettings<float> positionHideAnimationSettings;

    [Header("Scale Animation Settings")]
    [SerializeField] TweenSettings<float> scaleDisplayAnimationSettings;
    [SerializeField] TweenSettings<float> scaleHideAnimationSettings;

    [Header("Back Button Animation Settings")]
    [SerializeField] TweenSettings<float> backButtonDisplayAnimationSettings;
    [SerializeField] TweenSettings<float> backButtonHideAnimationSettings;

    [Header("Pseudo Button Animation Settings")]
    [SerializeField] TweenSettings<float> pseudoScaleButtonAnimationSettings;
    private Tween pseudoScaleButtonTween;

    [Header("Countdown Animation Settings")]
    [SerializeField] TweenSettings<float> countdownScaleAnimationSettings;

    [Header("Menu elements")]
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
            await DisplayMenu(levelGrid, true, AnimationType.Position);
        });

        leaderboardButton.onClick.AddListener(async () => {
            await DisplayMenu(leaderboardPanel, true, AnimationType.Position);
        });

        pseudoButton.onClick.AddListener(async () => {
            await DisplayMenu(pseudoPanel, true, AnimationType.Position);
        });

        optionsButton.onClick.AddListener(async () => {
            OptionsMenu.optionsMenuInstance.SetOptionsMenu();
            await DisplayMenu(optionsPanel, true, AnimationType.Position);
        });
    }

    public async UniTask DisplayMenu(GameObject menuGO, bool mainMenu, AnimationType animationType)
    {
        backButtonGO.SetActive(true);
        backButtonGO.GetComponent<Button>().onClick.RemoveAllListeners();
        backButtonGO.GetComponent<Button>().onClick.AddListener(async () =>
        {
            await HideMenu(menuGO, mainMenu, animationType);
        });

        menuGO.SetActive(true);

        if (mainMenu)
        {
            mainMenuGO.SetActive(false);
        }

        backButtonDisplayAnimationSettings.settings.useUnscaledTime = positionDisplayAnimationSettings.settings.useUnscaledTime;

        switch (animationType)
        {
            case AnimationType.Position:
                await Sequence.Create(useUnscaledTime: true)
                    .Group(Tween.UIAnchoredPositionY(menuGO.GetComponent<RectTransform>(), positionDisplayAnimationSettings))
                    .Group(Tween.UIAnchoredPositionY(backButtonGO.GetComponent<RectTransform>(), backButtonDisplayAnimationSettings));
                break;
            case AnimationType.Scale:
                await Tween.Scale(menuGO.GetComponent<RectTransform>(), scaleDisplayAnimationSettings);
                break;
        }
    }

    public async UniTask HideMenu(GameObject menuGO, bool mainMenu, AnimationType animationType)
    {
        Time.timeScale = 1f;

        switch(animationType)
        {
            case AnimationType.Position:
                await Sequence.Create(useUnscaledTime: true)
                    .Group(Tween.UIAnchoredPositionY(menuGO.GetComponent<RectTransform>(), positionHideAnimationSettings))
                    .Group(Tween.UIAnchoredPositionY(backButtonGO.GetComponent<RectTransform>(), backButtonHideAnimationSettings));
                break;
            case AnimationType.Scale:
                await Tween.Scale(menuGO.GetComponent<RectTransform>(), scaleHideAnimationSettings);
                break;
        }

        menuGO.SetActive(false);

        if (mainMenu)
        {
            mainMenuGO.SetActive(true);
        }
    }

    public void PseudoScaleButton(GameObject menuGO)
    {
        pseudoScaleButtonTween = Tween.Scale(menuGO.GetComponent<RectTransform>(), pseudoScaleButtonAnimationSettings);
    }

    public void StopPseudoScaleButton()
    {
        if(pseudoScaleButtonTween.isAlive)
        {
            pseudoScaleButtonTween.Stop();
        }
    }

    public async UniTask CountdownScaleButton(GameObject menuGO, float duration)
    {
        countdownScaleAnimationSettings.settings.duration = duration;
        await Tween.Scale(menuGO.GetComponent<RectTransform>(), countdownScaleAnimationSettings);
    }
}
