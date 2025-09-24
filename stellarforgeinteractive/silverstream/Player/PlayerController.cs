using System;
using System.Linq;
using ca.stellarforgeinteractive.silverstream.Core;
using ca.stellarforgeinteractive.silverstream.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        public static float distance {get; private set;} = 0;
        [Header("Refs")]
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] EventHelper groundCheck;
        [SerializeField] EventHelper hurtBox;
        PlayerStatController statController = PlayerStatController.GetPSC();
        PlayerController.InputHelper inputHelper;
        Rigidbody2D rb;
        [Header("Movement")]
        [SerializeField] float horizontalSpeed = 5;
        [SerializeField] float activeAcceleration = 15;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 45;
        [SerializeField] int jumpTicks = 6;
        [SerializeField] int coyoteTicks = 9;
        [Header("SpawnSettings")]
        [SerializeField] Vector3 SpawnPosition;
        int jumpTicksLeft;
        int coyoteTicksLeft = 9;

        bool grounded = true;

        // We'll replace this with GM.TimeMod.
        const float TempTimeMod = 1;
        float TimeMod {get => TempTimeMod*Time.deltaTime;}

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputHelper = new PlayerController.InputHelper(inputActions);
            rb = GetComponent<Rigidbody2D>();
            
            // Spawn the player
            transform.position = SpawnPosition;
            
            // Event things.
            statController.OutofStamina += OutOfStamina;
            hurtBox.TriggerEnter2D += HitObstacle;
            groundCheck.TriggerEnter2D += GCEnter;
            groundCheck.TriggerExit2D += GCExit;

            // Death things
            void HitObstacle(Collider2D obj)
            {
                if (obj.TryGetComponent(out Hazard ignored))
                {
                    Death();
                }
            }
            void OutOfStamina()
            {
                Death();
            }
            // Ground things
            void GCEnter(Collider2D obj)
            {
                // if(obj.TryGetComponent())
                if (obj.gameObject != gameObject)
                    grounded = true;
            }
            void GCExit(Collider2D obj)
            {
                // if(obj.TryGetComponent())
                if (obj.gameObject != gameObject)
                    grounded = false;
            }
        }

        private void Death()
        {
            try
            {
                Debug.Log("death");
                EventSystem.PlayerDied();
                rb.linearVelocity=Vector2.zero;
                transform.position = SpawnPosition;
                grounded = true;
            }catch(Exception e){Debug.Log(e);}
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.GameplayRunning)
            {
                HandleMovement();
                
                // For UI things
                distance = transform.position.magnitude;
            }
        }

        void HandleMovement()
        {
            var Drain = Array.Empty<DrainType>().ToList();
            // Capture current velocity, we'll "buffer" it before applying.
            Vector2 cVel = rb.linearVelocity;
            // Capture bool move so we don't recalculate it.
            Vector2 boolInput = inputHelper.BooleanMove;

            // Horizontal movement stuff.

            #region Hmove

            // use raw for x movement.
            float hVelTarget = inputHelper.RawMove.x * horizontalSpeed;
                float hInputAbsolute = Mathf.Abs(boolInput.x);
            // Debug.Log($"Hvel Target: {hVelTarget}");
            
            // Set stamina stuff
            if (hInputAbsolute > 0)
            {
                Drain.Add(DrainType.Walk);
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                Drain.Add(DrainType.Jump);
            }
            statController.UpdateStamina(Drain.ToArray());
            
            // If we're off by 0.1 units/s, accelerate.
            var velDiff = Mathf.Abs(cVel.x - hVelTarget);
            Debug.Log($"HVel target: {hVelTarget} VelDiff: {velDiff}");
            if (velDiff > 0.1)
            {
                // If we're pressing a horizontal input by more than 0.5, or grounded accelerate quickly.
                if (hInputAbsolute > 0 || grounded)
                {
                    //Debug.Log(TimeMod);
                    cVel.x += boolInput.x * activeAcceleration * TimeMod;
                }
                else
                {
                    //Debug.Log(TimeMod);
                    cVel.x += boolInput.x * passiveDeceleration * TimeMod;
                }
            }
            // If we're close to target velocity, still moving, and not giving input, set hvel to 0.
            else if (hInputAbsolute == 0)
            {
                cVel.x = 0;
            }

            #endregion

            // Vertical movement stuff.

            #region Vmove

            UpdateJump();

            //Debug.Log($"Jump ticks = {jumpTicksLeft}, coyote frames = {coyoteTicksLeft}, grounded = {grounded}");
            if (jumpTicksLeft > 0)
            {
                cVel.y = cVel.y < 0 ? 0 : cVel.y + jumpAcceleration * TimeMod;
                jumpTicksLeft--;
            }

            #endregion

            // Apply velocity
            rb.linearVelocity = cVel;
        }

        void UpdateJump()
        {
            if (grounded)
            {
                coyoteTicksLeft = coyoteTicks;
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                jumpTicksLeft = jumpTicks;
                coyoteTicksLeft = 0;
                grounded = false;
            }
        }
    }
}