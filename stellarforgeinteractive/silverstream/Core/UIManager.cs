using System;
using System.IO;
using ca.stellarforgeinteractive.silverstream.Player;
using ca.stellarforgeinteractive.silverstream.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class UIManager:MonoBehaviour
    {
        const string
            ReasonTired = "Rusty is too tired to continue.",
            ReasonHazard = "Rusty encountered a hazard.",
            ReasonError = "Rusty broke.";
        readonly string[] reasons = new[] {ReasonTired,ReasonHazard };
        
        [Header("Core")]
        [SerializeField] InputActionAsset input;
        InputAction pauseIA;

        // System/Manager refs
        CurrencyTracker ct;

        // UI Elements
        [Header("HUD Elements")]
        [SerializeField] Slider progressBar;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] float maxValue = 100;
        [SerializeField] Slider staminaBar;
        [SerializeField] TextMeshProUGUI treatsText;
        // Main
        [Header("Main Menu")]
        [SerializeField] ButtonHelper play;
        [SerializeField] ButtonHelper settingsMM;
        [SerializeField] ButtonHelper quit;
        // Settings (Main)
        [Header("Settings")]
        [SerializeField] ButtonHelper settingsMMReturn;
        [SerializeField] ButtonHelper settingsMMWipeSave;
        [SerializeField] ButtonHelper settingsMMCredits;
        // Settings (Pause)
        [SerializeField] ButtonHelper settingsPMReturn;
        // Settings (all)
        // Nothing yet
        // Pause
        [Header("Pause")]
        [SerializeField] ButtonHelper resume;
        [SerializeField] ButtonHelper settingsPM;
        [SerializeField] ButtonHelper pauseReturn;
        // Results
        [Header("Results")]
        [SerializeField] ButtonHelper resultsContinue;
        [SerializeField] ButtonHelper resultsQuit;
        [SerializeField] TextMeshProUGUI resultsHowRustyFailed;
        [SerializeField] Slider resultsDistanceSlider;
        [SerializeField] TextMeshProUGUI distDescriptor, stamDescriptor, treatsDescriptor, runTotalDescriptor, overallTotalDescriptor;
        [SerializeField] TextMeshProUGUI distValue,stamValue,treatsValue,runTotalValue,overallTotalValue;
        // Upgrade
        [Header("Shop")]
        [SerializeField] ButtonHelper upgradeContinue;
        [SerializeField] ButtonHelper upgradeBuy1, upgradeBuy2, upgradeBuy3;
        [SerializeField] TextMeshProUGUI upgradeCurrencyCounter;
        // Win
        [Header("Win Screen")]
        [SerializeField] ButtonHelper winExit;
        
        [Header("Misc")]
        // Credits
        [SerializeField] ButtonHelper creditsQuit;
        // Tutorial bunk
        [SerializeField] ButtonHelper introQuit;
        [SerializeField] ButtonHelper dashIntroQuit;

        // UI Screens
        [Header("UI Screens")]
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject hud;
        [SerializeField] GameObject settingsMenuMM;
        [SerializeField] GameObject settingsMenuPM;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject resultsMenu;
        [SerializeField] GameObject upgradeMenu;
        [SerializeField] GameObject winScreen;
        [SerializeField] GameObject creditsScreen;
        [SerializeField] GameObject introScreen;
        [SerializeField] GameObject dashIntroScreen;

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
            settingsMMWipeSave.Clicked += CurrencyTracker.WipeSave;
            settingsMMCredits.Clicked += SettingsMMCredits;
            settingsPMReturn.Clicked += SettingsPMReturn;
            resultsContinue.Clicked += ResultsContinue;
            resultsQuit.Clicked += ResultsQuit;
            upgradeContinue.Clicked += UpgradeContinue;
            winExit.Clicked += WinExit;
            creditsQuit.Clicked += SettingsMMReturn; // I'm feeling lazy. Also might be more memory efficient.

            introQuit.Clicked += IntroClose;
            dashIntroQuit.Clicked += DashIntroClose;

            // Purely for ease of use, I'm going to lambda the upgrade buttons. This is generally bad practice.
            ct = CurrencyTracker.Instance;
            upgradeBuy1.Clicked = () => { ct.TryPurchaseUpgrade(0); UpdateUpgradeScreenElements(); };
            upgradeBuy2.Clicked = () => { ct.TryPurchaseUpgrade(1); UpdateUpgradeScreenElements(); };
            upgradeBuy3.Clicked = () => { ct.TryPurchaseUpgrade(2); UpdateUpgradeScreenElements(); };

            // External inbound events.
            EventSystem.PlayerDied += OnPlayerDeath;
            EventSystem.PlayerWon += OnPlayerWin;
            EventSystem.GameplayStart += OnGameplayStart;

            UpdateUpgradeScreenElements();
            upgradeMenu.SetActive(true);
            upgradeMenu.SetActive(false);
        }

        void FixedUpdate()
        {
            staminaBar.value = PlayerStatController.Instance.CurrentStamina;
            progressBar.value = PlayerController.Distance;
            resultsDistanceSlider.value = PlayerController.Distance;
            progressText.text = $"Distance: {Mathf.RoundToInt(PlayerController.Distance)}m";
            treatsText.text = $"Treats: {CurrencyTracker.Instance.TreatsThisRun}";
            CheckForPause();
        }

        #region GameplayEvents

        // Updates max stamina.
        void OnGameplayStart()
        {
            staminaBar.maxValue = PlayerStatController.Instance.MaxStamina;
            progressBar.maxValue = maxValue;
        }

        void OnPlayerDeath(int i)
        {
            // When player dies, switch to results and have the event system deal with state stuff.
            HideAll();
            resultsMenu.SetActive(true);

            UpdateResultsScreenElements(i);

            EventSystem.GameplayEnd();
        }

        void OnPlayerWin()
        {
            HideAll();
            winScreen.SetActive(true);

            EventSystem.GameplayEnd();

            // throw new NotImplementedException("Winning is not implemented");
        }

        #endregion

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

        void UpdateResultsScreenElements(int i)
        {
            PlayerStatController psc = PlayerStatController.Instance;
            var staminaUsed = psc.MaxStamina - psc.CurrentStamina;
            string distDescriptorText =
                $"Distance Travelled: ({Mathf.RoundToInt(PlayerController.Distance)} x {CurrencyTracker.DistanceMultiplier}):";
            string stamDescriptorText =
                $"Stamina Used: ({Mathf.RoundToInt(staminaUsed / 1000)}k x {CurrencyTracker.StaminaMultiplier}):";
            string treatsDescriptorText =
                $"Treats Collected: ({ct.TreatsThisRun} x {CurrencyTracker.TreatMultiplier}):";

            try { resultsHowRustyFailed.text = reasons[i];}
            catch(IndexOutOfRangeException ignored) { resultsHowRustyFailed.text = ReasonError; }
            distDescriptor.text = distDescriptorText;
            stamDescriptor.text = stamDescriptorText;
            treatsDescriptor.text = treatsDescriptorText;

            // TODO: Yeah i need to find a cleaner alternative to recalcing everything but im lazy.
            int treatsValue = (int)(ct.TreatsThisRun * CurrencyTracker.TreatMultiplier);
            int distValue = (int)(PlayerController.Distance * CurrencyTracker.DistanceMultiplier);
            int stamValue = (int)(staminaUsed * CurrencyTracker.StaminaMultiplier);

            this.distValue.text = distValue.ToString();
            this.stamValue.text = stamValue.ToString();
            this.treatsValue.text = treatsValue.ToString();

            int rTotal = treatsValue + distValue + stamValue;
            runTotalValue.text = rTotal.ToString();

            overallTotalValue.text = ct.Currency.ToString();
        }

        void UpdateUpgradeScreenElements()
        {
            Debug.Log("Updating upgrade screen elements");
            PlayerStatController psc = PlayerStatController.Instance;
            ButtonHelper[] upgradeButtons = { upgradeBuy1, upgradeBuy2, upgradeBuy3 };
            // Avoid recalculating things by creating locals.
            int currency = ct.Currency;
            int sLvl = psc.StaminaLevel;

            upgradeCurrencyCounter.text = $"Currency: {currency}";

            // Assign prices to upgrades.
            for (int index = 0; index < upgradeButtons.Length; index++)
            {
                ButtonHelper bh = upgradeButtons[index];
                try
                {
                    // If first upgrade, get stamina level for second index.
                    int lvl = index == 0 ? sLvl : 0;

                    // update levels so we can catch IOR to set something to "maxed"
                    lvl = index == 1 ? psc.DashUnlocked? 1:0 : lvl;
                    lvl = index == 2 ? psc.WallJumpUnlocked? 1:0 : lvl;

                    // Assign price to UI components.
                    int price = ct.prices[index][lvl];
                    if (bh.Text)
                    {
                        SetPrice();
                    }
                    else
                    {
                        bh.TextInit = () =>
                        {
                            SetPrice();
                            bh.TextInit = null;
                        };
                    }

                    void SetPrice()
                    {
                        bh.Text.text = $"Buy ({price})";
                        bh.Button.interactable = price <= currency;
                    }
                }
                catch (IndexOutOfRangeException ignored)
                {
                    bh.Text.text = "Max level!";
                    bh.Button.interactable = false;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
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
            creditsScreen.SetActive(false);
        }

        #region SettingsEvents

        void SettingsPMReturn()
        {
            HideAll();
            pauseMenu.SetActive(true);
        }

        void SettingsMMCredits()
        {
            HideAll();
            creditsScreen.SetActive(true);
        }

        void SettingsMMReturn()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        #endregion

        #region UpgradeEvents

        void UpgradeContinue()
        {
            HideAll();
            hud.SetActive(true);

            // Restart gameplay when upgrade menu continue is clicked.
            EventSystem.GameplayStart();
        }

        #endregion

        #region ResultsEvents

        void ResultsQuit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        void ResultsContinue()
        {
            HideAll();
            upgradeMenu.SetActive(true);

            try
            {
                // Run this at the end in case it borks, but eventually we should task run this before hideall.
                UpdateUpgradeScreenElements();
                //break;
            }
            catch (NullReferenceException e)
            {
                // Console.WriteLine(e);
            }
        }

        #endregion

        #region PauseEvents

        void PauseReturn()
        {
            // Hide all but menu
            HideAll();
            mainMenu.SetActive(true);

            EventSystem.GameplayEnd();
        }

        void SettingsPMClicked()
        {
            HideAll();
            settingsMenuPM.SetActive(true);
        }

        void ResumeClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayResume();
        }

        #endregion

        #region MainMenuEvents

        void PlayClicked()
        {
            // Hide all but hud.
            HideAll();
            hud.SetActive(true);

            EventSystem.GameplayStart();
        }

        void SettingsMMClicked()
        {
            HideAll();
            settingsMenuMM.SetActive(true);
        }

        void QuitClicked()
        {
            Application.Quit(0);
            throw new DebugException("Quit pressed.");
        }

        #endregion

        #region MiscEvents

        void WinExit()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

        void DashIntroClose()
        {
            dashIntroScreen.SetActive(false);
        }

        void IntroClose()
        {
            introScreen.SetActive(false);

            // Reset things in case player waited too long.
            EventSystem.GameplayStart();
        }

        #endregion

    }
}