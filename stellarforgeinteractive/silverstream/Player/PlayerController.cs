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
        public static float Distance {get; private set;}
        [Header("Refs")]
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] EventHelper groundCheck;
        [SerializeField] EventHelper hurtBox;
        PlayerStatController statController = PlayerStatController.Instance;
        InputHelper inputHelper;
        Rigidbody2D rb;
        [Header("Movement")]
        [SerializeField] float horizontalSpeed = 5;
        [SerializeField] float activeAcceleration = 15;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 60;
        [SerializeField] float postJumpGravityScale = 2;
        [SerializeField] int jumpTicks = 9;
        [SerializeField] int coyoteTicks = 9;
        [FormerlySerializedAs("SpawnPosition")]
        [Header("SpawnSettings")]
        [SerializeField] Vector3 spawnPosition;
        float defaultGravityScale;
        int jumpTicksLeft;
        int coyoteTicksLeft = 9;

        bool grounded = true;

        // We'll replace this with GM.TimeMod.
        const float TempTimeMod = 1;
        float TimeMod {get => TempTimeMod*Time.deltaTime;}

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputHelper = new InputHelper(inputActions);
            rb = GetComponent<Rigidbody2D>();
            
            // Set movement stuff
            defaultGravityScale = rb.gravityScale;
            
            // Spawn the player
            transform.position = spawnPosition;
            
            // Event things.
            EventSystem.GameplayStart+= GameplayStart;

            statController.OutOfStamina += OutOfStamina;
            hurtBox.TriggerEnter2D += HitObstacle;
            groundCheck.TriggerEnter2D += GCEnter;
            groundCheck.TriggerExit2D += GCExit;

            // Reset character when gameplay starts.
            void GameplayStart()
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = spawnPosition;
                grounded = true;
            }

            // Death things
            void HitObstacle(Collider2D obj)
            {
                if (obj.TryGetComponent(out Hazard ignored1))
                {
                    Death();
                }

                if (obj.TryGetComponent(out EndLevelTrigger ignored2))
                {
                    EventSystem.PlayerWon();
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
                if (obj.gameObject != gameObject) grounded = false;
            }
        }

        private void Death()
        {
            try
            {
                EventSystem.PlayerDied();
            }
            catch (Exception ignored)
            {
                // ignored
            }
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.GameplayRunning)
            {
                HandleMovement();
                
                // For UI things
                Distance = transform.position.magnitude;
            }
        }

        void HandleMovement()
        {
            var drain = Array.Empty<DrainType>().ToList();
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
                drain.Add(DrainType.Walk);
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                drain.Add(DrainType.Jump);
            }
            statController.UpdateStamina(drain.ToArray());
            
            // A lot of this is just so I can debug it.
            var velDiff = cVel.x - hVelTarget;
            var diffSign = -Mathf.Sign(velDiff);
            var absDiff = Mathf.Abs(velDiff);
            // Debug.Log($"HVel target: {hVelTarget} AbsDiff: {absDiff} VelDiff: {velDiff}");
            // If we're off by 0.1 units/s, accelerate.
            if (absDiff > 0.1)
            {
                // If we're pressing a horizontal input by more than 0.5, or grounded accelerate quickly.
                if (hInputAbsolute > 0 || grounded)
                {
                    //Debug.Log(TimeMod);
                    cVel.x += diffSign * activeAcceleration * TimeMod;
                }
                else
                {
                    //Debug.Log(TimeMod);
                    cVel.x += diffSign * passiveDeceleration * TimeMod;
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
                rb.gravityScale = defaultGravityScale;
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                jumpTicksLeft = jumpTicks;
                coyoteTicksLeft = 0;
                grounded = false;
            }

            if (jumpTicksLeft == 1)
            {
                rb.gravityScale = postJumpGravityScale;
            }
        }
    }
}