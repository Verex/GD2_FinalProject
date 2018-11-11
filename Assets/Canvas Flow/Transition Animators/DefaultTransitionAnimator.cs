/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

using UnityEngine;

namespace P7.CanvasFlow
{
    // Easily create instances of this transition animator in the Unity menu.
    [CreateAssetMenu(fileName = "DefaultTransitionAnimator",
                     menuName = "Canvas Flow/Default Transition Animator Instance",
                     order = 500)]

    public class DefaultTransitionAnimator : CanvasControllerTransitioningAnimator,
        ICanvasControllerTransitioningAnimator
    {
        [Tooltip("The duration of the transition")]
        public float duration;

        [Tooltip("The animation curve of the transition")]
        public AnimationCurve curve;

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
            var animateContent = MoveContentAnimation(transitionContext);

            // Start a routine to animate our content.
            Routine animation = new Routine(duration, 0f, UpdateMode);
            animation.Run((progress01) =>
            {
                // Evaluate the animation curve with the current progress.
                float easedProgress01 = curve.Evaluate(progress01);

                // Execute our move animation action with the eased progress.
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

        #region Move Content Animation

        private System.Action<float> MoveContentAnimation(
            CanvasControllerTransitionContext transitionContext)
        {
            // Determine the target canvas controller - the one being presented or dismissed.
            var targetCanvasController = (transitionContext.isUpstream) ?
                transitionContext.sourceCanvasController :
                    transitionContext.destinationCanvasController;

            // Determine the content's start and end positions.
            Vector3 contentStartPosition = (transitionContext.isUpstream) ?
                targetCanvasController.OnScreenContentPosition() :
                    targetCanvasController.OffScreenBottomContentPosition();
            Vector3 contentEndPosition = (transitionContext.isUpstream) ?
                targetCanvasController.OffScreenBottomContentPosition() :
                    targetCanvasController.OnScreenContentPosition();

            // Create the move animation action.
            System.Action<float> moveContentAnimation = (progress01) =>
            {
                targetCanvasController.ContentPosition = Vector3.LerpUnclamped(
                    contentStartPosition,
                    contentEndPosition,
                    progress01);
            };

            return moveContentAnimation;
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

            description = "The default transition animator presents content from the bottom of " +
                "the screen, sliding it upwards. It dismisses content by performing the reverse " +
                "- animating onscreen content downwards.";
        }

        #endregion
    }
}