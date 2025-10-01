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
        [SerializeField] float dashVerticalAcceleration = 30;
        [SerializeField] float dashDownMultiplier = 1f;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 60;
        [SerializeField] float postJumpGravityScale = 2;
        [SerializeField] int jumpTicks = 9;
        [SerializeField] int coyoteTicks = 9;
        [SerializeField] int dashTicks = 15;
        [SerializeField] int dashCooldownTicks = 10;
        [Header("SpawnSettings")]
        [SerializeField] Vector3 spawnPosition;
        Vector2 dashDirection;
        float defaultGravityScale;
        int jumpTicksLeft,wallJumpTicksLeft;
        int dashTicksLeft, dashCooldownTicksLeft;
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
            // If we're not dashing, and we're off by 0.1 units/s, accelerate.
            if (absDiff > 0.1 && dashTicksLeft<1)
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
            if (dashTicksLeft>0)
            {
                float x = dashDirection.x * dashHorizontalAcceleration * TimeMod;
                float y = dashDirection.y * dashVerticalAcceleration * TimeMod;
                if (y < 0) { y *= dashDownMultiplier;}
                
                cVel.x += x;
                cVel.y += y;
                
                // I doubt things will break, but if they do, dont eat the dash.
                dashTicksLeft--;
            }
            
            // Apply velocity and stamina drain.
            statController.UpdateStamina(drain.ToArray());
            rb.linearVelocity = cVel;
        }

        Vector2 UpdateJumpAndDash(Vector2 linearVelocity)
        {
            // Tick coyote time and dash cooldown.
            coyoteTicksLeft = coyoteTicksLeft > 0 ? coyoteTicksLeft - 1 : 0;
            dashCooldownTicksLeft=dashCooldownTicksLeft>0 ? dashCooldownTicksLeft - 1 : 0;
            
            if (grounded)
            {
                coyoteTicksLeft = coyoteTicks;
                rb.gravityScale = defaultGravityScale;
                
                // If player grounded and on their last dash tick, end the dash early, and cap horizontal speed.
                // I want to intentionally allow supers and extended supers, so we only check for one tick.
                if (dashTicksLeft == 1)
                {
                    dashTicksLeft = 0;
                    float speedCap = horizontalSpeed * 1f;
                    // If magnitude is greater than speedcap, multiply normalized speed by speedcap.
                    Debug.Log($"Cap is {speedCap}, xVel is {linearVelocity.x}");
                    linearVelocity.x = Math.Abs(linearVelocity.x)> speedCap ? linearVelocity.normalized.x * speedCap:linearVelocity.x;
                    Debug.Log($"New value is {linearVelocity.x}");
                }
                
                // If dash cooldown is over, mark dash as ready for use again
                dashReady = dashCooldownTicksLeft < 1 && statController.DashUnlocked;
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

            // Start a dash if: its ready, input is down, and we're not already dashing.
            if (inputHelper.DashInput && dashReady && dashTicksLeft < 1)
            {
                // Grab direction and reset y velocity.
                dashDirection = inputHelper.BooleanMove;
                linearVelocity.y = 0;
                rb.gravityScale = 0;
                
                // Setup ticking system
                dashTicksLeft = dashTicks;
                dashCooldownTicksLeft = dashCooldownTicks;
                
                // Consume resources
                statController.UpdateStamina(new []{DrainType.Dash});
                dashReady = false;
            }

            // Floor player's Y velocity at 0 so we dont spike them, and reset gravity scale.
            if (dashTicksLeft == 1)
            {
                // Player's Y vel should never be low, but not zero post dash.
                if (linearVelocity.y > 0)
                {
                    rb.gravityScale = postJumpGravityScale;
                }
                else
                {
                    rb.gravityScale = defaultGravityScale;
                    linearVelocity.y = 0;
                }
            }

            return linearVelocity;
        }
    }
}