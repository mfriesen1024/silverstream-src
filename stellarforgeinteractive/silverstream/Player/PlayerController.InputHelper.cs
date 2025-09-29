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
            private bool jumpInput;

            public InputHelper(InputActionAsset inputActions)
            {
                move = inputActions.FindAction("Move");
                jump = inputActions.FindAction("Jump");
                dash = inputActions.FindAction("Dash");
            }

            public bool JumpInput { get => jump.ReadValue<float>()>0.1; }
            
            /// <summary>
            /// Determines if player has valid move input for dashing, AND dash key pressed.
            /// </summary>
            public bool DashInput { get => dash.ReadValue<float>()>0.1 && BooleanMove != Vector2.zero; }

            public Vector2 RawMove { get => move.ReadValue<Vector2>(); }

            public Vector2 BooleanMove { get => RefineInput(); }

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