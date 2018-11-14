using P7.CanvasFlow;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingCanvasController : CanvasController
{
    public enum NetworkMode
    {
        CLIENT,
        SERVER
    }

    private string sceneToLoad;
    public string SceneToLoad
    {
        set
        {
            sceneToLoad = value;
        }
    }

    public NetworkHandler NetworkHandler;
    public NetworkMode handlerMode;

    #region Canvas Controller Appearance

    protected override void CanvasDidAppear()
    {
        if (string.IsNullOrEmpty(sceneToLoad) == false)
        {
            StartCoroutine(LoadSceneRoutine(sceneToLoad));
        }
        else
        {
            Debug.LogError("No SceneToLoad has been set on FCLoadingCanvasController.");
        }
    }

    protected override void CanvasDidDisappear()
    {
        // Ensure network handler instantiated.
        if (NetworkHandler != null)
        {
            // Handle network start.
            switch (handlerMode)
            {
                case NetworkMode.CLIENT:
                    NetworkHandler.StartClient();
                    break;
                case NetworkMode.SERVER:
                    NetworkHandler.StartHost();
                    break;
            }
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