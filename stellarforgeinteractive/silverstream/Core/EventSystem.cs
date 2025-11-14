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

        public static Action<int> PlayerDied = EndGameplay;

        public static Action<Vector3> SecondLifeUsed = DoNothing;

        public static Action PlayerWon = EndGameplay;

        /// <summary>
        /// Called when a treat is collected.
        /// </summary>
        /// Triggers feedback and currency system.
        public static Action<Vector3> TreatCollected;

        /// <summary>
        /// Called when the player jumps or wall jumps.
        /// </summary>
        /// Triggers feedback system.
        public static Action<Vector3> PlayerJumped;
        
        public static Action<Transform> PlayerStartedDash;

        /// <summary>
        /// Determines if a tutorial should be shown, and which one should be shown.
        /// </summary>
        public static Action<int> ShowTutorial;
        
        public static void DoNothing() { }
        public static void DoNothing(Vector3 obj) { }
        
        static void EndGameplay(int obj) { EndGameplay(); }

        static void EndGameplay() { GameplayEnd(); }

        static void _Init()
        {
            Debug.Log(new NotImplementedException());
        }

    }
}