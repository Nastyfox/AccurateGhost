using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEditor;
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
    [SerializeField] private TweenSettings<float> positionDisplayAnimationSettings;
    [SerializeField] private TweenSettings<float> positionHideAnimationSettings;

    [Header("Scale Animation Settings")]
    [SerializeField] private TweenSettings<float> scaleDisplayAnimationSettings;
    [SerializeField] private TweenSettings<float> scaleHideAnimationSettings;

    [Header("Back Button Animation Settings")]
    [SerializeField] private TweenSettings<float> backButtonDisplayAnimationSettings;
    [SerializeField] private TweenSettings<float> backButtonHideAnimationSettings;

    [Header("Pseudo Button Animation Settings")]
    [SerializeField] private TweenSettings<float> pseudoScaleButtonAnimationSettings;
    private Tween pseudoScaleButtonTween;

    [Header("Countdown Animation Settings")]
    [SerializeField] private TweenSettings<float> countdownScaleAnimationSettings;

    [Header("Cross Fade Animation Settings")]
    [SerializeField] private TweenSettings<float> fadeInAnimationSettings;
    [SerializeField] private TweenSettings<float> fadeOutAnimationSettings;

    [Header("Scale Button Selection Animation Settings")]
    [SerializeField] private TweenSettings<float> selectedButtonAnimationSettings;
    [SerializeField] private TweenSettings<float> deselectedButtonAnimationSettings;

    [Header("Menu elements")]
    [SerializeField] private GameObject backButtonGO;

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
    }

    public async UniTask DisplayMenu(GameObject menuGO, GameObject mainMenuGO, AnimationType animationType)
    {
        backButtonGO.SetActive(true);
        backButtonGO.GetComponent<Button>().onClick.RemoveAllListeners();
        backButtonGO.GetComponent<Button>().onClick.AddListener(async () =>
        {
            if(mainMenuGO ==  null)
            {
                await OptionsMenu.optionsMenuInstance.ResumeFromPause();
            }
            else
            {
                await HideMenu(menuGO, mainMenuGO, animationType);
            }
        });

        menuGO.SetActive(true);

        if (mainMenuGO != null)
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

    public async UniTask HideMenu(GameObject menuGO, GameObject mainMenuGO, AnimationType animationType)
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

        if (mainMenuGO != null)
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

    public async UniTask FadeInTransition(CanvasGroup image)
    {
        await Tween.Alpha(image, fadeInAnimationSettings);
    }

    public async UniTask FadeOutTransition(CanvasGroup image)
    {
        await Tween.Alpha(image, fadeOutAnimationSettings);
    }

    public async UniTask ButtonSelected(GameObject buttonGO)
    {
        await Tween.Scale(buttonGO.GetComponent<RectTransform>(), selectedButtonAnimationSettings);
    }

    public async UniTask ButtonDeselected(GameObject buttonGO)
    {
        await Tween.Scale(buttonGO.GetComponent<RectTransform>(), deselectedButtonAnimationSettings);
    }
}
