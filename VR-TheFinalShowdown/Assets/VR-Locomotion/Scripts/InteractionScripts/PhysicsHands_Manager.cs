using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

[RequireComponent(typeof(XRDirectInteractor))]

public class PhysicsHands_Manager : MonoBehaviour
{
    [Header("Makes this Rigidbody's Mass = 0 while selecting an object")]

    public Rigidbody HandBody;
    public Joint MainHandJoint;

    private bool Hovering;
    private XRDirectInteractor interactor = null;
    private float OriginalMass;

    public bool DisableHandOnGrab = false;
    [SerializeField] List<GameObject> ObjectsToDisableOnGrab;
    [SerializeField] List<Collider> HandColliders;
    private string ControllerDeviceName;

    private enum ControllerType
    {
        Quest,
        RiftS,
        ViveWand,
        Index,
        WMR,
        Focus3
    }

    [Serializable]
    private class ControllerOffset
    {
        public ControllerType Type;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
    }

    [Header("Moves the hand based on the controller type")]
    [SerializeField] Transform HandTarget;

    [SerializeField] List<ControllerOffset> ControllerList;

    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
        OriginalMass = HandBody.mass;
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(Hide);
        interactor.selectExited.AddListener(Show);
        interactor.hoverEntered.AddListener(SetHovering);
        interactor.hoverExited.AddListener(UnsetHovering);
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(Hide);
        interactor.selectExited.RemoveListener(Show);
        interactor.hoverEntered.RemoveListener(SetHovering);
        interactor.hoverExited.RemoveListener(UnsetHovering);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ControllerTypeDetect());
    }

    IEnumerator ControllerTypeDetect()
    {
        yield return new WaitForSeconds(2f);

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            ControllerDeviceName = device.name;
        }

        if (ControllerDeviceName == null || ControllerDeviceName == " Head Tracking - OpenXR")
        {
            Debug.LogError("No controller detected. Is your headset running and on your face?");
        }

        if (ControllerDeviceName.ToLower().Contains("oculus") && Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Set hand orientation to Quest");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.Quest);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.Quest);
        }

        if (ControllerDeviceName.ToLower().Contains("oculus") && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer))
        {
            Debug.Log("Set hand orientation to Rift");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.RiftS);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.RiftS);
        }

        if (ControllerDeviceName.ToLower().Contains("vive"))
        {
            Debug.Log("Set hand orientation to Vive Wand");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.ViveWand);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.ViveWand);
        }

        if (ControllerDeviceName.ToLower().Contains("vive") && Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Set hand orientation to Vive Focus 3");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.Focus3);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.Focus3);
        }

        if (ControllerDeviceName.ToLower().Contains("index"))
        {
            Debug.Log("Set hand orientation to Knuckles");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.Index);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.Index);
        }
        if (ControllerDeviceName.ToLower().Contains("windows"))
        {
            Debug.Log("Set hand orientation to WMR");
            HandTarget.localPosition = ReturnControllerPositionOffset(ControllerType.WMR);
            HandTarget.localEulerAngles = ReturnControllerRotationOffset(ControllerType.WMR);
        }
    }

    private void SetHovering(HoverEnterEventArgs arg0)
    {
        //Only perform this with Grabbables
        if (!arg0.interactableObject.GetType().IsSubclassOf(typeof(XRGrabInteractable)))
        {
            return;
        }
        Hovering = true;
    }

    private void UnsetHovering(HoverExitEventArgs arg0)
    {
        //Only perform this with Grabbables
        if (!arg0.interactableObject.GetType().IsSubclassOf(typeof(XRGrabInteractable)))
        {
            return;
        }
        Hovering = false;
    }

    private void Hide(SelectEnterEventArgs arg0)
    {
        //Only perform this with Grabbables
        if (!arg0.interactableObject.GetType().IsSubclassOf(typeof(XRGrabInteractable)))
        {
            return;
        }

        if (DisableHandOnGrab)
        {
            for (int i = 0; i < ObjectsToDisableOnGrab.Count; i++)
            {
                ObjectsToDisableOnGrab[i].SetActive(false);
            }
        }
        HandBody.mass = 0;

        for (int i = 0; i < HandColliders.Count; i++)
        {
            HandColliders[i].enabled = false;
        }
    }

    private void Show(SelectExitEventArgs arg0)
    {
        //Only perform this with Grabbables
        if (!arg0.interactableObject.GetType().IsSubclassOf(typeof(XRGrabInteractable)))
        {
            return;
        }

        if (DisableHandOnGrab)
        {
            for (int i = 0; i < ObjectsToDisableOnGrab.Count; i++)
            {
                ObjectsToDisableOnGrab[i].SetActive(true);
            }
        }

        StartCoroutine(WaitForRange());
    }

    Vector3 ReturnControllerPositionOffset(ControllerType Device)
    {
        Vector3 Final = Vector3.zero;

        foreach (var TypeOfController in ControllerList)
        {
            if (TypeOfController.Type == Device)
            {
                Final = TypeOfController.PositionOffset;
            }
        }

        return Final;
    }
    
    Vector3 ReturnControllerRotationOffset(ControllerType Device)
    {
        Vector3 Final = Vector3.zero;

        foreach (var TypeOfController in ControllerList)
        {
            if (TypeOfController.Type == Device)
            {
                Final = TypeOfController.RotationOffset;
            }
        }

        return Final;
    }

    private IEnumerator WaitForRange()
    {
        yield return new WaitWhile(() => Hovering);

        HandBody.mass = OriginalMass;
        for (int i = 0; i < HandColliders.Count; i++)
        {
            HandColliders[i].enabled = true;
        }
    }
}
