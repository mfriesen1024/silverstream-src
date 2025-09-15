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

        public static Action PausePressed = DoNothing;

        public static Action PlayerDied = GameplayEnd;
        
        public static Action PlayerWon = GameplayEnd;

        private static void DoNothing()
        {
            
        }
        
        private static void _Init()
        {
            Debug.Log(new NotImplementedException());
        }
    }
}