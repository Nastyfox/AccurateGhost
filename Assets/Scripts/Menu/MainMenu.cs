using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button levelsButton;
    [SerializeField] private GameObject levelGrid;

    [SerializeField] private Button leaderboardButton;
    [SerializeField] private GameObject leaderboardPanel;

    [SerializeField] private Button pseudoButton;
    [SerializeField] private Button savePseudoButton;
    [SerializeField] private GameObject pseudoPanel;

    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject optionsPanel;

    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject mainMenu;

    [SerializeField] private PseudoMenu pseudoMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelsButton.onClick.AddListener(async () => {
            await MenuManager.menuManagerInstance.DisplayMenu(levelGrid, mainMenu, MenuManager.AnimationType.Position);
        });

        leaderboardButton.onClick.AddListener(async () => {
            await MenuManager.menuManagerInstance.DisplayMenu(leaderboardPanel, mainMenu, MenuManager.AnimationType.Position);
        });

        pseudoButton.onClick.AddListener(async () => {
            await MenuManager.menuManagerInstance.DisplayMenu(pseudoPanel, mainMenu, MenuManager.AnimationType.Position);
        });

        savePseudoButton.onClick.AddListener(async () => {
            pseudoMenu.SavePlayerPseudo();
            await MenuManager.menuManagerInstance.HideMenu(pseudoPanel, mainMenu, MenuManager.AnimationType.Position);
        });

        optionsButton.onClick.AddListener(async () => {
            OptionsMenu.optionsMenuInstance.SetOptionsMenu();
            await MenuManager.menuManagerInstance.DisplayMenu(optionsPanel, mainMenu, MenuManager.AnimationType.Position);
        });

        quitButton.onClick.AddListener(() => {
            LevelLoader.levelLoaderInstance.QuitGame();
        });
    }
}
