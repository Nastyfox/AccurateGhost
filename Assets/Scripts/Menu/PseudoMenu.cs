using Esper.ESave;
using TMPro;
using UnityEngine;

public class PseudoMenu : MonoBehaviour
{
    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    [SerializeField] private TMP_InputField pseudoInputField;

    [SerializeField] private GameObject pseudoButton;

    private string pseudo = "";

    [SerializeField] private GlobalDataScriptableObject globalDataScriptableObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pseudo = globalDataScriptableObject.pseudo;

        if (!string.IsNullOrEmpty(pseudo))
        {
            pseudoInputField.text = pseudo;
            globalDataScriptableObject.pseudo = pseudo;
        }
        else
        {
            MenuAnimationManager.menuManagerInstance.PseudoScaleButton(pseudoButton);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(pseudo))
        {
            MenuAnimationManager.menuManagerInstance.StopPseudoScaleButton();
        }
    }

    public void SavePlayerPseudo()
    {
        pseudo = pseudoInputField.text;

        globalDataScriptableObject.pseudo = pseudo;
        playerDataSaveSystem.SavePlayerData(pseudo, "Pseudo", playerDataSaveFileSetup);
    }
}
