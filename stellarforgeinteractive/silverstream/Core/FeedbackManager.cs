using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    // Responsible for spawning particles and sound objects.
    public class FeedbackManager:MonoBehaviour
    {
        public static FeedbackManager instance;

        AudioSource audioPlayer;

        [SerializeField] AudioClip sadMeowSFX;
        [SerializeField] GameObject treatParticlePrefab;
        [SerializeField] AudioClip purrSFX;
        [SerializeField] GameObject jumpParticlePrefab;
        [SerializeField] Vector2 jumpParticleOffset;
        [SerializeField] AudioClip jumpSFX;
        [SerializeField] GameObject dashParticlePrefab;
        [SerializeField] Vector2 dashParticleOffset;
        [SerializeField] AudioClip dashSFX;

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            audioPlayer = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;

            EventSystem.PlayerDied += PlayerDied;
            EventSystem.TreatCollected += TreatCollected;
            EventSystem.PlayerJumped += PlayerJumped;
            EventSystem.PlayerStartedDash += PlayerStartedDash;
        }

        void PlayerStartedDash(Transform obj)
        {
            if(dashSFX){audioPlayer.PlayOneShot(dashSFX);}

            TrySpawnParticles(dashParticlePrefab, obj.position + (Vector3)dashParticleOffset)
                .transform.parent = obj;
        }

        void PlayerJumped(Vector3 position)
        {
            if(jumpSFX){audioPlayer.PlayOneShot(jumpSFX);}
            TrySpawnParticles(jumpParticlePrefab, position + (Vector3)jumpParticleOffset);
        }

        private void TreatCollected(Vector3 position)
        {
            audioPlayer.PlayOneShot(purrSFX);
            TrySpawnParticles(treatParticlePrefab, position);
        }

        private void PlayerDied()
        {
            audioPlayer.PlayOneShot(sadMeowSFX);
        }

        GameObject TrySpawnParticles(GameObject prefab, Vector3 position)
        {
            try
            {
                var temp = Instantiate(prefab, position, Quaternion.identity);
                
                if (!temp.TryGetComponent(out ParticleSystem ps))
                {
                    Debug.Log(new NullReferenceException("Wheres my particle system!!!!"));
                }

                ps.TryGetComponent(out EventHelper eh);
                ps.Play();

                return temp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}