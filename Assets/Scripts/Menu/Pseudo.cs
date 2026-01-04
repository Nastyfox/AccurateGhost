using Esper.ESave;
using TMPro;
using UnityEngine;
using PrimeTween;

public class Pseudo : MonoBehaviour
{
    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    [SerializeField] private TMP_InputField pseudoInputField;

    [SerializeField] private GameObject pseudoButton;
    [SerializeField] private float animationScaleFactor;
    [SerializeField] private float animationScaleDuration;
    private Tween buttonScaleAnimation;
    private Vector3 pseudoButtonStartLocalScale;

    string pseudo = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pseudoButtonStartLocalScale = pseudoButton.transform.localScale;

        pseudo = playerDataSaveSystem.LoadPlayerData(playerDataSaveFileSetup, "Pseudo");

        if (!string.IsNullOrEmpty(pseudo))
        {
            pseudoInputField.text = pseudo;
        }
        else
        {
            buttonScaleAnimation = Tween.Scale(pseudoButton.transform, endValue: pseudoButton.transform.localScale * animationScaleFactor, duration: animationScaleDuration, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(pseudo))
        {
            buttonScaleAnimation.Stop();
            pseudoButton.transform.localScale = pseudoButtonStartLocalScale;
        }
    }

    public void SavePlayerPseudo()
    {
        pseudo = pseudoInputField.text;

        playerDataSaveSystem.SavePlayerData(pseudo, "Pseudo", playerDataSaveFileSetup);
    }
}
