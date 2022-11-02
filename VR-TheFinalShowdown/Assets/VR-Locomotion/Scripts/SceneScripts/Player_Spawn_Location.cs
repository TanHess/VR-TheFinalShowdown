using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Player_Spawn_Location : MonoBehaviour
{
    [HideInInspector] public bool CurrentSpawnPoint = false;
    [SerializeField] private Player_Spawn_Location[] SpawnLocations;

    public void SetSpawnPoint(bool value)
    {
        CurrentSpawnPoint = value;
    }

    void Awake()
    {
        SpawnLocations = FindObjectsOfType<Player_Spawn_Location>();
#if UNITY_EDITOR
        //Turns itself into a non-prefab to reduce interference on play
        if (PrefabUtility.IsPartOfAnyPrefab(this))
        {
            PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
        }
#endif
    }

    public void SetCheckpoint()
    {
        foreach (Player_Spawn_Location SpawnPoint in SpawnLocations)
        {
            SpawnPoint.CurrentSpawnPoint = false; //First, toggle all of them as false
        }
        CurrentSpawnPoint = true; //Then, mark the desired one as the current point
        Debug.Log("Set Checkpoint to " + name);
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#endif
    }
}

