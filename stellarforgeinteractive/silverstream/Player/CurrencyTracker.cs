using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public class CurrencyTracker
    {
        public static CurrencyTracker Instance { get; private set; }

        static CurrencyTracker()
        {
            Instance = new CurrencyTracker();
        }

        private CurrencyTracker()
        {
            EventSystem.TreatCollected += TreatCollected;
            EventSystem.GameplayStart += GameplayStart;
            // If/when we have a reset save we'll deal with it here.

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