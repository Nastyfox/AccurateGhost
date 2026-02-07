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
            await MenuAnimationManager.menuManagerInstance.DisplayMenu(levelGrid, mainMenu, MenuAnimationManager.AnimationType.Position);
        });

        leaderboardButton.onClick.AddListener(async () => {
            await MenuAnimationManager.menuManagerInstance.DisplayMenu(leaderboardPanel, mainMenu, MenuAnimationManager.AnimationType.Position);
        });

        pseudoButton.onClick.AddListener(async () => {
            await MenuAnimationManager.menuManagerInstance.DisplayMenu(pseudoPanel, mainMenu, MenuAnimationManager.AnimationType.Position);
        });

        savePseudoButton.onClick.AddListener(async () => {
            pseudoMenu.SavePlayerPseudo();
            await MenuAnimationManager.menuManagerInstance.HideMenu(pseudoPanel, mainMenu, MenuAnimationManager.AnimationType.Position);
        });

        optionsButton.onClick.AddListener(async () => {
            OptionsMenu.optionsMenuInstance.SetOptionsMenu();
            await MenuAnimationManager.menuManagerInstance.DisplayMenu(optionsPanel, mainMenu, MenuAnimationManager.AnimationType.Position);
        });

        quitButton.onClick.AddListener(() => {
            LevelLoader.levelLoaderInstance.QuitGame();
        });
    }
}
