using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    public static class AnimatorExtension
    {
        public static AnimationClip GetAnimationClip(this Animator animator, string name)
        {
            if (!animator)
                return null;

            for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                if (animator.runtimeAnimatorController.animationClips[i].name == name)
                    return animator.runtimeAnimatorController.animationClips[i];
            }
            return null;
        }
    }
}
