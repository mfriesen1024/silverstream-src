using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    // Responsible for spawning particles and sound objects.
    public class FeedbackManager: MonoBehaviour
    {
        public static FeedbackManager instance;

        private void Start()
        {
            if(instance==null){instance=this;}
            else{Destroy(gameObject); return; }
            
            
        }
        
        
    }
}