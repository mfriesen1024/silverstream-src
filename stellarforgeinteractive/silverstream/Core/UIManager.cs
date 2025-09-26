using System;
using ca.stellarforgeinteractive.silverstream.Core;
using ca.stellarforgeinteractive.silverstream.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] InputActionAsset input;

        // UI Elements
        [SerializeField] Slider ProgressBar;
        [SerializeField] TextMeshProUGUI ProgressText;
        [SerializeField] float MaxValue=100;
        [SerializeField] Slider StaminaBar;
        // Main
        [SerializeField] ButtonHelper play;
        // Pause
        [SerializeField] ButtonHelper resume;
        [SerializeField] ButtonHelper pauseReturn;
        // Results
        [SerializeField] ButtonHelper resultsContinue;
        [SerializeField] ButtonHelper resultsQuit;
        // Upgrade
        [SerializeField] ButtonHelper upgradeContinue;
        // Win
        [SerializeField] ButtonHelper winExit;

        // UI Screens
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject hud;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject resultsMenu;
        [SerializeField] GameObject upgradeMenu;
        [SerializeField] GameObject winScreen;

        void Start()
        {
            // Internal Events
            play.Clicked += PlayClicked;
            resume.Clicked += ResumeClicked;
            pauseReturn.Clicked += PauseReturn;
            resultsContinue.Clicked += ResultsContinue;
            resultsQuit.Clicked += ResultsQuit;
            upgradeContinue.Clicked += UpgradeContinue;
            winExit.Clicked+= WinExit;

            // External inbound events.
            EventSystem.PlayerDied += OnPlayerDeath;
            EventSystem.PlayerWon += OnPlayerWin;
            EventSystem.GameplayStart += OnGameplayStart;
        }

        // Updates max stamina.
        private void OnGameplayStart()
        {
            StaminaBar.maxValue = PlayerStatController.instance.MaxStamina;
            ProgressBar.maxValue = MaxValue;
        }

        private void FixedUpdate()
        {
            StaminaBar.value = PlayerStatController.instance.CurrentStamina;
            ProgressBar.value = PlayerController.distance;
            ProgressText.text = $"Distance: {Mathf.RoundToInt(PlayerController.distance)}m";
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
            HideAll();
            winScreen.SetActive(true);
            
            EventSystem.GameplayEnd();

            throw new NotImplementedException("Winning is not implemented");
        }

        void HideAll()
        {
            mainMenu.SetActive(false);
            hud.SetActive(false);
            pauseMenu.SetActive(false);
            resultsMenu.SetActive(false);
            upgradeMenu.SetActive(false);
            winScreen.SetActive(false);
        }

        private void UpgradeContinue()
        {
            HideAll();
            hud.SetActive(true);

            // Restart gameplay when upgrade menu continue is clicked.
            EventSystem.GameplayStart();
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

        private void WinExit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }
    }
}
