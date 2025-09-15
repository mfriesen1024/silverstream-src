using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    /// <summary>
    /// Responsible for handling game state and progression.
    /// </summary>
    public class GameManager:MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private bool _init = false;
        
        // I don't know if I'll need this but i have it now anyway.
        public static bool Initialized
        {
            get => GetIsInitialized();
        }
        
        private static bool GetIsInitialized()
        {
            if (Instance != null)
            {
                return Instance._init;
            }

            return false;
        }
    }
}