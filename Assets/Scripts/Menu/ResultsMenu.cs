using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsMenu : MonoBehaviour
{
    public static ResultsMenu resultsMenuInstance;

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Slider resultsSlider;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] TweenSettings<float> resultsSliderAnimationSettings;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask ResultsLevel(int result)
    {
        await MenuManager.menuManagerInstance.DisplayMenu(resultsPanel, false, MenuManager.AnimationType.Scale);
        resultsSliderAnimationSettings.endValue = result;
        await Tween.UISliderValue(resultsSlider, resultsSliderAnimationSettings);
        resultsText.text = result.ToString() + "%";
    }
}
