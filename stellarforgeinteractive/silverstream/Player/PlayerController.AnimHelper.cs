using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public partial class PlayerController
    {
        class AnimHelper
        {
            PlayerController pc;
            Animator animator;
            EventHelper selfNode;

            public AnimHelper(EventHelper selfNode, PlayerController pc)
            {
                this.selfNode = selfNode;
                this.pc = pc;
                animator = pc.animator;
                
                this.selfNode.Process += Process;
            }

            void Process(float delta)
            {
                animator.SetBool("jump", pc.jumpTicksLeft > 0);
                animator.SetBool("walljump", pc.wallJumpTicksLeft > 0);
                
                animator.SetBool("dash", pc.dashTicksLeft > 0);

                var boolMove = pc.inputHelper.BooleanMove;
                if (boolMove.x > 0)
                {
                    animator.SetBool("right",true);
                }
                if (boolMove.x < 0)
                {
                    animator.SetBool("right",false);
                }
                animator.SetBool("moving", boolMove.x != 0);
            }
        }
    }
}