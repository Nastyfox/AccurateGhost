using Esper.ESave;
using TMPro;
using UnityEngine;

public class Pseudo : MonoBehaviour
{
    [SerializeField] private ESaveSystem playerDataSaveSystem;
    [SerializeField] private SaveFileSetup playerDataSaveFileSetup;

    [SerializeField] private TMP_InputField pseudoInputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string pseudo = playerDataSaveSystem.LoadPlayerData(playerDataSaveFileSetup, "Pseudo");
        if(pseudo != "")
        {
            pseudoInputField.text = pseudo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SavePlayerPseudo()
    {
        string pseudo = pseudoInputField.text;
        playerDataSaveSystem.SavePlayerData(pseudo, "Pseudo", playerDataSaveFileSetup);
    }
}
