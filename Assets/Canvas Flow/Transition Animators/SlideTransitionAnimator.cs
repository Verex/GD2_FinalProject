/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

using UnityEngine;

namespace P7.CanvasFlow
{
    // Easily create instances of this transition animator in the Unity menu.
    [CreateAssetMenu(fileName = "SlideTransitionAnimator",
                     menuName = "Canvas Flow/Slide Transition Animator Instance",
                     order = 500)]

    public class SlideTransitionAnimator : CanvasControllerTransitioningAnimator,
        ICanvasControllerTransitioningAnimator
    {
        [Header("General")]
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

        public enum OffScreenLocation
        {
            Left,
            Top,
            Right,
            Bottom
        }
        [Header("Presented Canvas")]
        [Tooltip("The off-screen location of the presented canvas.")]
        public OffScreenLocation presentedCanvasOffScreenLocation;

        [Header("Presenting Canvas")]
        [Tooltip("Animate the presenting canvas.")]
        public bool animatePresentingCanvas;

        [Tooltip("The off-screen location of the presenting canvas.")]
        public OffScreenLocation presentingCanvasOffScreenLocation;

        [Tooltip("The presenting canvas' parallax scalar. A value of 1 will animate fully to the " +
                 "off-screen location (no parallax), whilst 0.5 would animate halfway to the " +
                 "off-screen location.")]
        [Range(0f, 1f)]
        public float presentingCanvasParallaxScalar;

        #region ICanvasControllerTransitioningAnimator

        public override void AnimateTransition(CanvasControllerTransitionContext transitionContext)
        {
            // Create our presented content animation action.
            var animatePresentedContent = MovePresentedContentAnimation(transitionContext);
            var animatePresentingContent = MovePresentingContentAnimation(transitionContext);

            // Start a routine to animate our content.
            Routine animation = new Routine(duration, 0f, UpdateMode);
            animation.Run((progress01) =>
            {
                // Evaluate the animation curve with the current progress.
                float easedProgress01 = curve.Evaluate(progress01);

                // Execute our animations with the eased progress.
                animatePresentedContent(easedProgress01);
                if (animatePresentingContent != null)
                {
                    animatePresentingContent(easedProgress01);
                }

            }, transitionContext.CompleteTransition); // Call CompleteTransition() on completion.
        }

        public override void AnimateTransitionForInitialCanvasController(
            CanvasControllerTransitionContext transitionContext)
        {
            /*
             *  We support transitions to and from the initial canvas controller by
             *  default in our main AnimateTransition() method by observing for a
             *  null presenter, so we can just forward to that method.
             */

            AnimateTransition(transitionContext);
        }

        #endregion

        #region Move Content Animations

        private System.Action<float> MovePresentedContentAnimation(
            CanvasControllerTransitionContext transitionContext)
        {
            // Determine the presented canvas controller - the one being presented or dismissed.
            var presentedCanvasController = (transitionContext.isUpstream) ?
                transitionContext.sourceCanvasController :
                    transitionContext.destinationCanvasController;
            
            // Get the off screen position for the presented canvas controller.
            Vector3 offScreenPosition = PositionForCanvasControllerAtLocation(
                presentedCanvasController, presentedCanvasOffScreenLocation);

            // Determine the content's start and end positions.
            Vector3 contentStartPosition = (transitionContext.isUpstream) ?
                presentedCanvasController.OnScreenContentPosition() : offScreenPosition;
            Vector3 contentEndPosition = (transitionContext.isUpstream) ?
                offScreenPosition : presentedCanvasController.OnScreenContentPosition();

            // Create the move animation action.
            System.Action<float> moveContentAnimation = (progress01) =>
            {
                presentedCanvasController.ContentPosition = Vector3.LerpUnclamped(
                    contentStartPosition,
                    contentEndPosition,
                    progress01);
            };

            return moveContentAnimation;
        }

        private System.Action<float> MovePresentingContentAnimation(
            CanvasControllerTransitionContext transitionContext)
        {
            System.Action<float> moveContentAnimation = null;
            if (animatePresentingCanvas)
            {
                // Determine the presenting canvas controller (the presenter).
                var presentingCanvasController = (transitionContext.isUpstream) ?
                    transitionContext.destinationCanvasController :
                        transitionContext.sourceCanvasController;

                // The presenting canvas controller will be null for transitions
                // involving an initial canvas controller.
                if (presentingCanvasController != null)
                {
                    // Get the on screen position for the presenting canvas controller.
                    Vector3 onScreenPosition = presentingCanvasController.OnScreenContentPosition();

                    // Get the off screen position for the presenting canvas controller.
                    Vector3 offScreenPosition = PositionForCanvasControllerAtLocation(
                        presentingCanvasController, presentingCanvasOffScreenLocation);

                    // Interpolate based on the parallax scalar.
                    offScreenPosition = Vector3.Lerp(onScreenPosition,
                                                     offScreenPosition,
                                                     presentingCanvasParallaxScalar);

                    // Determine the content's start and end positions.
                    Vector3 contentStartPosition = (transitionContext.isUpstream) ?
                        offScreenPosition : onScreenPosition;
                    Vector3 contentEndPosition = (transitionContext.isUpstream) ?
                        onScreenPosition : offScreenPosition;

                    // Create the move animation action.
                    moveContentAnimation = (progress01) =>
                    {
                        presentingCanvasController.ContentPosition = Vector3.LerpUnclamped(
                            contentStartPosition,
                            contentEndPosition,
                            progress01);
                    };
                }
            }

            return moveContentAnimation;
        }

        #endregion

        #region Position Calculation

        private Vector3 PositionForCanvasControllerAtLocation(CanvasController canvasController,
                                                              OffScreenLocation location)
        {
            Vector3 position = Vector3.zero;
            switch (location)
            {
                case OffScreenLocation.Left:
                {
                    position = canvasController.OffScreenLeftContentPosition();
                    break;
                }

                case OffScreenLocation.Top:
                {
                    position = canvasController.OffScreenTopContentPosition();
                    break;
                }

                case OffScreenLocation.Right:
                {
                    position = canvasController.OffScreenRightContentPosition();
                    break;
                }

                case OffScreenLocation.Bottom:
                {
                    position = canvasController.OffScreenBottomContentPosition();
                    break;
                }
            }

            return position;
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

            presentedCanvasOffScreenLocation = OffScreenLocation.Right;

            animatePresentingCanvas = true;
            presentingCanvasOffScreenLocation = OffScreenLocation.Left;
            presentingCanvasParallaxScalar = 0.4f;

            description = "The slide transition animation presents content by sliding it in " +
                "from an off-screen location. It dismisses content by performing the reverse - " +
                "sliding it to an off-screen location.\n\nIn addition to the presented canvas " +
                "controller, the presenting canvas controller can also be animated to and from " +
                "an off-screen location. This gives the appearance of the " +
                "content sliding out as the presented content slides in.\n\nWith the " +
                "'presentingCanvasParallaxScalar' property, you can scale the amount by which " +
                "the presenter moves, creating parallax effects. A value of 1 will animate " +
                "fully to the off-screen location (no parallax), whilst 0.5 would animate " +
                "halfway to the off-screen location.A value of 0 has the same effect as " +
                "disabling 'animatePresentingCanvas'.";
        }

        #endregion
    }
}