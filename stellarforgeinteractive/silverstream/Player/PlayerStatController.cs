using System;
using System.IO;
using ca.stellarforgeinteractive.silverstream.Core;
using ca.stellarforgeinteractive.silverstream.Util;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    /// <summary>
    /// Responsible for tracking upgrades, and values related to them.
    /// </summary>
    //[Serializable]
    public class PlayerStatController
    {
        public Action OutOfStamina = EventSystem.DoNothing;

        int baseStamina = 6000;
        float staminaUpgradeValue = 0.5f;
        bool usePassiveDrain = true;
        int passiveDrain = 1;
        int walkDrain = 4;
        int jumpDrain = 180;

        public static PlayerStatController instance { get; private set; }

        static PlayerStatController()
        {
            instance = new PlayerStatController();
        }

        private PlayerStatController()
        {
            EventSystem.GameplayStart += ReInit;

            // Initialize anyway in case it borked.
            // ReInit();
        }

        public void ReInit()
        {
            CurrentStamina = MaxStamina;
            Debug.Log($"Initialized stamina system, max is {MaxStamina}, current is {CurrentStamina}");
        }

        /// <summary>
        /// The max stamina the player can have.
        /// </summary>
        public int MaxStamina
        {
            get => (int)(baseStamina + baseStamina * (1 + staminaUpgradeValue) * staminaLevel);
        }

        public int CurrentStamina { get; private set; }

        // Upgrade levels
        public int staminaLevel;

        internal void UpdateStamina(DrainType[] actions)
        {
            foreach (DrainType a in actions)
            {
                try
                {
                    switch (a)
                    {
                        case DrainType.Walk: CurrentStamina -= walkDrain; break;
                        case DrainType.Jump: CurrentStamina -= jumpDrain; break;
                        default: throw new InvalidDataException("Unknown DrainType");
                    }
                }
                catch (Exception e)
                {
                    // Debug.LogException(e);
                }
            }

            if (usePassiveDrain)
            {
                CurrentStamina -= passiveDrain;
            }

            if (CurrentStamina <= 0)
            {
                OutOfStamina();
            }

            //Debug.Log($"Stamina drain update, stamina now at: {CurrentStamina}");
        }
    }
}