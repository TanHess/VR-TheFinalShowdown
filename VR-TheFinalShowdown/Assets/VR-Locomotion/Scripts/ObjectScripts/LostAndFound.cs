using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LostAndFound : MonoBehaviour
{
    [SerializeField] List<SnapDropZone_HiddenObjectMethod> LostAndFoundZones;

    [SerializeField] private LostAndFound_Spawn_Location[] SpawnLocations;
    private Vector3 OriginalScale;
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        OriginalScale = transform.localScale;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " was loaded. Spawn Lost and Found");
        //Keeps the persistent scene from triggering the Spawn
        if (scene != this.gameObject.scene)
        {
           // transform.localScale = OriginalScale;
            SpawnLostAndFound();
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log(scene.name + " was unloaded. Reset Lost and Found");
        ResetRoot();
    }

    public void ResetRoot()
    {
        Vector3 TempLocation = new Vector3(0, -10000, 0);
        transform.position = TempLocation;
    }

    public void SpawnLostAndFound()
    {
        StartCoroutine(SpawnLostAndFoundRoutine());
    }

    private IEnumerator SpawnLostAndFoundRoutine()
    {
        yield return new WaitForEndOfFrame();

        SpawnLocations = FindObjectsOfType<LostAndFound_Spawn_Location>();

        if (SpawnLocations.Length < 1)
        {
            Debug.LogWarning("We don't have any Lost and Found spawn points. We'll just put it at (0,0,0)");
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            yield return null;
        }

        foreach (LostAndFound_Spawn_Location SpawnPoint in SpawnLocations)
        {
            if (SpawnPoint.CurrentSpawnPoint) //You've gotta have the spawn point be marked as the current one.
            {
                transform.position = SpawnPoint.transform.position;
                transform.rotation = SpawnPoint.transform.rotation;
                Debug.Log("Spawned Lost and Found at " + SpawnPoint.transform.name);
            }
        }

        yield return null;

    }

    public void RetrieveItem(XRBaseInteractable DesiredItem)
    {
        for (int i = 0; i < LostAndFoundZones.Count; i++)
        {
            if (!LostAndFoundZones[i].HasStoredObject)
            {
                Debug.Log("COUNT NEXT");
                StartCoroutine(RetrieveRoutine(LostAndFoundZones[i], DesiredItem));
                break;
            }
        }
    }
    IEnumerator RetrieveRoutine(SnapDropZone_HiddenObjectMethod Zone, XRBaseInteractable DesiredItem)
    {
        Zone.Attach(DesiredItem);
        yield return new WaitForEndOfFrame();

    }
}
