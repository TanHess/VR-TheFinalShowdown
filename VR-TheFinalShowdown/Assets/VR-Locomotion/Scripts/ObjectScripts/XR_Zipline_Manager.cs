using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XR_Zipline_Manager : MonoBehaviour
{
    [SerializeField] Transform StartingLocation;
    private Vector3 OriginalPosition;

    [SerializeField] private XRBaseInteractable grabInteractor;
    [SerializeField] private Rigidbody HandleBody;

    [SerializeField] private float ResetCooldown = 1.0f;
    private bool CoolingDown;
    private bool ReadyToCoolDown;
    private float CooldownCounter;
    XRBaseInteractor Interactor => (XRBaseInteractor)grabInteractor.GetOldestInteractorSelecting();

    [Range(0, 1)] [SerializeField] float HapticForce = 0.05f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.05f;

    private void Awake()
    {
        OriginalPosition = StartingLocation.position;
    }

    private void OnEnable()
    {
        grabInteractor.selectEntered.AddListener(Grabbed);
    }
    private void OnDisable()
    {
        grabInteractor.selectEntered.RemoveListener(Grabbed);
    }

    private void Grabbed(SelectEnterEventArgs arg0)
    {
        HandleBody.isKinematic = false;
    }

    public void ResetZipline(Transform Handle)
    {
        Handle.position = OriginalPosition;
        Debug.Log("Reset Zipline");
        HandleBody.isKinematic = true;
    }

    void HapticMaker()
    {
        if (grabInteractor.isSelected)
        {
            float HandleSpeedMultiplier = HandleBody.velocity.magnitude;
            float HandleRumbleForce = HapticForce * HandleSpeedMultiplier;
            HandleRumbleForce = Mathf.Clamp(HandleRumbleForce, 0, 1);
            Interactor.GetComponent<XRDirectInteractor>().SendHapticImpulse(HandleRumbleForce, HapticDuration);
        }
    }

    void CooldownManager()
    {
        if(grabInteractor.isSelected && !CoolingDown)
        {
            ReadyToCoolDown = true;
        }

        if (!grabInteractor.isSelected && !CoolingDown && ReadyToCoolDown)
        {
            CoolingDown = true;
        }

        if (grabInteractor.isSelected && CoolingDown)
        {
            CooldownCounter = 0;
            CoolingDown = false;
        }

        if (CooldownCounter < ResetCooldown && CoolingDown)
        {
            CooldownCounter += Time.deltaTime;
        }

        if(CooldownCounter > ResetCooldown)
        {
            CooldownCounter = 0;
            CoolingDown = false;
            ResetZipline(HandleBody.transform);

            ReadyToCoolDown = false;
        }

    }

    private void LateUpdate()
    {
        HapticMaker();
        CooldownManager();
    }
}
