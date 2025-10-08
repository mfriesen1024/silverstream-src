using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    // Responsible for spawning particles and sound objects.
    public class FeedbackManager: MonoBehaviour
    {
        public static FeedbackManager instance;
        
        AudioSource audioPlayer;
        
        [SerializeField] GameObject treatParticlePrefab;
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
            TrySpawnParticles(Instantiate(treatParticlePrefab));
        }

        private void PlayerDied()
        {
            audioPlayer.PlayOneShot(sadMeow);
        }

        GameObject TrySpawnParticles(GameObject prefab)
        {
            try
            {
                var temp = prefab;
                if (temp.TryGetComponent(out ParticleSystem ps))
                {
                    ps.TryGetComponent(out EventHelper eh);
                    ps.Play();

                    try
                    {
                        eh.PhysicsProcess += PhysicsProcess;

                        void PhysicsProcess(float f)
                        {
                            if (!ps.isPlaying)
                            {
                                eh.PhysicsProcess = null;
                                Destroy(ps);
                            }
                        }
                    }
                    catch (Exception ignored)
                    {
                        Destroy(temp);
                    }
                }
                return prefab;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}