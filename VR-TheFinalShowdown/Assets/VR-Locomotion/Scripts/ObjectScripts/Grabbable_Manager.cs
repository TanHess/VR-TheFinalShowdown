using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Grabbable_Manager : MonoBehaviour
{
    [SerializeField] LayerMask LayerToLookFor;

    [SerializeField] private List<XRGrabInteractable> GrabbableArray;
    private List<XRGrabInteractable> Temp = new List<XRGrabInteractable>();
    // Start is called before the first frame update
    void Awake()
    {
        Temp.AddRange(FindObjectsOfType<XRGrabInteractable>());
        foreach (XRGrabInteractable item in Temp)
        {
            if (((1 << item.gameObject.layer) & LayerToLookFor.value) != 0)
            {
                GrabbableArray.Add(item);
                item.gameObject.AddComponent<Storable_Items_Stats>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
