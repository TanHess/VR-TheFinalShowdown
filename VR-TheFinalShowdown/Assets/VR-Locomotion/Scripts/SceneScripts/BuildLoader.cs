using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildLoader : MonoBehaviour
{
    [Header("Loads Scene 0 in the Build Settings, but also additively loads the starting scene.")]
    [SerializeField] string DesiredStartingScene = "Menu";

    private void Awake()
    {
        if (!Application.isEditor)
            LoadPersistent();
    }

    private void LoadPersistent()
    {
        SceneManager.LoadSceneAsync(DesiredStartingScene, LoadSceneMode.Additive);
    }
}
