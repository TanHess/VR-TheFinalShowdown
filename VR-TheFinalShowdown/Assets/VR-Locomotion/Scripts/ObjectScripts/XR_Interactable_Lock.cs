using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XR_Interactable_Lock : MonoBehaviour
{
    public bool Locked = true;
    public void ToggleLock(bool Toggle)
    {
        Locked = Toggle;
    }
}
