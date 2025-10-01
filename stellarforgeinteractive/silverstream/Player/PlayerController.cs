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
        [SerializeField] float dashHorizontalAcceleration = 30;
        [SerializeField] float dashVerticalAcceleration = 75;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 60;
        [SerializeField] float postJumpGravityScale = 2;
        [SerializeField] int jumpTicks = 9;
        [SerializeField] int coyoteTicks = 9;
        [SerializeField] int dashTicks = 15;
        [Header("SpawnSettings")]
        [SerializeField] Vector3 spawnPosition;
        float defaultGravityScale;
        int jumpTicksLeft,wallJumpTicksLeft;
        int dashTicksLeft=-1;
        int coyoteTicksLeft,wallCoyoteTicksLeft;

        bool grounded = true;
        bool wallGrounded;
        bool dashReady;

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
            groundCheck.TriggerStay2D += GCStay;
            groundCheck.TriggerExit2D += GCExit;

            // Reset character when gameplay starts.
            void GameplayStart()
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = spawnPosition;
                grounded = true;
                dashReady = statController.DashUnlocked;
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
            void GCStay(Collider2D obj)
            {
                // If we find a ground collider, become grounded if we're not starting a new jump
                // This prevents "i hit jump" when gliding over a corner and trying to jump, while preventing double jumps.
                if (obj.TryGetComponent(out GroundCollider ignored)) grounded = jumpTicksLeft < jumpTicks/2;
            }
            void GCExit(Collider2D obj)
            {
                if (obj.TryGetComponent(out GroundCollider ignored)) grounded = false;
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

            // Before we do anything, determine what we're going to do about jumping, and whether velocities should be reset.
            cVel = UpdateJumpAndDash(cVel);

            // Horizontal movement stuff.
            #region Hmove
            // use raw for x movement.
            float hVelTarget = inputHelper.RawMove.x * horizontalSpeed;
            float hInputAbsolute = Mathf.Abs(boolInput.x);
            // Debug.Log($"Hvel Target: {hVelTarget}");
            
            // Update stamina drain info
            if (hInputAbsolute > 0)
            {
                drain.Add(DrainType.Walk);
            }
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
            if (jumpTicksLeft > 0)
            {
                cVel.y = cVel.y < 0 ? 0 : cVel.y + jumpAcceleration * TimeMod;
                jumpTicksLeft--;
            }

            if (wallJumpTicksLeft > 0)
            {
                throw new NotImplementedException("Wall jump not implemented");
            }

            #endregion

            // Handle dash.
            if (inputHelper.DashInput && dashReady)
            {
                Debug.LogException(new NotImplementedException("Dash not implemented"));
                
                
            }
            
            // Apply velocity and stamina drain.
            statController.UpdateStamina(drain.ToArray());
            rb.linearVelocity = cVel;
        }

        Vector2 UpdateJumpAndDash(Vector2 linearVelocity)
        {
            coyoteTicksLeft = coyoteTicksLeft > 0 ? coyoteTicksLeft - 1 : 0;
            
            if (grounded)
            {
                coyoteTicksLeft = coyoteTicks;
                rb.gravityScale = defaultGravityScale;
                
                // If player grounded and on their last dash tick, end the dash early, and cap horizontal speed.
                // I want to intentionally allow supers and extended supers, so we only check for one tick.
                if (dashTicksLeft == 1)
                {
                    dashTicksLeft = 0;
                    float speedCap = horizontalSpeed * 1.5f;
                    // If magnitude is greater than speedcap, multiply normalized speed by speedcap.
                    linearVelocity.x = Math.Abs(linearVelocity.x)> speedCap ? linearVelocity.normalized.x * speedCap:linearVelocity.x;
                }
            }

            if (wallGrounded&&!grounded)
            {
                wallCoyoteTicksLeft = coyoteTicks;
                rb.gravityScale = defaultGravityScale;
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                jumpTicksLeft = jumpTicks;
                coyoteTicksLeft = 0;
                grounded = false;
                statController.UpdateStamina(new [] { DrainType.Jump });

                // TODO: determine whether to cancel or floor velocities.
                linearVelocity.y = 0;
            }

            if (inputHelper.JumpInput && wallCoyoteTicksLeft > 0)
            {
                wallJumpTicksLeft = jumpTicks;
                wallCoyoteTicksLeft = 0;
                wallGrounded = false;
                
                // TODO: determine whether to cancel or floor velocities.
                linearVelocity=Vector2.zero;
            }

            if (jumpTicksLeft == 1 || wallJumpTicksLeft == 1)
            {
                rb.gravityScale = postJumpGravityScale;
            }

            return linearVelocity;
        }
    }
}