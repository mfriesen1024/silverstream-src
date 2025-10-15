using System;
using System.IO;
using ca.stellarforgeinteractive.silverstream.Core;
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
        int dashDrain = 360;

        public static PlayerStatController Instance { get; private set; }

        static PlayerStatController()
        {
            Instance = new PlayerStatController();
        }

        private PlayerStatController()
        {
            if (!GameManager.Initialized)
            {
                EventSystem.Init += Init;
            }
            else
            {
                Init();
            }

            EventSystem.GameplayStart += ReInit;
        }


        public void WipeSave()
        {
            throw new NotImplementedException("Clearing saves is not implemented.");
        }

        public void ReInit()
        {
            CurrentStamina = MaxStamina;
            Debug.Log($"Initialized stamina system, max is {MaxStamina}, current is {CurrentStamina}");
        }

        void Init()
        {
            StaminaLevel = SaveSystem.StaminaLevel;
            DashUnlocked = SaveSystem.DashUnlocked;
            WallJumpUnlocked = SaveSystem.WallJumpUnlocked;
        }

        /// <summary>
        /// The max stamina the player can have.
        /// </summary>
        public int MaxStamina
        {
            get => (int)(baseStamina + baseStamina * (staminaUpgradeValue) * StaminaLevel);
        }

        public int CurrentStamina { get; private set; }

        // Upgrade levels
        public int StaminaLevel;
        public bool DashUnlocked = false;
        public bool WallJumpUnlocked = false;

        internal void UpdateStamina(DrainType action)
        {
            try
            {
                switch (action)
                {
                    case DrainType.Walk: CurrentStamina -= walkDrain; break;
                    case DrainType.Jump: CurrentStamina -= jumpDrain; break;
                    case DrainType.Dash: CurrentStamina -= dashDrain; break;
                    default: throw new InvalidDataException("Unknown DrainType");
                }
            }
            catch (Exception ignored)
            {
                // Debug.LogException(e);
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