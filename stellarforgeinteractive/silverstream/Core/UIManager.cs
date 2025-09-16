using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.UI
{
    public class UIManager:MonoBehaviour
    {
        private InputActionAsset input;
        // UI Elements
        [SerializeField] Slider ProgressBar;
        [SerializeField] ButtonHelper play;
        [SerializeField] ButtonHelper resume;
        [SerializeField] ButtonHelper pauseReturn;
        [SerializeField] ButtonHelper resultsContinue;
        [SerializeField] ButtonHelper resultsQuit;
        [SerializeField] ButtonHelper upgradeContinue;
        
        // UI Screens
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject hud;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject resultsMenu;
        [SerializeField] GameObject upgradeMenu;

        void Start()
        {
            // Internal Events
            play.Clicked += PlayClicked;
            resume.Clicked += ResumeClicked;
            pauseReturn.Clicked += PauseReturn;
            resultsContinue.Clicked += ResultsContinue;
            resultsQuit.Clicked += ResultsQuit;
            upgradeContinue.Clicked += UpgradeContinue;

            // External inbound events.
            EventSystem.PlayerDied += OnPlayerDeath;
            EventSystem.PlayerWon += OnPlayerWin;
        }

        private void OnPlayerDeath()
        {
            // When player dies, switch to results and have the event system deal with state stuff.
            HideAll();
            resultsMenu.SetActive(true);

            EventSystem.GameplayEnd();
        }

        private void OnPlayerWin()
        {
            throw new System.NotImplementedException("Winning is not implemented");
        }

        void HideAll()
        {
            mainMenu.SetActive(false);
            hud.SetActive(false);
            pauseMenu.SetActive(false);
            resultsMenu.SetActive(false);
            upgradeMenu.SetActive(false);
        }

        private void UpgradeContinue()
        {
            HideAll();
            hud.SetActive(true);
        }

        private void ResultsQuit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        private void ResultsContinue()
        {
            HideAll();
            upgradeMenu.SetActive(true);
        }

        private void PauseReturn()
        {
            // Hide all but menu
            HideAll();
            mainMenu.SetActive(true);

            EventSystem.GameplayEnd();
        }

        private void ResumeClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayResume();
        }

        private void PlayClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayStart();
        }
    }
}