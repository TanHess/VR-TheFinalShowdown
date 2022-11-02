using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class KeyItem_Manager : MonoBehaviour
{
    [SerializeField] private KeyItem_Object[] SceneKeyItems;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneKeyItems = FindObjectsOfType<KeyItem_Object>(true); //Get the new scene's Key Items, even inactive ones
        DestroyDuplicates();
    }
    void OnSceneUnloaded(Scene scene)
    {   
        //Reset the arrays
        SceneKeyItems = null;
    }

    private void DestroyDuplicates()
    {
        StartCoroutine(DestroyDuplicatesRoutine());
    }

    private IEnumerator DestroyDuplicatesRoutine()
    {
        yield return new WaitForEndOfFrame();

        foreach (KeyItem_Object SceneItem in SceneKeyItems)
        {
            foreach (KeyItem_Object SceneItem2 in SceneKeyItems) //Compare each item to each item
            {
                if (SceneItem.name == SceneItem2.name && SceneItem != SceneItem2) // They have the same name, but they aren't the same object
                {
                    if (SceneItem.Found && !SceneItem2.Found && SceneItem.DestroyDuplicates) //Ensure that we want to destroy
                    {
                        Debug.Log("Duplicate found. Destroy " + SceneItem2);
                        Destroy(SceneItem2.gameObject); // We destroy the duplicate that isn't 'Found'
                    }
                }
            }
        }

        yield return new WaitForEndOfFrame();

        SceneKeyItems = null;
        SceneKeyItems = FindObjectsOfType<KeyItem_Object>(true); //Update the array once destroys are done.

        yield return null;

    }
}

