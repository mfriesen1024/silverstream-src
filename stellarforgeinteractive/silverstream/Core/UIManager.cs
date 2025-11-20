using System;
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
        readonly string[] reasons = {ReasonTired,ReasonHazard };
        GameObject[] tutorials;
        
        [Header("Core")]
        [SerializeField] InputActionAsset input;
        InputAction pauseIA;
        
        bool fixUpgradeButtons=false;

        // System/Manager refs
        CurrencyTracker ct;

        // UI Elements
        [Header("HUD Elements")] [SerializeField]
        Animator HUDAnimator;
        [SerializeField] Slider progressBar;
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
        [SerializeField] ButtonHelper settingsMMControls;
        [SerializeField] SliderHelper settingsMMSFXVolume, settingsMMOSTVolume;
        // Settings (Pause)
        [SerializeField] ButtonHelper settingsPMReturn;
        [SerializeField] ButtonHelper settingsPMControls;
        [SerializeField] SliderHelper settingsPMSFXVolume, settingsPMOSTVolume;
        // Settings (all)
        // Nothing yet
        // Controls page
        [Header("Controls")] 
        [SerializeField] ButtonHelper controlsReturn;
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

        [SerializeField] ButtonHelper upgradeBuy1, upgradeBuy2, upgradeBuy3, upgradeBuy4, upgradeBuy5;
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
        [SerializeField] GameObject controlsPage;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject resultsMenu;
        [SerializeField] GameObject upgradeMenu;
        [SerializeField] GameObject winScreen;
        [SerializeField] GameObject creditsScreen;
        [SerializeField] GameObject introScreen;
        [SerializeField] GameObject dashIntroScreen;

        void Start()
        {
            // Set important refs
            tutorials = new[] { introScreen, null, dashIntroScreen };
            
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
            settingsMMControls.Clicked += ControlsClicked;
            settingsPMControls.Clicked += ControlsClicked;
            settingsMMSFXVolume.Updated += UpdateSFXVolume;
            settingsPMSFXVolume.Updated += UpdateSFXVolume;
            settingsMMOSTVolume.Updated += UpdateOSTVolume;
            settingsPMOSTVolume.Updated += UpdateOSTVolume;
            settingsMMWipeSave.Clicked += WipeSave;
            settingsMMCredits.Clicked += SettingsMMCredits;
            controlsReturn.Clicked += ControlsReturn;
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
            upgradeBuy4.Clicked = () => { ct.TryPurchaseUpgrade(3); UpdateUpgradeScreenElements(); };
            upgradeBuy5.Clicked = () => { ct.TryPurchaseUpgrade(4); UpdateUpgradeScreenElements(); };

            // External inbound events.
            EventSystem.ShowTutorial += ShowTutorial;
            EventSystem.PlayerDied += OnPlayerDeath;
            EventSystem.PlayerWon += OnPlayerWin;
            EventSystem.GameplayStart += OnGameplayStart;
            EventSystem.PlayerTired += OnPlayerTired;

            // Force showing intro tutorial because this got broken at one point.
            EventSystem.ShowTutorial(0);
            UpdateUpgradeScreenElements();
            upgradeMenu.SetActive(true);
            upgradeMenu.SetActive(false);
        }

        void ShowTutorial(int i)
        {
            tutorials[i]?.SetActive(true);
        }

        void FixedUpdate()
        {
            staminaBar.value = PlayerStatController.Instance.CurrentStamina;
            progressBar.value = PlayerController.Distance;
            resultsDistanceSlider.value = PlayerController.Distance;
            treatsText.text = $"{CurrencyTracker.Instance.TreatsThisRun}";
            CheckForPause();
            
            if(fixUpgradeButtons){UpdateUpgradeScreenElements();}
        }

        #region GameplayEvents

        void OnPlayerTired()
        {
            HUDAnimator.SetBool("lowStamina",true);
        }

        // Updates max stamina.
        void OnGameplayStart()
        {
            staminaBar.maxValue = PlayerStatController.Instance.MaxStamina;
            progressBar.maxValue = maxValue;
            
            HUDAnimator.SetBool("lowStamina",false);
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
            ButtonHelper[] upgradeButtons = { upgradeBuy1, upgradeBuy2, upgradeBuy3, upgradeBuy4, upgradeBuy5 };
            // Avoid recalculating things by creating locals.
            int currency = ct.Currency;
            int sLvl = psc.StaminaLevel;

            upgradeCurrencyCounter.text = $"{currency}";

            // Assign prices to upgrades.
            for (int index = 0; index < upgradeButtons.Length; index++)
            {
                ButtonHelper bh = upgradeButtons[index];
                try
                {
                    // If first upgrade, get stamina level for second index.
                    int lvl = index == 0 ? sLvl : 0;

                    // update levels so we can catch IOR to set something to "maxed"
                    lvl = index == 1 ? psc.WallJumpUnlocked? 1:0 : lvl;
                    lvl = index == 2 ? psc.DashUnlocked? 1:0 : lvl;
                    lvl = index == 3 ? psc.AirJumpUnlocked? 1:0 : lvl;
                    lvl = index == 4 ? psc.SecondLifeUnlocked? 1:0 : lvl;

                    // Assign price to UI components.
                    int price = ct.Prices[index][lvl];
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

            fixUpgradeButtons = false;
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

        void SettingsMMReturn()
        {
            HideAll();
            mainMenu.SetActive(true);
        }

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

        void UpdateOSTVolume(float obj)
        {
            EventSystem.OSTVolumeChanged(obj);
        }

        void UpdateSFXVolume(float obj)
        {
            EventSystem.SFXVolumeChanged(obj);
        }

        void WipeSave()
        {
            FeedbackManager.instance.OnWipeSave();
            EventSystem.ShowTutorial(0);
            CurrencyTracker.WipeSave();
        }

        void ControlsClicked()
        {
            controlsPage.SetActive(true);
        }
        
        #endregion

        #region ControlsEvents

        void ControlsReturn()
        {
            controlsPage.SetActive(false);
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
                Console.WriteLine(e);
                fixUpgradeButtons = true;
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