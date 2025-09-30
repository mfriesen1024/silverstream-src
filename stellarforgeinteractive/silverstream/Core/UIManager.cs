using System;
using ca.stellarforgeinteractive.silverstream.Player;
using ca.stellarforgeinteractive.silverstream.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] InputActionAsset input;
        InputAction pauseIA;

        // UI Elements
        [SerializeField] Slider progressBar;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] float maxValue=100;
        [SerializeField] Slider staminaBar;
        [SerializeField] TextMeshProUGUI treatsText;
        // Main
        [SerializeField] ButtonHelper play;
        [SerializeField] ButtonHelper settingsMM;
        [SerializeField] ButtonHelper quit;
        // Settings (Main)
        [SerializeField] ButtonHelper settingsMMReturn;
        // Settings (Pause)
        [SerializeField] ButtonHelper settingsPMReturn;
        // Settings (all)
        // Nothing yet
        // Pause
        [SerializeField] ButtonHelper resume;
        [SerializeField] ButtonHelper settingsPM;
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
        [SerializeField] GameObject settingsMenuMM;
        [SerializeField] GameObject settingsMenuPM;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject resultsMenu;
        [SerializeField] GameObject upgradeMenu;
        [SerializeField] GameObject winScreen;

        void Start()
        {
            // Input stuff
            pauseIA = input.FindAction("pause");
            
            // Internal Events
            play.Clicked += PlayClicked;
            settingsMM.Clicked += SettingsMMClicked;
            quit.Clicked += QuitClicked;
            resume.Clicked += ResumeClicked;
            settingsPM.Clicked += SettingsPMClicked;
            pauseReturn.Clicked += PauseReturn;
            settingsMMReturn.Clicked += SettingsMMReturn;
            settingsPMReturn.Clicked += SettingsPMReturn;
            resultsContinue.Clicked += ResultsContinue;
            resultsQuit.Clicked += ResultsQuit;
            upgradeContinue.Clicked += UpgradeContinue;
            winExit.Clicked+= WinExit;

            // External inbound events.
            EventSystem.PlayerDied += OnPlayerDeath;
            EventSystem.PlayerWon += OnPlayerWin;
            EventSystem.GameplayStart += OnGameplayStart;
        }

        void QuitClicked()
        {
            Application.Quit(0);
            throw new DebugException("Quit pressed.");
        }

        // Updates max stamina.
        void OnGameplayStart()
        {
            staminaBar.maxValue = PlayerStatController.Instance.MaxStamina;
            progressBar.maxValue = maxValue;
        }

        void FixedUpdate()
        {
            staminaBar.value = PlayerStatController.Instance.CurrentStamina;
            progressBar.value = PlayerController.Distance;
            progressText.text = $"Distance: {Mathf.RoundToInt(PlayerController.Distance)}m";
            treatsText.text = $"Treats: {CurrencyTracker.Instance.TreatsThisRun}";
            CheckForPause();
        }

        void CheckForPause()
        {
            try
            {
                if (pauseIA.ReadValue<float>() > 0 && GameManager.Instance.GameplayRunning)
                {
                    HideAll();
                    pauseMenu.SetActive(true);

                    EventSystem.GameplayPause();
                }
            }
            catch (Exception ignored)
            {
                // Debug.LogException(ignored);
            }
        }

        void OnPlayerDeath()
        {
            // When player dies, switch to results and have the event system deal with state stuff.
            HideAll();
            resultsMenu.SetActive(true);

            EventSystem.GameplayEnd();
        }

        void OnPlayerWin()
        {
            HideAll();
            winScreen.SetActive(true);
            
            EventSystem.GameplayEnd();

            // throw new NotImplementedException("Winning is not implemented");
        }

        void HideAll()
        {
            mainMenu.SetActive(false);
            hud.SetActive(false);
            settingsMenuMM.SetActive(false);
            settingsMenuPM.SetActive(false);
            pauseMenu.SetActive(false);
            resultsMenu.SetActive(false);
            upgradeMenu.SetActive(false);
            winScreen.SetActive(false);
        }

        void SettingsPMReturn()
        {
            HideAll();
            pauseMenu.SetActive(true);
        }

        void SettingsMMReturn()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        void SettingsPMClicked()
        {
            HideAll();
            settingsMenuPM.SetActive(true);
        }

        void SettingsMMClicked()
        {
            HideAll();
            settingsMenuMM.SetActive(true);
        }

        void UpgradeContinue()
        {
            HideAll();
            hud.SetActive(true);

            // Restart gameplay when upgrade menu continue is clicked.
            EventSystem.GameplayStart();
        }

        void ResultsQuit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        void ResultsContinue()
        {
            HideAll();
            upgradeMenu.SetActive(true);
        }

        void PauseReturn()
        {
            // Hide all but menu
            HideAll();
            mainMenu.SetActive(true);

            EventSystem.GameplayEnd();
        }

        void ResumeClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayResume();
        }

        void PlayClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayStart();
        }

        void WinExit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }
    }
}
