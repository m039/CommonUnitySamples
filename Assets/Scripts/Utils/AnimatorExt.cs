using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Game
{
    public static class AnimatorExt
    {
        public static float GetAnimationLength(this Animator animator, int animation, float defaultValue = 0, int layer = 0)
        {
            var clip = animator.GetAnimationClip(animation, layer);
            if (clip == null)
                return defaultValue;

            return clip.length;
        }

        public static AnimationClip GetAnimationClip(this Animator animator, int animation, int layer = 0)
        {
            var state = animator.GetAnimatorState(animation, layer);
            if (state != null && state.motion is AnimationClip clip)
            {
                return clip;
            }

            return null;
        }

        public static AnimatorState GetAnimatorState(this Animator animator, int animation, int layer = 0)
        {
            var animatorController = animator.runtimeAnimatorController as AnimatorController;

            if (animatorController.layers.Length <= layer)
                return null;

            foreach (var state in animatorController.layers[layer].stateMachine.states)
            {
                if (state.state.nameHash == animation)
                {
                    return state.state;
                }
            }

            return null;
        }
    }
}
