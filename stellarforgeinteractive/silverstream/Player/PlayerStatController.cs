using System;
using System.IO;
using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    /// <summary>
    /// Responsible for tracking upgrades, and values related to them.
    /// </summary>
    [Serializable]
    public class PlayerStatController
    {
        public Action OutofStamina = EventSystem.DoNothing;
        
        [Header("Stamina")]
        [SerializeField] int baseStamina=12000;
        [Tooltip("How many percent stamina increases per level, divided by 100")]
        [SerializeField] float staminaUpgradeValue=0.5f;
        [SerializeField, Tooltip("Should we passively drain stamina?")] bool usePassiveDrain=true;
        [SerializeField, Tooltip("Passive drain in units/tick")] int passiveDrain=1;
        [SerializeField, Tooltip("Walking drain in units/tick")] int walkDrain=4;
        [SerializeField, Tooltip("Drain in units per use.")] int jumpDrain=180;
        
        /// <summary>
        /// The max stamina the player can have.
        /// </summary>
        public int MaxStamina { get => (int)(baseStamina * (1 + staminaUpgradeValue) * staminaLevel); }
        public int CurrentStamina { get; private set; }
        
        // Upgrade levels
        public int staminaLevel;

        void UpdateStamina(DrainType[] actions)
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
                catch (Exception e){ Debug.LogException(e);}
            }

            if (usePassiveDrain)
            {
                CurrentStamina -= passiveDrain;
            }
        }
    }
}