/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

using P7.CanvasFlow.DynamicObjectExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace P7.CanvasFlow
{
    // Easily create instances of this transition animator in the Unity menu.
    [CreateAssetMenu(fileName = "ScaleAndFadeBackgroundTransitionAnimator",
                     menuName = "Canvas Flow/Scale And Fade Background Transition Animator Instance",
                     order = 500)]

    public class ScaleAndFadeBackgroundTransitionAnimator : CanvasControllerTransitioningAnimator,
        ICanvasControllerTransitioningAnimator
    {
        [Tooltip("The duration of the transition")]
        public float duration;

        [Header("Scale Settings")]
        [Tooltip("The animation curve for the scaling animation of the transition.")]
        public AnimationCurve scaleCurve;

        [Tooltip("The property name of the content to be scaled.")]
        public string contentToScalePropertyName;

        [Tooltip("Reverse the scale animation curve for upstream transitions.")]
        public bool reverseScaleCurveOnUpstream;

        [Header("Fade Settings")]
        [Tooltip("The animation curve for the fading animation of the transition.")]
        public AnimationCurve fadeCurve;

        [Tooltip("Reverse the fade animation curve for upstream transitions.")]
        public bool reverseFadeCurveOnUpstream;

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
            // Create our fade animation action.
            var fadeBackground = FadeBackgroundAnimation(transitionContext);

            // Create our scale animation action.
            var scaleContent = ScaleContentAnimation(transitionContext);

            // Start a routine to animate our content.
            Routine animation = new Routine(duration, 0f, UpdateMode);
            animation.Run((progress01) =>
            {
                // Drive the fade animation action.
                if (fadeBackground != null)
                {
                    // Evaluate the fade animation curve with the current progress.
                    float fadeEasedProgress01;
                    if (transitionContext.isUpstream && reverseFadeCurveOnUpstream)
                    {
                        // If transitioning upstream with reverseFadeCurveOnUpstream, we
                        // invert the animation curve lookup.
                        fadeEasedProgress01 = fadeCurve.Evaluate(1f - progress01);
                        fadeEasedProgress01 = 1f - fadeEasedProgress01;
                    }
                    else
                    {
                        fadeEasedProgress01 = fadeCurve.Evaluate(progress01);
                    }

                    fadeBackground(fadeEasedProgress01);
                }

                // Drive the scale animation action.
                if (scaleContent != null)
                {
                    // If transitioning upstream with reverseScaleCurveOnUpstream, we
                    // invert the progress.
                    if (transitionContext.isUpstream && reverseScaleCurveOnUpstream)
                    {
                        progress01 = 1f - progress01;
                    }

                    // Evaluate the scale animation curve with the current progress.
                    float scaleEasedProgress01 = scaleCurve.Evaluate(progress01);

                    // If transitioning upstream without reverseScaleCurveOnUpstream, we
                    // invert the eased progress.
                    if (transitionContext.isUpstream && (reverseScaleCurveOnUpstream == false))
                    {
                        scaleEasedProgress01 = 1f - scaleEasedProgress01;
                    }

                    scaleContent(scaleEasedProgress01);
                }

            }, transitionContext.CompleteTransition);
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

            // Dynamically find the target transform from the property name.
            RectTransform targetTransform = null;
            if (string.IsNullOrEmpty(contentToScalePropertyName) == false)
            {
                targetTransform = targetCanvasController.GetField<RectTransform>(
                contentToScalePropertyName);
            }

            if (targetTransform == null)
            {
                // Fall back to the content transform.
                targetTransform = targetCanvasController.content;
            }

            // Create the scale animation action.
            System.Action<float> scaleContentAnimation = (progress01) =>
            {
                targetTransform.localScale = Vector3.LerpUnclamped(Vector3.zero,
                                                                   Vector3.one,
                                                                   progress01);
            };

            return scaleContentAnimation;
        }

        #endregion

        #region Fade Background Animation

        private System.Action<float> FadeBackgroundAnimation(
            CanvasControllerTransitionContext transitionContext)
        {
            // Determine the target canvas controller - the one being presented or dismissed.
            var targetCanvasController = (transitionContext.isUpstream) ?
                transitionContext.sourceCanvasController :
                    transitionContext.destinationCanvasController;

            System.Action<float> fadeBackgroundAnimation = null;
            Image backgroundImage = targetCanvasController.backgroundImage;
            if (backgroundImage != null)
            {
                // Get colors from background image.
                Color targetColor = backgroundImage.color;
                Color transparentColor = targetColor;
                transparentColor.a = 0f;

                // Determine start and end colors.
                Color startColor = (transitionContext.isUpstream) ? targetColor : transparentColor;
                Color endColor = (transitionContext.isUpstream) ? transparentColor : targetColor;

                // Create fade animation action .
                fadeBackgroundAnimation = (progress01) =>
                {
                    backgroundImage.color = Color.Lerp(startColor, endColor, progress01);
                };
            }

            return fadeBackgroundAnimation;
        }

        #endregion

        #region Scriptable Object Lifecycle

        private void Reset()
        {
            // Apply default properties when created or reset in the editor.
            duration = 0.4f;
            scaleCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f, 3.00052f, 3.00052f),
                new Keyframe(1f, 1f, 0.05463887f, 0.05463887f)
            });
            fadeCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f, 3.00052f, 3.00052f),
                new Keyframe(1f, 1f, 0.05463887f, 0.05463887f)
            });

            description = "The 'scale and fade background' transition animator presents " +
                "content by scaling it up from the center of the screen, whilst fading in the " +
                "backgroundImage property. It dismisses content by performing the reverse - " +
                "scaling content down and fading out the backgroundImage property.\n\nThe " +
                "'contentToScalePropertyName' field allows you to specify what content gets " +
                "scaled in your transition. You can set this to the name of any RectTransform " +
                "property on your canvas controller. If none is specified or the property " +
                "does not exist, the canvas controller's 'content' transform will be scaled.\n\n" +
                "The reverseScaleCurveOnUpstream and reverseFadeCurveOnUpstream flags cause " +
                "the relevant animation curves to be evaluated backwards on upstream transitions.";
        }

        #endregion
    }
}