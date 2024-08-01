using UnityEngine;

namespace Game
{
    public static class AnimationKeys
    {
        public readonly static int Idle = Animator.StringToHash("Idle");
        public readonly static int Move = Animator.StringToHash("Move");
        public readonly static int MoveSpeed = Animator.StringToHash("MoveSpeed");
    }
}
