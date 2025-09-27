using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.World
{
    public class Background:MonoBehaviour
    {
        [SerializeField] GameObject player;

        // Teleport background to player.
        private void Update()
        {
            try
            {
                transform.position = player.transform.position;
            }catch{}
        }
    }
}