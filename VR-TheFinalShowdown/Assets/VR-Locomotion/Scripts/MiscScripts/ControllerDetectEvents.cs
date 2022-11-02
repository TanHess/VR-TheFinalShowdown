using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class ControllerDetectEvents : MonoBehaviour
{
    private XRController DetectedController => FindObjectOfType<XRController>();
    private string ControllerDeviceName;

    [Header("Controller Events")]
    public UnityEvent OculusEvent;
    public UnityEvent ViveEvent;
    public UnityEvent KncukleEvent;
    public UnityEvent WMREvent;
    public UnityEvent GenericEvent;
    private bool NoneDetected;

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

        NoneDetected = true;

        if (ControllerDeviceName.ToLower().Contains("oculus"))
        {
            OculusEvent.Invoke();
            NoneDetected = false;
        }
        if (ControllerDeviceName.ToLower().Contains("vive"))
        {
            ViveEvent.Invoke();
            NoneDetected = false;
        }
        if (ControllerDeviceName.ToLower().Contains("index"))
        {
            KncukleEvent.Invoke();
            NoneDetected = false;
        }
        if (ControllerDeviceName.ToLower().Contains("windows"))
        {
            WMREvent.Invoke();
            NoneDetected = false;
        }
        if (NoneDetected)
        {
            GenericEvent.Invoke();
        }
    }
}
