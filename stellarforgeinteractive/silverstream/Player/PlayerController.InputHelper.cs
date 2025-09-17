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
            private bool jumpInput;

            public InputHelper(InputActionAsset inputActions)
            {
                move = inputActions.FindAction("Move");
                jump = inputActions.FindAction("Jump");
            }

            public bool JumpInput { get => jump.ReadValue<float>()>0.1; }

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