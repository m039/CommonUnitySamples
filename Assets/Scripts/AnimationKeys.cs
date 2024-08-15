using UnityEngine;

namespace Game
{
    public static class AnimationKeys
    {
        public readonly static int Idle = Animator.StringToHash("Idle");
        public readonly static int Move = Animator.StringToHash("Move");
        public readonly static int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public readonly static int Empty = Animator.StringToHash("Empty");
        public readonly static int Lit = Animator.StringToHash("Lit");
        public readonly static int Opened = Animator.StringToHash("Opened");
        public readonly static int Closed = Animator.StringToHash("Closed");
    }
}
