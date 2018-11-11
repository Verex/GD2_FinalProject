/*
 *  Copyright © 2018 Pelican 7 LTD. All rights reserved.
 *  This file is part of the Canvas Flow asset, which is distributed under the Asset
 *  Store Terms of Service and EULA - https://unity3d.com/legal/as_terms.
 */

namespace P7.CanvasFlow
{
    // A Storyboard Hook for Unity UI Button components.
    [UnityEngine.RequireComponent(typeof(UnityEngine.UI.Button))]
    public class StoryboardHookUnityEngineUIButton : StoryboardHook
	{
        public UnityEngine.UI.Button button;

        #region Mono Lifecycle

        protected override void Reset()
        {
            // Always call base.Reset() when overriding Reset() in a custom hook.
            base.Reset();

            // Store a reference to the Button component.
            button = GetComponent<UnityEngine.UI.Button>();
        }

        #endregion

        #region Storyboard Hook

        public override System.Type AutoAddComponentType
        {
            get
            {
                // Automatically add this hook to game objects with a Unity UI
                // Button component when the scene is saved.
                return typeof(UnityEngine.UI.Button);
            }
        }

        public override void Connect(System.Action<StoryboardHook> invokeTransition)
        {
            // For a Unity UI Button we invoke our storyboard transition when
            // our Button is clicked.
            button.onClick.AddListener(() => {
                invokeTransition(this);
            });
        }

        #endregion
	}
}