using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    // Responsible for spawning particles and sound objects.
    public class FeedbackManager: MonoBehaviour
    {
        public static FeedbackManager instance;
        
        AudioSource audioPlayer;
        
        [SerializeField] AudioClip purr;
        [SerializeField] AudioClip sadMeow;

        private void Start()
        {
            if(instance==null){instance=this;}
            else{Destroy(gameObject); return; }
            
            audioPlayer = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            
            EventSystem.PlayerDied+= PlayerDied;
            EventSystem.TreatCollected += TreatCollected;
        }

        private void TreatCollected(Vector3 obj)
        {
            audioPlayer.PlayOneShot(purr);
        }

        private void PlayerDied()
        {
            audioPlayer.PlayOneShot(sadMeow);
        }
    }
}