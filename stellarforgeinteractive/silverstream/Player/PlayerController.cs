using System;
using System.Linq;
using ca.stellarforgeinteractive.silverstream.Core;
using ca.stellarforgeinteractive.silverstream.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController:MonoBehaviour
    {
        public static float Distance { get; private set; }
        [Header("Core Refs")] [SerializeField] InputActionAsset inputActions;
        [SerializeField] Animator animator;
        [SerializeField] EventHelper selfNode; // I really dont know what to call it.
        [SerializeField] EventHelper groundCheck;
        [SerializeField] EventHelper wallCheckR, wallCheckL;
        [SerializeField] EventHelper hurtBox;
        PlayerStatController statController = PlayerStatController.Instance;
        AnimHelper animHelper;
        InputHelper inputHelper;
        Rigidbody2D rb;
        [Header("Movement")] [SerializeField] float horizontalSpeed = 5;
        [SerializeField] float activeAcceleration = 15;
        [SerializeField] float dashHorizontalAcceleration = 30;
        [SerializeField] float dashVerticalAcceleration = 40;
        [SerializeField] float dashDownMultiplier = 1f;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 60;
        [SerializeField] float wallJumpVAcceleration = 45;
        [SerializeField] float wallJumpHAcceleration = 30;
        
        /// <summary>
        /// Multiplies horizontal acceleration by this much if at any point we're going the wrong direction.
        /// </summary>
        [SerializeField] float wallJumpHDirectionBoost = 1.5f;
        [SerializeField] float postJumpGravityScale = 2;
        [SerializeField] int jumpTicks = 9;
        [SerializeField] int coyoteTicks = 9;
        [SerializeField] int dashTicks = 15;
        [SerializeField] int dashCooldownTicks = 10;
        [Header("SpawnSettings")] [SerializeField]
        Vector3 spawnPosition;
        Vector2 dashDirection;
        float defaultGravityScale;
        int jumpTicksLeft, wallJumpTicksLeft;
        int dashTicksLeft, dashCooldownTicksLeft;
        int coyoteTicksLeft, wallCoyoteTicksLeft;

        bool grounded = true;
        bool airJumpAvailable = false;
        bool hasSecondLife = false;
        bool wallGrounded;
        bool wallJumpDirectionBoost = false;
        float wallJumpDirection; // This is the direction the wall jump will go.
        bool dashReady;

        // We'll replace this with GM.TimeMod.
        const float TempTimeMod = 1;

        float TimeMod { get => TempTimeMod * Time.deltaTime; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputHelper = new InputHelper(inputActions);
            rb = GetComponent<Rigidbody2D>();
            animator ??= GetComponent<Animator>();
            selfNode ??= GetComponent<EventHelper>();
            animHelper = new AnimHelper(selfNode, this);

            // Set movement stuff
            defaultGravityScale = rb.gravityScale;

            // Spawn the player
            transform.position = spawnPosition;

            // Event things.
            EventSystem.GameplayStart += GameplayStart;

            statController.OutOfStamina += OutOfStamina;
            hurtBox.TriggerEnter2D += HitObstacle;
            groundCheck.TriggerStay2D += GCStay;
            groundCheck.TriggerExit2D += GCExit;
            wallCheckL.TriggerStay2D += WCLStay;
            wallCheckL.TriggerExit2D += WCExit;
            wallCheckR.TriggerStay2D += WCRStay;
            wallCheckR.TriggerExit2D += WCExit;

            // Reset character when gameplay starts.
            void GameplayStart()
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = spawnPosition;
                grounded = true;
                dashReady = statController.DashUnlocked;
                hasSecondLife = statController.SecondLifeUnlocked;
                
                // Force reset dash and jumps.
                dashTicksLeft=0;
                jumpTicksLeft = 0;
                wallJumpTicksLeft=0;
            }

            // Death things
            void HitObstacle(Collider2D obj)
            {
                if (obj.TryGetComponent(out Hazard ignored1))
                {
                    Death(1);
                }

                if (obj.TryGetComponent(out EndLevelTrigger ignored2))
                {
                    EventSystem.PlayerWon();
                }
            }

            void OutOfStamina()
            {
                Death(0);
            }

            // Ground things
            void GCStay(Collider2D obj)
            {
                // If we find a ground collider, become grounded if we're not starting a new jump
                // This prevents "i hit jump" when gliding over a corner and trying to jump, while preventing double jumps.
                if (obj.TryGetComponent(out GroundCollider ignored))
                {
                    grounded =
                        jumpTicksLeft < jumpTicks / 2 &&
                        jumpTicksLeft < jumpTicks / 2 &&
                        dashTicksLeft < 10;
                }
            }

            void GCExit(Collider2D obj)
            {
                if (obj.TryGetComponent(out GroundCollider ignored)) grounded = false;
            }

            // Wall check things
            void WCLStay(Collider2D obj)
            {
                if (obj.TryGetComponent(out GroundCollider ignored))
                {
                    SetWallGround();
                    wallJumpDirection = 1; // we hit the left wall, so go right.
                }
            }

            void WCRStay(Collider2D obj)
            {
                if (obj.TryGetComponent(out GroundCollider ignored))
                {
                    SetWallGround();
                    wallJumpDirection = -1; // we hit the right wall, so go left.
                }
            }

            void SetWallGround()
            {
                wallGrounded =
                    !grounded &&
                    wallJumpTicksLeft < jumpTicks / 2 &&
                    jumpTicksLeft < jumpTicks / 2 &&
                    dashTicksLeft < 10;
                if (wallGrounded) wallJumpDirectionBoost = false;
            }

            void WCExit(Collider2D obj)
            {
                if (obj.TryGetComponent(out GroundCollider ignored)) wallGrounded = false;
            }
        }

        void Death(int i)
        {
            if (!hasSecondLife)
            {
                try
                {
                    EventSystem.PlayerDied(i);
                    return;
                }
                catch (Exception ignored)
                {
                    // ignored
                }
            }
            hasSecondLife = false;
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.GameplayRunning)
            {
                HandleMovement();

                // For UI things
                Distance = transform.position.x > 0? transform.position.x:0;
            }
        }

        void HandleMovement()
        {
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
                statController.UpdateStamina(DrainType.Walk);
            }

            // A lot of this is just so I can debug it.
            var velDiff = cVel.x - hVelTarget;
            var diffSign = -Mathf.Sign(velDiff);
            var absDiff = Mathf.Abs(velDiff);
            // Debug.Log($"HVel target: {hVelTarget} AbsDiff: {absDiff} VelDiff: {velDiff}");
            // If we're not dashing, and we're off by 0.1 units/s, accelerate.
            if (absDiff > 0.1 && dashTicksLeft < 1)
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

            // if we're walljumping, do stuff.
            if (wallJumpTicksLeft > 0)
            {
                // Compare our direction to ensure we set velocity to 0 if going the wrong direction.
                float xVelSuchThatTargetDirectionIsPositive = cVel.x * wallJumpDirection;
                if(xVelSuchThatTargetDirectionIsPositive < 0)
                {
                    cVel.x = 0;
                    wallJumpDirectionBoost = true;
                }
                float directionModifier = wallJumpDirectionBoost? wallJumpDirection * wallJumpHDirectionBoost : wallJumpDirection;
                cVel.x += directionModifier * wallJumpHAcceleration * TimeMod;
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
                cVel.y = cVel.y < 0 ? 0 : cVel.y + wallJumpVAcceleration * TimeMod;
                wallJumpTicksLeft--;
            }

            #endregion

            // Handle dash.
            if (dashTicksLeft > 0)
            {
                float x = dashDirection.x * dashHorizontalAcceleration * TimeMod;
                float y = dashDirection.y * dashVerticalAcceleration * TimeMod;
                if (y < 0)
                {
                    y *= dashDownMultiplier;
                }

                cVel.x += x;
                cVel.y += y;

                // I doubt things will break, but if they do, dont eat the dash.
                dashTicksLeft--;
            }

            // Apply velocity.
            rb.linearVelocity = cVel;
        }

        Vector2 UpdateJumpAndDash(Vector2 linearVelocity)
        {
            // Tick coyote time and dash cooldown.
            coyoteTicksLeft = coyoteTicksLeft > 0 ? coyoteTicksLeft - 1 : 0;
            wallCoyoteTicksLeft = wallCoyoteTicksLeft > 0 ? wallCoyoteTicksLeft - 1 : 0;
            dashCooldownTicksLeft = dashCooldownTicksLeft > 0 ? dashCooldownTicksLeft - 1 : 0;

            if (grounded)
            {
                airJumpAvailable = statController.AirJumpUnlocked;
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
                    linearVelocity.x = Math.Abs(linearVelocity.x) > speedCap
                        ? linearVelocity.normalized.x * speedCap
                        : linearVelocity.x;
                    Debug.Log($"New value is {linearVelocity.x}");
                }

                // If dash cooldown is over, mark dash as ready for use again
                dashReady = dashCooldownTicksLeft < 1 && statController.DashUnlocked;
            }

            if (wallGrounded &&statController.WallJumpUnlocked && !grounded)
            {
                wallCoyoteTicksLeft = coyoteTicks;
                rb.gravityScale = defaultGravityScale;
            }

            if (inputHelper.JumpInput && (coyoteTicksLeft > 0||airJumpAvailable))
            {
                airJumpAvailable = coyoteTicksLeft > 0;
                if (dashTicksLeft > 0)
                {
                    Debug.Log($"Timing: {dashCooldownTicksLeft}, {dashTicksLeft}");
                }

                jumpTicksLeft = jumpTicks;
                coyoteTicksLeft = 0;
                grounded = false;
                statController.UpdateStamina(DrainType.Jump);
                EventSystem.PlayerJumped(transform.position);

                linearVelocity.y = 0;
            }

            if (inputHelper.JumpInput && wallCoyoteTicksLeft > 0)
            {
                wallJumpTicksLeft = jumpTicks;
                wallCoyoteTicksLeft = 0;
                wallGrounded = false;
                statController.UpdateStamina(DrainType.Jump);
                EventSystem.PlayerJumped(transform.position);

                // TODO: determine whether to cancel or floor velocities.
                linearVelocity = Vector2.zero;
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

                // Consume resources (remove jump ticks too)
                Debug.Log("Starting dash.");
                jumpTicksLeft = 0;
                wallJumpTicksLeft = 0;
                dashReady = false;
                
                statController.UpdateStamina(DrainType.Dash);
                EventSystem.PlayerStartedDash(transform);
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