using UnityEngine;

public class SceneChange : MonoBehaviour
{
    public void LoadScene(string SceneToLoad)
    {
        SceneLoader.Instance.LoadNewScene(SceneToLoad);
    }
}
