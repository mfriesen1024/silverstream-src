using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public class CurrencyTracker
    {
        public const double TreatMultiplier = 5;
        public const double DistanceMultiplier = 0.05;
        public const double StaminaMultiplier = 0.025;
        
        public static CurrencyTracker Instance { get; private set; }

        public int Currency {get; private set;}

        static CurrencyTracker()
        {
            Instance = new CurrencyTracker();
        }

        private CurrencyTracker()
        {
            EventSystem.TreatCollected += TreatCollected;
            EventSystem.GameplayStart += GameplayStart;
            EventSystem.PlayerDied += PlayerDied;
            // If/when we have a reset save we'll deal with it here.

            // Update currency when player dies.
            void PlayerDied()
            {
                PlayerStatController psc = PlayerStatController.Instance;
                Currency += (int)(TreatsThisRun*TreatMultiplier);
                Currency += (int)(PlayerController.Distance*DistanceMultiplier);
                Currency += (int)((psc.MaxStamina - psc.CurrentStamina) * StaminaMultiplier);
            }

            void GameplayStart()
            {
                TreatsThisRun = 0;
            }

            void TreatCollected(Vector3 obj)
            {
                TreatsThisRun++;
            }
        }

        public int TreatsThisRun;
    }
}