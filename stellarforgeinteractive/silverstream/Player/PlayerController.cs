using System;
using System.Linq;
using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] EventHelper groundCheck;
        [SerializeField] PlayerStatController statController = new PlayerStatController();
        PlayerController.InputHelper inputHelper;
        Rigidbody2D rb;
        [SerializeField] float horizontalSpeed = 5;
        [SerializeField] float activeAcceleration = 15;
        [SerializeField] float passiveDeceleration = 5;
        [SerializeField] float jumpAcceleration = 45;
        [SerializeField] int jumpTicks = 6;
        [SerializeField] int coyoteTicks = 9;
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

            statController.OutofStamina += OutOfStamina;

            void OutOfStamina()
            {
                throw new System.NotImplementedException("Death not implemented");
            }

            groundCheck.TriggerEnter2D += GCEnter;

            void GCEnter(Collider2D obj)
            {
                // if(obj.TryGetComponent())
                if (obj.gameObject != gameObject)
                    grounded = true;
            }

            groundCheck.TriggerExit2D += GCExit;

            void GCExit(Collider2D obj)
            {
                // if(obj.TryGetComponent())
                if (obj.gameObject != gameObject)
                    grounded = false;
            }
        }

        void FixedUpdate()
        {
            HandleMovement();
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
            //Debug.Log($"Hvel Target: {hVelTarget}");

            
            // Set stamina stuff
            if (Math.Abs(boolInput.x) > 0)
            {
                Drain.Add(DrainType.Walk);
            }

            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                Drain.Add(DrainType.Jump);
            }
            statController.UpdateStamina(Drain.ToArray());
            
            // If we're off by 0.1 units/s, accelerate.
            if (Mathf.Abs(cVel.x - hVelTarget) > 0.1)
            {
                // If we're pressing a horizontal input by more than 0.5, or grounded accelerate quickly.
                float hInputAbsolute = Mathf.Abs(boolInput.x);
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