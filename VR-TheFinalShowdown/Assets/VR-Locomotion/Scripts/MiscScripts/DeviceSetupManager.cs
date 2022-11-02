using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;
using UnityEngine.InputSystem;
using TMPro;
public class DeviceSetupManager : MonoBehaviour
{
    [Header("Your headset will go as high as it can, capping at this.")]
    public int DesiredFPS = 90;
    public bool EnableFFROnOculus = true;
    public int Level = 2;
    [SerializeField] TextMeshPro DebugFPS;
    [SerializeField] TextMeshPro DebugTimestep;
    [SerializeField] TextMeshPro DebugFFRLevel;

    private XRController DetectedController => FindObjectOfType<XRController>();
    [HideInInspector] public string ControllerDeviceName;

    public static bool IsOculus;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ControllerTypeDetect());  
    }

    public void SetFFRLevel(int Level)
    {
        Unity.XR.Oculus.Utils.EnableDynamicFFR(true);
        Unity.XR.Oculus.Utils.SetFoveationLevel(Level);  
    }

    IEnumerator ControllerTypeDetect()
    {
        yield return new WaitForSeconds(3f);

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            ControllerDeviceName = device.name;
        }

        if (ControllerDeviceName == null)
        {
            Debug.LogError("Hey! You need to have the controller turned on and tracking first.");
        }

        Debug.Log("Controller Device Name = " + ControllerDeviceName);

        if (ControllerDeviceName.ToLower().Contains("oculus"))
        {
            Debug.Log("ACTIVATE OCULUS SETTINGS");
            IsOculus = true;
            SetFFRLevel(Level);
        }

        // Automatically detect the FPS so we can set the physics timestep accordingly!
        if (IsOculus)
        {
            //Allows tracking when Universal Menu is open
            Application.runInBackground = true;

            Unity.XR.Oculus.Performance.TryGetAvailableDisplayRefreshRates(out float[] refreshRates);
            
            int HighestPossibleFPS = 0;
            //We iterate through the list of possible refresh rates, going as high as we can before hitting the cap.
            for (int i = 0; i < refreshRates.Length; i++)
            {
                if (DesiredFPS >= (int)refreshRates[i])
                {
                    Debug.Log(refreshRates[i]+" FPS Detected. " + DesiredFPS + " Desired.");
                    HighestPossibleFPS = (int)refreshRates[i];
                }

            }
            Debug.Log("Set FPS to " + DesiredFPS);
            DesiredFPS = HighestPossibleFPS;
        }
        else
        {
            var xrDisplay = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
            if (xrDisplay != null)
            {
                if (xrDisplay.TryGetDisplayRefreshRate(out float refreshRate))
                {
                    DesiredFPS = (int)refreshRate;
                    Debug.Log($"Refresh Rate: {refreshRate}hz");
                }
            }
        }
        StartCoroutine(DisplayRoutine(DesiredFPS));
    }

    IEnumerator DisplayRoutine(float FPS)
    {
        yield return new WaitForEndOfFrame();

        //Checks every second to check the status of things.
        while (true)
        {

            Unity.XR.Oculus.Performance.TryGetDisplayRefreshRate(out float CurrentRate);
            if ((int)CurrentRate != (int)FPS)
            {
                Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(FPS);
            }

            //The FPS hasn't changed since the last cycle, so we're good to exit early.
            if ((int)CurrentRate == (int)FPS)
            {
                break;
            }

            int CurrentTimestep = (int)(1/Time.fixedDeltaTime);
            //The Timestep hasn't changed since the last cycle, so we're good to exit early.
            if (CurrentTimestep == (int)(1 / Time.fixedDeltaTime))
            {
                break;
            }

            Debug.Log("Fixed Timestep is oriented to " + CurrentTimestep + " FPS.");
            Debug.Log("We want it to be " + FPS);

            //Assign the Physics Timestep
            if (CurrentTimestep != FPS)
            {
                Debug.Log("Timestep assigned to " + 1f / FPS);

                Time.fixedDeltaTime = 1f / FPS;
                Time.maximumDeltaTime = 1f / FPS;
            }

           

            if (DebugFPS != null)
            {
                DebugFPS.text = CurrentRate.ToString();
            }

            if (DebugTimestep != null)
            {
                DebugTimestep.text = Time.fixedDeltaTime.ToString("F2");
            }

            if (DebugFFRLevel != null && IsOculus)
            {
                DebugFFRLevel.text = Unity.XR.Oculus.Utils.GetFoveationLevel().ToString();
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
