using System;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    /// <summary>
    /// Exposes unity events to other classes. Oh I miss Godot...
    /// </summary>
    public class EventHelper:MonoBehaviour
    {
        public Action Ready = DoNothing;
        public Action<float> Process = DoNothing;
        public Action<float> PhysicsProcess = DoNothing;
        public Action EnterTree = DoNothing;
        public Action ExitTree = DoNothing;
        public Action QueueFree = DoNothing;
        public Action<Collider2D> TriggerEnter2D = DoNothing;
        public Action<Collider2D> TriggerExit2D = DoNothing;
        public Action<Collider2D> TriggerStay2D = DoNothing;

        private static void DoNothing() { }

        private static void DoNothing(float obj) { }

        private static void DoNothing(Collider2D obj) { }

        
        private void Awake()
        {
            Ready();
        }

        private void Update()
        {
            Process(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            PhysicsProcess(Time.fixedDeltaTime);
        }

        private void OnEnable()
        {
            EnterTree();
        }

        private void OnDisable()
        {
            ExitTree();
        }

        private void OnDestroy()
        {
            QueueFree();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter2D(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit2D(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TriggerStay2D(other);
        }
    }
}