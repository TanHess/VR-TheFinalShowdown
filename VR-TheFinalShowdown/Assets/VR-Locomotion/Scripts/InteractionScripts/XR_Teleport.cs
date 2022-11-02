using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class XR_Teleport : MonoBehaviour
{
    [SerializeField] float TeleportFadeTime = 0.25f;
    [SerializeField] Color TeleportFadeColor = Color.black;
    [SerializeField] float TeleportVerticalOffset = 0.1f;

    [SerializeField] PlayerBodyController BodyController;

    [SerializeField] XRRayInteractor LeftTeleportRay;
    [SerializeField] XRRayInteractor RightTeleportRay;
    [Header("Left Activate Action")]
    [SerializeField] InputAction LeftTeleportActivate;
    [Header("Left Deactivate Action")]
    [SerializeField] InputAction LeftTeleportDeactivate;
    [Header("Right Activate Action")]
    [SerializeField] InputAction RightTeleportActivate;
    [Header("Right Deactivate Action")]
    [SerializeField] InputAction RightTeleportDeactivate;

    public UnityEvent ActivationEvent;
    public UnityEvent DeactivationEvent;

    [HideInInspector] public bool TeleportStarted;

    private void OnEnable()
    {
        LeftTeleportActivate.Enable();
        LeftTeleportDeactivate.Enable();

        RightTeleportActivate.Enable();
        RightTeleportDeactivate.Enable();

        LeftTeleportActivate.performed += LeftActivated;
        RightTeleportActivate.performed += RightActivated;

        LeftTeleportDeactivate.performed += LeftDeactivated;
        RightTeleportDeactivate.performed += RightDeactivated;

        LeftTeleportRay.enabled = false;
        RightTeleportRay.enabled = false;
    }

    private void LeftActivated(InputAction.CallbackContext obj)
    {
        if (!TeleportStarted)
        {
            RightTeleportRay.enabled = false;
            LeftTeleportRay.enabled = true;
            ActivationEvent.Invoke();

            TeleportStarted = true;
        }
       
    }

    private void LeftDeactivated(InputAction.CallbackContext obj)
    {
        if (TeleportStarted && LeftTeleportRay.enabled)
        {
            LeftTeleportRay.enabled = false;
            TeleportStarted = false;

            StartCoroutine(TeleportRoutine(LeftTeleportRay));
            DeactivationEvent.Invoke();
            
        }
    }

    private void RightActivated(InputAction.CallbackContext obj)
    {
        if (!TeleportStarted)
        {
            RightTeleportRay.enabled = true;
            LeftTeleportRay.enabled = false;
            ActivationEvent.Invoke();

            TeleportStarted = true;
        }
    }

    private void RightDeactivated(InputAction.CallbackContext obj)
    {
        if (TeleportStarted && RightTeleportRay.enabled)
        {
            RightTeleportRay.enabled = false;
            TeleportStarted = false;

            StartCoroutine(TeleportRoutine(RightTeleportRay));
            DeactivationEvent.Invoke();
        }
    }

    private IEnumerator TeleportRoutine(XRRayInteractor Ray)
    {
        yield return new WaitForEndOfFrame();

        List<IXRInteractable> ValidTargets = new List<IXRInteractable>();
        Ray.GetValidTargets(ValidTargets);

        //We're not hovering over a valid surface, or we're in a wall, so call off the whole thing!
        if (ValidTargets.Count < 1 || HeadCollisionManager.HeadColliding)
        {
            yield break;
        }

        //Fade before Teleporting
        ColorFilter_Controller.FadePanelChange(TeleportFadeColor, TeleportFadeTime, true);
        yield return new WaitForSeconds(TeleportFadeTime);
        
        BodyController.PlayerBody.isKinematic = true;
        Transform Head = BodyController.BodySystem.HeadTransform;
        Vector3 TeleportPoint = Vector3.zero;

        //Check again for valid targets to counter tele-spamming
        Ray.GetValidTargets(ValidTargets);
        
        if (ValidTargets.Count < 1 || HeadCollisionManager.HeadColliding)
        {
            yield break;
        }

        if (Ray.TryGetCurrent3DRaycastHit(out RaycastHit Hit))
        {
            TeleportPoint = Hit.point;
        }

        //Get the difference between the center of the Play Area
        Vector3 BetweenHeadAndPoint = (Head.position - BodyController.PlayerBody.position);
        //Negate the Y for simplicity
        BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

        //Raise us up a hair
        Vector3 FootOffset = new Vector3(0, TeleportVerticalOffset, 0);

        //Get the position of our player point and the subtract the difference, sending us right on top of the point!
        BodyController.PlayerBody.position = (TeleportPoint - BetweenHeadAndPoint) + FootOffset;
        
        //Fade back to normal
        ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, TeleportFadeTime, false);
        yield return new WaitForSeconds(TeleportFadeTime);

        BodyController.PlayerBody.isKinematic = false;
        
        yield return null;

    }

}
