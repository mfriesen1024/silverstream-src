using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    /// <summary>
    /// Implements observer pattern.
    /// </summary>
    public static class EventSystem
    {
        /// <summary>
        /// Called by GM during initialization.
        /// </summary>
        public static Action Init = _Init;

        /// <summary>
        /// Called by UIM when gameplay is about to begin.
        /// </summary>
        public static Action GameplayStart = DoNothing;

        public static Action GameplayEnd = DoNothing;

        public static Action GameplayPause = DoNothing;
        
        public static Action GameplayResume = DoNothing;

        public static Action PlayerDied = GameplayEnd;
        
        public static Action PlayerWon = GameplayEnd;

        /// <summary>
        /// Called when a treat is collected.
        /// </summary>
        /// Triggers feedback and currency system.
        public static Action<Vector3> TreatCollected;
        
        public static void DoNothing() { }
        
        private static void _Init()
        {
            Debug.Log(new NotImplementedException());
        }

    }
}