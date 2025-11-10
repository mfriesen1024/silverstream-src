using UnityEngine;
using UnityEngine.InputSystem;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController
    {
        private class InputHelper
        {
            InputAction move;
            InputAction jump;
            InputAction dash;
            bool jumpInput;
            bool jumpGhost;
            bool dashInput;
            bool dashGhost;

            public InputHelper(InputActionAsset inputActions)
            {
                move = inputActions.FindAction("Move");
                jump = inputActions.FindAction("Jump");
                dash = inputActions.FindAction("Dash");
            }

            /// <summary>
            /// Returns whether the player just pressed jump. This value should be cached.
            /// </summary>
            public bool JumpInput => GetJumpInput();

            bool GetJumpInput()
            {
                var jumpInput = jump.ReadValue<float>() > 0.1;
                jumpGhost = jumpInput == this.jumpInput;
                return jumpInput && !jumpGhost;
            }

            /// <summary>
            /// Determines if player has valid move input for dashing, AND dash key pressed. This value should be cached.
            /// </summary>
            public bool DashInput => GetDashInput();

            bool GetDashInput()
            {
                var dashInput=dash.ReadValue<float>() > 0.1 && BooleanMove != Vector2.zero;
                dashGhost = dashInput == this.dashInput;
                return dashInput && !dashGhost;
            }

            public Vector2 RawMove => move.ReadValue<Vector2>();

            public Vector2 BooleanMove => RefineInput();

            Vector2 RefineInput()
            {
                Vector2 value = RawMove;
                value.x = value.x > 0.5 ? 1 : value.x < -0.5 ? -1 : 0;
                value.y = value.y > 0.5 ? 1 : value.y < -0.5 ? -1 : 0;
                return value;
            }
        }
    }
}