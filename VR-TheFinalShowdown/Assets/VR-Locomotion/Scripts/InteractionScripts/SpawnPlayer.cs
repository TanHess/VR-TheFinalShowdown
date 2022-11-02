using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public void SpawnWithDelay(float Delay)
    {
        Player_Spawn_Manager.SpawnPlayer(Delay);
    }
}
