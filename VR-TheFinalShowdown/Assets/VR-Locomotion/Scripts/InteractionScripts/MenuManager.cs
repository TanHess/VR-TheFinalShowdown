using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class MenuManager : MonoBehaviour
{
    [Header("Left Activate Action")]
    [SerializeField] InputAction LeftActivationAction;
    [Header("Right Activate Action")]
    [SerializeField] InputAction RightActivationAction;
    [Header("Left Deactivate Action")]
    [SerializeField] InputAction LeftDeactivationAction;
    [Header("Right Deactivate Action")]
    [SerializeField] InputAction RightDeactivationAction;

    [Header("The player has to wait this long until they're allowed to switch")]
    [SerializeField] float ScannerSwitchAllowedTime;

    public UnityEvent LeftActivated;
    public UnityEvent LeftDectivated;

    public UnityEvent RightActivated;
    public UnityEvent RightDectivated;

    bool Activated;

    private float AntiSpamCounter;

    // Start is called before the first frame update
    void OnEnable()
    {
        LeftActivationAction.Enable();
        LeftActivationAction.performed += LeftActivationAction_performed;

        RightActivationAction.Enable();
        RightActivationAction.performed += RightActivationAction_performed;

        LeftDeactivationAction.Enable();
        LeftDeactivationAction.performed += LeftDectivationAction_performed;

        RightDeactivationAction.Enable();
        RightDeactivationAction.performed += RightDectivationAction_performed;
    }

    private void OnDisable()
    {
        LeftActivationAction.Disable();
        LeftActivationAction.performed -= LeftActivationAction_performed;

        RightActivationAction.Disable();
        RightActivationAction.performed -= RightActivationAction_performed;

        LeftDeactivationAction.Disable();
        LeftDeactivationAction.performed -= LeftDectivationAction_performed;

        RightDeactivationAction.Disable();
        RightDeactivationAction.performed -= RightDectivationAction_performed;
    }

    private void LeftActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (!Activated && AntiSpamCounter >= ScannerSwitchAllowedTime)
        {
            LeftActivated.Invoke();
            Activated = true;
        }
    }

    private void RightActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (!Activated && AntiSpamCounter >= ScannerSwitchAllowedTime)
        {
            RightActivated.Invoke();
            Activated = true;
        }
    }

    private void LeftDectivationAction_performed(InputAction.CallbackContext obj)
    {
        if (Activated)
        {
            LeftDectivated.Invoke(); 
            Activated = false;
            AntiSpamCounter = 0;
        }
    }

    private void RightDectivationAction_performed(InputAction.CallbackContext obj)
    {
        if (Activated)
        {
            RightDectivated.Invoke();
            Activated = false;
            AntiSpamCounter = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Count up the Anti-Spam counter
        AntiSpamCounter += Time.deltaTime;
    }
}
