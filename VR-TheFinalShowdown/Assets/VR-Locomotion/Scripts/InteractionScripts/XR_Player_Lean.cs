using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XR_Player_Lean : MonoBehaviour
{
    [SerializeField] BodyAlignmentSystem BodyAlignmentSystem;
    [SerializeField] PlayerBodyController PlayerBodyController;
    [Header("The player will be able to stray from their original point this far before getting kicked out")]
    [Range(0.001f, 1)] public float LeanAllowance;


    [Header("Activation Action")]
    [SerializeField] InputAction ActivationAction;
    [Header("Deactivation Action")]
    [SerializeField] InputAction DeactivationAction;

    private bool ShouldActivate;
    private bool Active;
    private Vector3 OriginalPosition;
    private void OnEnable()
    {
        ActivationAction.Enable();
        DeactivationAction.Enable();

        ActivationAction.performed += ActivationAction_performed;
        DeactivationAction.performed += DeactivationAction_performed;
    }

    private void OnDisable()
    {
        ActivationAction.Disable();
        DeactivationAction.Disable();

        ActivationAction.performed -= ActivationAction_performed;
        DeactivationAction.performed -= DeactivationAction_performed;
    }

    private void ActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (!ShouldActivate && PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded)
        {
            Vector3 TempPoint = BodyAlignmentSystem.HeadTransform.position;
            TempPoint.y = 0;
            OriginalPosition = TempPoint;

            PlayerBodyController.PlayerBody.isKinematic = true;

            ShouldActivate = true;
            Debug.Log("ACTIVATE");
            
        }  
    }

    private void DeactivationAction_performed(InputAction.CallbackContext obj)
    {
        if (ShouldActivate)
        {
            ShouldActivate = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldActivate)
        {
            if (!Active)
            {
                Active = true;
            }

            if (!HeadCollisionManager.HeadColliding)
            {
                Vector3 TempPoint = BodyAlignmentSystem.HeadTransform.position;
                TempPoint.y = 0;

                if (Vector3.Distance(OriginalPosition, TempPoint) > LeanAllowance)
                {
                    ShouldActivate = false;
                    return;
                }

            }
            else 
            {
                ShouldActivate = false;
                return;
            }
        }
        else 
        {
            if (Active)
            {
                Debug.Log("DEACTIVATE");
                PlayerBodyController.PlayerBody.isKinematic = false;
                Active = false;
            }
           
        }
    }
}
