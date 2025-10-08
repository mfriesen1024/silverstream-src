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
                if (pc.jumpTicksLeft == pc.jumpTicks -1)
                {
                    animator.SetTrigger("jump");
                }
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
            }

        }
    }
}