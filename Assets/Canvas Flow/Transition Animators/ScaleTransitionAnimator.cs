/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

using UnityEngine;

namespace P7.CanvasFlow
{
    // Easily create instances of this transition animator in the Unity menu.
    [CreateAssetMenu(fileName = "ScaleTransitionAnimator",
                     menuName = "Canvas Flow/Scale Transition Animator Instance",
                     order = 500)]

    public class ScaleTransitionAnimator : CanvasControllerTransitioningAnimator,
        ICanvasControllerTransitioningAnimator
    {
        [Tooltip("The duration of the transition")]
        public float duration;

        [Tooltip("The animation curve of the transition")]
        public AnimationCurve curve;

        [Tooltip("Reverse the animation curve for upstream transitions.")]
        public bool reverseCurveOnUpstream;

        [Tooltip("Animate with scaled time. By default this transition uses unscaled time - " +
                 "i.e. it won't be affected by changes to 'Time.timeScale'. This allows it to " +
                 "animate when the game is paused, for example. Enable this if you want the " +
                 "transition to be affected by changes to 'Time.timeScale'.")]
        public bool useScaledTime;
        private RoutineUpdateMode UpdateMode
        {
            get
            {
                return (useScaledTime) ?
                    RoutineUpdateMode.ScaledTime : RoutineUpdateMode.UnscaledTime;
            }
        }

        #region ICanvasControllerTransitioningAnimator

        public override void AnimateTransition(CanvasControllerTransitionContext transitionContext)
        {
            // Create our animation action.
            var animateContent = ScaleContentAnimation(transitionContext);

            // Start a routine to animate our content.
            Routine animation = new Routine(duration, 0f, UpdateMode);
            animation.Run((progress01) =>
            {
                // If transitioning upstream with reverseCurveOnUpstream, we
                // invert the progress.
                if (transitionContext.isUpstream && reverseCurveOnUpstream)
                {
                    progress01 = 1f - progress01;
                }

                // Evaluate the animation curve with the current progress.
                float easedProgress01 = curve.Evaluate(progress01);

                // If transitioning upstream without reverseCurveOnUpstream, we
                // invert the eased progress.
                if (transitionContext.isUpstream && (reverseCurveOnUpstream == false))
                {
                    easedProgress01 = 1f - easedProgress01;
                }

                // Execute our scale animation action with the eased progress.
                animateContent(easedProgress01);

            }, transitionContext.CompleteTransition); // Call CompleteTransition() on completion.
        }

        public override void AnimateTransitionForInitialCanvasController(
            CanvasControllerTransitionContext transitionContext)
        {
            /*
             *  We support transitions to and from the initial canvas controller by
             *  default in our main AnimateTransition() method as we only operate on the
             *  presented canvas controller, so we can just forward to that method.
             */

            AnimateTransition(transitionContext);
        }

        #endregion

        #region Scale Content Animation

        private System.Action<float> ScaleContentAnimation(
            CanvasControllerTransitionContext transitionContext)
        {
            // Determine the target canvas controller - the one being presented or dismissed.
            var targetCanvasController = (transitionContext.isUpstream) ?
                transitionContext.sourceCanvasController :
                    transitionContext.destinationCanvasController;

            // Create the scale animation action.
            System.Action<float> scaleContentAnimation = (progress01) =>
            {
                targetCanvasController.ContentScale = Vector3.LerpUnclamped(Vector3.zero,
                                                                            Vector3.one,
                                                                            progress01);
            };

            return scaleContentAnimation;
        }

        #endregion

        #region Scriptable Object Lifecycle

        private void Reset()
        {
            // Apply default properties when created or reset in the editor.
            duration = 0.4f;
            curve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f, 3.00052f, 3.00052f),
                new Keyframe(1f, 1f, 0.05463887f, 0.05463887f)
            });

            description = "The scale transition animator presents content by scaling it up from " +
                "the center of the screen. It dismisses content by performing the reverse - " +
                "scaling content down.\n\nThe reverseCurveOnUpstream flag causes the animation " +
                "curve to be evaluated backwards on upstream transitions.";
        }

        #endregion
    }
}