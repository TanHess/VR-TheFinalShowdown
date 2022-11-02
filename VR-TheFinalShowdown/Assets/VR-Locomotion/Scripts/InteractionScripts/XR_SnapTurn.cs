using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XR_SnapTurn : MonoBehaviour
{
    [Header("Turn Axis")]
    [SerializeField] InputAction SnapActivationAxis;
    [Header("Turn Action")]
    [SerializeField] InputAction ActivationAction;
    [SerializeField] Transform RotationPoint;
    [SerializeField] Rigidbody PlayerBody;
    [SerializeField] int RotationAmount = 45;
    [Range(0, 1)] [SerializeField] float StickDeadzone = 0.5f;

    [SerializeField] XR_Climb ClimbComponent;

    [SerializeField] bool StopSwimMomentum;

    private Vector2 MoveInput;
    // Start is called before the first frame update
    void Awake()
    {
        SnapActivationAxis.Enable();
        ActivationAction.Enable();
        SnapActivationAxis.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        ActivationAction.performed += ActivationAction_performed;
    }

    private void ActivationAction_performed(InputAction.CallbackContext obj)
    {
        if ((PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded
            || PlayerBodyController.State == PlayerBodyController.PlayerState.InWater
            || PlayerBodyController.State == PlayerBodyController.PlayerState.Falling)
            && PlayerBodyController.State != PlayerBodyController.PlayerState.Climbing)
        {
            if (MoveInput.x > StickDeadzone)
            {
                PlayerBody.transform.RotateAround(RotationPoint.transform.position, Vector3.up, RotationAmount);
            }
            else if (MoveInput.x < -StickDeadzone)
            {
                PlayerBody.transform.RotateAround(RotationPoint.transform.position, Vector3.up, -RotationAmount);
            }

            if (PlayerBodyController.State == PlayerBodyController.PlayerState.InWater && StopSwimMomentum)
            {
                PlayerBody.velocity = Vector3.zero;
            }
        }
    }
}
