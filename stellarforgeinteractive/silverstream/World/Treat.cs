using System;
using ca.stellarforgeinteractive.silverstream.Core;
using ca.stellarforgeinteractive.silverstream.Player;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.World
{
    [RequireComponent(typeof(EventHelper))]
    public class Treat:MonoBehaviour
    {
        [SerializeField] EventHelper eventHelper;

        private void Start()
        {
            eventHelper ??= GetComponent<EventHelper>();
            if (eventHelper == null)
            {
                throw new NullReferenceException("EventHelper is required!");
            }
            
            EventSystem.GameplayStart+= GameplayStart;
            eventHelper.TriggerEnter2D += TriggerEnter2D;
        }

        private void GameplayStart()
        {
            gameObject.SetActive(true);
        }

        private void TriggerEnter2D(Collider2D obj)
        {
            if (obj.TryGetComponent(out PlayerController ignored))
            {
                Debug.LogException(new NotImplementedException("Treats not implemented."));
                
                EventSystem.TreatCollected(transform.position);
                
                gameObject.SetActive(false);
            }
        }
    }
}