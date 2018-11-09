using P7.CanvasFlow;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FCLoadingCanvasController : CanvasController
{
    private string sceneToLoad;
    public string SceneToLoad
    {
        set
        {
            sceneToLoad = value;
        }
    }

    #region Canvas Controller Appearance

	protected override void CanvasDidAppear()
	{
        // Begin loading the scene once our canvas controller is completely on screen (after the
        // transition has completed).

        if (string.IsNullOrEmpty(sceneToLoad) == false)
        {
            StartCoroutine(LoadSceneRoutine(sceneToLoad));
        }
        else
        {
            Debug.LogError("No SceneToLoad has been set on FCLoadingCanvasController.");
        }
    }

    #endregion

    #region Load Scene

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        Scene previousScene = SceneManager.GetActiveScene();

        // Unload the previous scene.
        AsyncOperation loadOperation = SceneManager.UnloadSceneAsync(previousScene);
        yield return loadOperation;

        // Load the new scene asynchronously.
        loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return loadOperation;

        // Make the newly loaded scene the active scene.
        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);
        SceneManager.SetActiveScene(loadedScene);

        // Dismiss all canvas controllers in our hierarchy.
        DismissAllCanvasControllers();
    }

    #endregion
}