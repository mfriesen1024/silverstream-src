using System;
using ca.stellarforgeinteractive.silverstream.Player;
using ca.stellarforgeinteractive.silverstream.Util;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    /// <summary>
    /// Responsible for handling game state and progression.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        bool _init = false;

        // I don't know if I'll need this but i have it now anyway.
        public static bool Initialized { get => GetIsInitialized(); }

        public bool GameplayRunning { get; private set; }

        private void Start()
        {
            if (!Initialized)
            {
                Instance = this;
            }
            else{Destroy(gameObject);}
            
            EventSystem.GameplayEnd += Halt;
            EventSystem.GameplayPause += Halt;
            EventSystem.GameplayResume += Resume;
            EventSystem.GameplayStart += Resume;
            
            SaveSystem.Load();
            
            // When everything else is done, mark as initialized.
            _init = true;
            EventSystem.Init();
            EventSystem.Init = null;

            void Resume()
            {
                GameplayRunning = true;
            }

            void Halt()
            {
                GameplayRunning = false;
            }
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