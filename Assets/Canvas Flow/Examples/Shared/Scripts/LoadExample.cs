#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace P7.CanvasFlow
{
    [ExecuteInEditMode]
    public class LoadExample : MonoBehaviour
    {
        #if UNITY_EDITOR
        public SceneAsset[] scenes;
        #endif

        public string sceneToLoad;

        /*
         *  Add example scenes to the build only when the example scene is opened.
         */

        #if UNITY_EDITOR
        private void Awake()
        {
            if (Application.isPlaying == false)
            {
                // Determine which scenes are not in the build scenes.
                var scenesToAdd = new List<SceneAsset>();
                EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
                foreach (var scene in scenes)
                {
                    string scenePath = AssetDatabase.GetAssetPath(scene);
                    bool isInBuildScenes = buildScenes.Any(s => s.path.Equals(scenePath));
                    if (isInBuildScenes == false)
                    {
                        scenesToAdd.Add(scene);
                    }
                }

                if (scenesToAdd.Count > 0)
                {
                    // Add missing scenes to the build.
                    var newBuildScenes = new List<EditorBuildSettingsScene>(buildScenes);
                    foreach (var sceneToAdd in scenesToAdd)
                    {
                        string scenePath = AssetDatabase.GetAssetPath(sceneToAdd);
                        var buildScene = new EditorBuildSettingsScene(scenePath, true);
                        newBuildScenes.Add(buildScene);
                    }

                    EditorBuildSettings.scenes = newBuildScenes.ToArray();
                }
            }
        }
        #endif

        private void Start()
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
		}
    }
}