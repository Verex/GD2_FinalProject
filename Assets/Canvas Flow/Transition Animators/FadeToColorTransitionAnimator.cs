/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

using P7.CanvasFlow.RectTransformExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace P7.CanvasFlow
{
    // Easily create instances of this transition animator in the Unity menu.
    [CreateAssetMenu(fileName = "FadeToColorTransitionAnimator",
                     menuName = "Canvas Flow/Fade To Color Transition Animator Instance",
                     order = 500)]

    public class FadeToColorTransitionAnimator : CanvasControllerTransitioningAnimator,
        ICanvasControllerTransitioningAnimator
    {
        [Tooltip("The color of the transition")]
        public Color color;

        [System.Serializable]
        public struct FadeSetting
        {
            [Tooltip("The duration of the transition.")]
            public float duration;

            [Tooltip("The animation curve of the transition.")]
            public AnimationCurve curve;
        }
        [Tooltip("The settings for the 'pre-fade' section of the transition.")]
        public FadeSetting preFadeSettings;
        [Tooltip("The settings for the 'post-fade' section of the transition.")]
        public FadeSetting postFadeSettings;

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
            // Set the destination content inactive.
            var destination = transitionContext.destinationCanvasController;
            destination.ContentActive = false;

            // Create an image overlay.
            ImageOverlay imageOverlay = new ImageOverlay();

            // Set the image overlay to transparent color.
            Color targetColorTransparent = color;
            targetColorTransparent.a = 0f;
            imageOverlay.Color = targetColorTransparent;

            // Add the overlay to the source content.
            var source = transitionContext.sourceCanvasController;
            imageOverlay.SetParent(source.content);

            // Perform pre-fade.
            PerformPreFade(imageOverlay, () =>
            {
                // Pre-fade is complete. Move the image overlay to the destination canvas.
                var destinationContent = destination.content;
                imageOverlay.SetParent(destinationContent);

                // Make the source inactive and the destination active.
                source.ContentActive = false;
                destination.ContentActive = true;

                // Perform post-fade and complete transition on completion.
                PerformPostFade(imageOverlay, transitionContext.CompleteTransition);
            });
        }

        public override void AnimateTransitionForInitialCanvasController(
            CanvasControllerTransitionContext transitionContext)
        {
            /*  
             *  We implement different behaviour for transitions involving the
             *  initial canvas controller.
             */

            // Create an image overlay.
            ImageOverlay imageOverlay = new ImageOverlay();
            if (transitionContext.isUpstream)
            {
                /*
                 *  When dismissing an initial canvas controller we modify the
                 *  post-fade to fade to nothing.
                 */

                // Set the image overlay to transparent color.
                Color targetColorTransparent = color;
                targetColorTransparent.a = 0f;
                imageOverlay.Color = targetColorTransparent;

                // Add the overlay to the source content.
                var source = transitionContext.sourceCanvasController;
                imageOverlay.SetParent(source.content);

                // Perform pre fade.
                PerformPreFade(imageOverlay, () =>
                {
                    // Pre-fade is complete. Set the source content to inactive and
                    // move the overlay to its parent (the canvas).
                    imageOverlay.SetParent(source.CanvasRectTransform());
                    source.ContentActive = false;

                    // Perform post-fade and complete transition on completion.
                    PerformPostFade(imageOverlay, transitionContext.CompleteTransition);
                });
            }
            else
            {
                /* 
                 *  When presenting an initial canvas controller we skip the 'pre-fade'
                 *  and begin immediately at the transition color.
                 */

                // Set image overlay to the transition color immediately.
                imageOverlay.Color = color;

                // Add the overlay to the destination content.
                var destination = transitionContext.destinationCanvasController;
                imageOverlay.SetParent(destination.content);

                // Perform post-fade and complete transition on completion.
                PerformPostFade(imageOverlay, transitionContext.CompleteTransition);
            }
        }

        #endregion

        #region Perform Fade Animations

        private void PerformPreFade(ImageOverlay imageOverlay, System.Action completion)
        {
            Color targetColor = color;
            Color targetColorTransparent = targetColor;
            targetColorTransparent.a = 0f;

            // Fade in the image overlay.
            Routine fadeInOverlay = new Routine(preFadeSettings.duration, 0f, UpdateMode);
            fadeInOverlay.Run((progress01) =>
            {
                float easedProgress01 = preFadeSettings.curve.Evaluate(progress01);
                imageOverlay.Color = Color.LerpUnclamped(targetColorTransparent,
                                                         targetColor,
                                                         easedProgress01);
            }, completion);
        }

        private void PerformPostFade(ImageOverlay imageOverlay, System.Action completion)
        {
            Color targetColor = color;
            Color targetColorTransparent = targetColor;
            targetColorTransparent.a = 0f;

            // Fade out the image overlay.
            Routine fadeOutOverlay = new Routine(postFadeSettings.duration, 0f, UpdateMode);
            fadeOutOverlay.Run((progress01) =>
            {
                float easedProgress01 = postFadeSettings.curve.Evaluate(progress01);
                imageOverlay.Color = Color.LerpUnclamped(targetColor,
                                                         targetColorTransparent,
                                                         easedProgress01);
            }, () =>
            {
                // Post-fade is complete. Destroy the image overlay.
                Destroy(imageOverlay.gameObject);

                // Call the completion action.
                completion();
            });
        }

        #endregion

        #region Scriptable Object Lifecycle

        private void Reset()
        {
            // Apply default properties when created or reset in the editor.
            color = Color.black;

            preFadeSettings = new FadeSetting
            {
                duration = 0.4f,
                curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
            };

            postFadeSettings = new FadeSetting
            {
                duration = 0.4f,
                curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
            };

            description = "The 'fade to color' transition animator creates a color fade " +
                "transition between screens.\n\nIt will first fade from the source content to " +
                "the specified color - the 'pre-fade' - and then fade from the specified color " +
                "to the destination content - the 'post-fade'.";
        }

        #endregion

        #region Image Overlay

        private class ImageOverlay
        {
            public GameObject gameObject;
            public Image image;
            public RectTransform rectTransform;

            #region Constructor

            public ImageOverlay()
            {
                gameObject = new GameObject("FadeToColorTransitionAnimator - Overlay",
                                            typeof(RectTransform));

                image = gameObject.AddComponent<Image>();
                rectTransform = gameObject.GetComponent<RectTransform>();
            }

            #endregion

            #region Convenience

            public Color Color
            {
                get
                {
                    return image.color;
                }

                set
                {
                    image.color = value;
                }
            }

            public void SetParent(RectTransform parent)
            {
                rectTransform.SetParent(parent, false);
                rectTransform.FillParent();
            }

            #endregion
        }

        #endregion
    }
}