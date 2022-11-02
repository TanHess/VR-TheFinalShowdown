using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint_Manager : MonoBehaviour
{
    private Player_Spawn_Location[] SpawnLocations;
    [SerializeField] private Player_Spawn_Location DesiredCheckpoint;

    private bool CurrentCheckpoint;

    [Header("This fires when the Checkpoint is set to this one")]
    public UnityEvent SetCheckpointToThis;
    [Header("This fires when the Checkpoint is set to one that isn't this one")]
    public UnityEvent CheckpointTakenAway;



    // Start is called before the first frame update
    void Start()
    {
        SpawnLocations = FindObjectsOfType<Player_Spawn_Location>();
    }

    public void SetCheckpoint(Player_Spawn_Location Checkpoint)
    {
        foreach (Player_Spawn_Location SpawnPoint in SpawnLocations)
        {
            SpawnPoint.CurrentSpawnPoint = false; //First, toggle all of them as false
        }
        Checkpoint.CurrentSpawnPoint = true; //Then, mark the desired one as the current point
        Debug.Log("Set Checkpoint to "+ Checkpoint.name);

    }

    private void Update()
    {
        if (!DesiredCheckpoint.CurrentSpawnPoint && CurrentCheckpoint)
        {
            CheckpointTakenAway.Invoke();
            CurrentCheckpoint = false;
        }

        if (DesiredCheckpoint.CurrentSpawnPoint && !CurrentCheckpoint)
        {
            SetCheckpointToThis.Invoke();
            CurrentCheckpoint = true;
        }
    }
}
