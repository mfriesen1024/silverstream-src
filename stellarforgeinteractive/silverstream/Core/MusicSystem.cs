using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicSystem:MonoBehaviour
    {
        bool gameplayActiveLastTick = false;
        
        AudioSource audioPlayer;

        [SerializeField] AudioClip menu, gameplay;

        void Start()
        {
            gameplayActiveLastTick = true;
            audioPlayer = GetComponent<AudioSource>();
        }

        void Update()
        {
            try
            {
                if (gameplayActiveLastTick && !GameManager.Instance.GameplayRunning)
                {
                    audioPlayer.resource = menu;
                    audioPlayer.Play();
                }

                if (!gameplayActiveLastTick && GameManager.Instance.GameplayRunning)
                {
                    audioPlayer.resource = gameplay;
                    audioPlayer.Play();
                }
            }
            catch (Exception ignored) { }
            
            gameplayActiveLastTick = GameManager.Instance.GameplayRunning;
        }
    }
}