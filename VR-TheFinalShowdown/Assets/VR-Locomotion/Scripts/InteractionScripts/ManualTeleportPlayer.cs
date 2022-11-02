using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTeleportPlayer : MonoBehaviour
{
    public void ManualTeleport(Transform Point)
    {
        XR_Manual_Teleport.ManualTeleportPlayer(Point);
    }
}
