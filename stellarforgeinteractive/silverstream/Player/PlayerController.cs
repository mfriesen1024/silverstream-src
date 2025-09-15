using UnityEngine;
using UnityEngine.InputSystem;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        PlayerController.InputHelper inputHelper;
        Rigidbody2D rb;
        [SerializeField] float HorizontalSpeed;
        [SerializeField] float ActiveAcceleration;
        [SerializeField] float PassiveDeceleration;
        [SerializeField] float JumpAcceleration;
        [SerializeField] int JumpTicks;
        int jumpTicksLeft;
        int coyoteTicksLeft;

        // We'll replace this with GM.TimeMod.
        const float TempTimeMod = 1;
        float TimeMod {get => TempTimeMod*Time.deltaTime;}

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputHelper = new PlayerController.InputHelper(inputActions);
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            HandleMovement();
        }

        void HandleMovement()
        {
            // Capture current velocity, we'll "buffer" it before applying.
            Vector2 cVel = rb.linearVelocity;
            // Capture bool move so we don't recalculate it.
            Vector2 boolInput = inputHelper.BooleanMove;

            // Horizontal movement stuff.

            #region Hmove

            // use raw for x movement.
            Debug.Log(TimeMod);
            float hVelTarget = inputHelper.RawMove.x * HorizontalSpeed * TimeMod;

            // If we're off by 0.1 units/s, accelerate.
            if (Mathf.Abs(cVel.x - hVelTarget) > 0.1)
            {
                // If we're pressing a horizontal input by more than 0.5, accelerate quickly.
                if (Mathf.Abs(boolInput.x) > 0)
                {
                    Debug.Log(TimeMod);
                    cVel.x += boolInput.x * ActiveAcceleration*TimeMod;
                }
                else
                {
                    Debug.Log(TimeMod);
                    cVel.x += boolInput.x * PassiveDeceleration*TimeMod;
                }
            }

            #endregion

            // Vertical movement stuff.

            #region Vmove

            UpdateJump();

            if (jumpTicksLeft > 0)
            {
                Debug.Log(TimeMod);
                cVel.y = cVel.y < 0 ? 0 : cVel.y + JumpAcceleration*TimeMod;
            }

            #endregion

            // Apply velocity
            rb.linearVelocity = cVel;
        }

        void UpdateJump()
        {
            if (inputHelper.JumpInput && coyoteTicksLeft > 0)
            {
                jumpTicksLeft = JumpTicks;
                coyoteTicksLeft=0;
            }
        }
    }
}