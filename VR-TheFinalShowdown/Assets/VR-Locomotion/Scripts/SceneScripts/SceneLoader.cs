using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public UnityEvent OnLoadBegin = new UnityEvent();
    public UnityEvent OnLoadEnd = new UnityEvent();

    [SerializeField] float FadeTime = 1.0f;
    [Header("The Loading Screen will stick around for this extra amount of time.")]
    [SerializeField] float LoadingScreenTime = 2.0f;
    [Header("The color that the fade goes to")]
    [SerializeField] private Color FadeColor = Color.black;

    private bool isLoading;

    private void Awake()
    {
        SceneManager.sceneLoaded += SetActiveScene;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SetActiveScene;
    }

    public void LoadNewScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadScene(sceneName));
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        isLoading = true;

        ColorFilter_Controller.FadePanelChange(FadeColor, FadeTime, true);

        yield return new WaitForSeconds(FadeTime);
        OnLoadBegin.Invoke();

        //Unload the scene that we're on right now
        yield return StartCoroutine(UnloadCurrent());

        //Fade out and wait
        ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, FadeTime, false);

        yield return new WaitForSeconds(FadeTime);

        //The Loading Screen will stick around for these extra seconds
        yield return new WaitForSeconds(LoadingScreenTime);

        //Fade to black again and wait
        ColorFilter_Controller.FadePanelChange(FadeColor, FadeTime, true);

        yield return new WaitForSeconds(FadeTime);
        //Load the new scene
        yield return StartCoroutine(LoadNew(sceneName));
        OnLoadEnd.Invoke();

        //Fade back in
        ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, FadeTime, false);
        yield return new WaitForSeconds(FadeTime);

        isLoading = false;



        yield return null;
    }

    private IEnumerator UnloadCurrent()
    {
        AsyncOperation UnloadOperation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        //Wait until the scene is fully unloaded
        while (!UnloadOperation.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadNew(string sceneName)
    {
        //Using the Additive scene mode will add the desired scene to the hierarchy rather than replace it.
        AsyncOperation LoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!LoadOperation.isDone)
        {
            yield return null;
        }

        //Re-syncs all of the Light Probes when everything's done.
        LightProbes.TetrahedralizeAsync();

    }

    private void SetActiveScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);
    }
}

