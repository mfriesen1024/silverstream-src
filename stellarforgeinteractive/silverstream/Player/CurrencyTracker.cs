using System;
using System.Linq;
using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;
using UnityEngine.Windows;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public class CurrencyTracker
    {
        public const double TreatMultiplier = 5;
        public const double DistanceMultiplier = 0.5;
        public const double StaminaMultiplier = 0.0025;

        public static CurrencyTracker Instance { get; private set; }

        public int Currency { get; private set; }

        static CurrencyTracker()
        {
            Instance = new CurrencyTracker();
        }

        public static void WipeSave()
        {
            PlayerStatController psc = PlayerStatController.Instance;
            Instance.Currency = 0;
            psc.StaminaLevel = 0;
            psc.DashUnlocked = false;
            psc.WallJumpUnlocked = false;
            
            SaveSystem.Save(psc,Instance);
        }

        private CurrencyTracker()
        {
            EventSystem.TreatCollected += TreatCollected;
            EventSystem.GameplayStart += GameplayStart;
            EventSystem.PlayerDied += PlayerDied;
            if (!GameManager.Initialized)
            {
                EventSystem.Init += Init;
            }
            else
            {
                Init();
            }
            // If/when we have a reset save we'll deal with it here.

            // Update currency when player dies.
            void PlayerDied(int i)
            {
                PlayerStatController psc = PlayerStatController.Instance;
                var staminaUsed = psc.MaxStamina - psc.CurrentStamina;
                Currency += (int)(TreatsThisRun * TreatMultiplier);
                Currency += (int)(PlayerController.Distance * DistanceMultiplier);
                Currency += (int)(staminaUsed * StaminaMultiplier);
                Debug.Log("Currency tracking:\n" +
                          $"Treats: {TreatsThisRun} (x{TreatMultiplier})\n" +
                          $"Distance: {PlayerController.Distance} (x{DistanceMultiplier})\n" +
                          $"Stamina Used: {staminaUsed} (x{StaminaMultiplier})\n" +
                          $"New Value: {Currency}"
                );
                
                // Autosave after death.
                SaveSystem.Save(psc,this);
            }

            void GameplayStart()
            {
                TreatsThisRun = 0;
            }

            void TreatCollected(Vector3 obj)
            {
                TreatsThisRun++;
            }

            // This is just to deal with accessors, otherwise id have the savesystem do it on its own.
            void Init()
            {
                Currency = SaveSystem.Currency;
            }
        }

        public int TreatsThisRun;
        public int[][] prices { get; } =
        {
            new[] { 50, 200, 400, 800 },
            new[] { 250 },
            new[] { 150 }
        };

        public void TryPurchaseUpgrade(int index)
        {
            PlayerStatController psc = PlayerStatController.Instance;
            // If we want the 0th (stamina) upgrade, check the price list for levels.
            // We won't have any other multi-tiered upgrades, so this reduces complexity.
            int level = index == 0 ? psc.StaminaLevel : 0;

            int price = prices[index][level];

            // Dont continue if the price is above money.
            if (price > Currency)
            {
                return;
            }

            Currency -= price;
            switch (index)
            {
                case 0: psc.StaminaLevel++; break;
                case 1: psc.DashUnlocked = true;
                    EventSystem.ShowTutorial(2);
                    break;
                case 2: 
                    psc.WallJumpUnlocked = true;
                    EventSystem.ShowTutorial(1); 
                    break;
                case 3:
                    psc.AirJumpUnlocked = true;
                    break;
                case 4:
                    psc.SecondLifeUnlocked = true;
                    break;
            }

            // Just in case the player quits out after upgrading.
            SaveSystem.Save(psc, this);
        }
    }
}