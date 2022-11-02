using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
public class XR_Arc_Launch : MonoBehaviour
{
    public PlayerBodyController BodyController;
    [SerializeField] XR_Climb ClimbComponent;
    public Rigidbody PlayerBody;

    [Header("Left Activate Action")]
    [SerializeField] InputAction LeftTeleportActivate;
    [Header("Left Deactivate Action")]
    [SerializeField] InputAction LeftTeleportDeactivate;
    [Header("Right Activate Action")]
    [SerializeField] InputAction RightTeleportActivate;
    [Header("Right Deactivate Action")]
    [SerializeField] InputAction RightTeleportDeactivate;

    [SerializeField] XRDirectInteractor LeftHandInteractor;
    [SerializeField] XRDirectInteractor RightHandInteractor;

    public UnityEvent ActivationEvent;
    public UnityEvent DeactivationEvent;

    [HideInInspector] public bool AimStarted;
    private bool LeftActive;
    private bool RightActive;

    [SerializeField] Transform LeftAimer;
    [SerializeField] Transform RightAimer;
    public Transform CurrentAimer;

    [Header("Assists in premature 'grounded' misfires on slopes")]
    [SerializeField] float InitialStartHeight = 0.1f;

    [Range(3, 10)] public float MinJumpPower = 4;
    [Range(3, 10)] public float MaxJumpPower = 8;
    [HideInInspector] public float CurrentChargePower;
    float RawChargeTime;
    [SerializeField] float ChargeTime = 3;

    [Header("Charge Haptics")]
    [Range(0, 5)] [SerializeField] float HapticForceMultiplier = 1.0f;

    private float CurrentChargeTime;
    private float ChargePercent;
    public UnityEvent JumpEvent;
    [Header("Allows for Double and Triple-jumping")]
    [Range(0, 3)] public int MaxJumpCount = 1;
    private int JumpCounter;
    private bool Jumped;
    [Header("Forces the player to point into the air")]
    [Range(-1, 1)] [SerializeField] float DownwardPointThreshold = -0.5f;

    [SerializeField] private LineRenderer Line;
    [SerializeField] Gradient ValidColor;
    [SerializeField] Gradient InvalidColor;

    private void Awake()
    {
        LeftTeleportActivate.Enable();
        LeftTeleportDeactivate.Enable();

        RightTeleportActivate.Enable();
        RightTeleportDeactivate.Enable();

        LeftTeleportActivate.performed += LeftActivated;
        RightTeleportActivate.performed += RightActivated;

        LeftTeleportDeactivate.performed += LeftDeactivated;
        RightTeleportDeactivate.performed += RightDeactivated;

        JumpCounter = MaxJumpCount;
    }

    private void LeftActivated(InputAction.CallbackContext obj)
    {
        if (!AimStarted && JumpCounter > 0)
        {
            ActivationEvent.Invoke();
            CurrentAimer = LeftAimer;
            AimStarted = true;
            LeftActive = true;

            Line.enabled = true;
        }

    }

    private void LeftDeactivated(InputAction.CallbackContext obj)
    {
        if (AimStarted && LeftActive)
        {
            LeftActive = false;
            AimStarted = false;
            
            PerformJump(CurrentAimer);

            DeactivationEvent.Invoke();

            CurrentChargeTime = 0;
            RawChargeTime = 0;
            CurrentChargePower = 0;

        }
    }

    private void RightActivated(InputAction.CallbackContext obj)
    {
        if (!AimStarted && JumpCounter > 0)
        {
            ActivationEvent.Invoke();
            CurrentAimer = RightAimer;
            AimStarted = true;
            RightActive = true;

            Line.enabled = true;
        }
    }

    private void RightDeactivated(InputAction.CallbackContext obj)
    {
        if (AimStarted && RightActive)
        {
            RightActive = false;
            AimStarted = false;
            PerformJump(CurrentAimer);

            DeactivationEvent.Invoke();

            CurrentChargeTime = 0;
            RawChargeTime = 0;
            CurrentChargePower = 0;

        }
    }
    public void PerformJump(Transform Aimer)
    {

        if (!HeadCollisionManager.HeadColliding && !PointingDown(Aimer.forward))
        {

            Jumped = true;

            //Raise us up a hair to take us off the ground just enough to prevent grounded misfires
            Vector3 StartPoint = PlayerBody.position;
            StartPoint.y += InitialStartHeight;
            PlayerBody.position = StartPoint;

            //Negates the current velocity so double-jumps can work
            PlayerBody.velocity = Vector3.zero;
            PlayerBody.AddForce(Aimer.forward * CurrentChargePower, ForceMode.VelocityChange);

            JumpEvent.Invoke();
            PlayerBodyController.State = PlayerBodyController.PlayerState.Jumping;

            JumpCounter--;

            JumpEvent.Invoke();
            Jumped = true;

            ClimbComponent.EndClimbingAll();
            
        }

        Line.enabled = false;

    }

    IEnumerator DelayedJump(Transform Aimer)
    {
        //Raise us up a hair to take us off the ground just enough to prevent grounded misfires
        Vector3 StartPoint = PlayerBody.position;
        StartPoint.y += InitialStartHeight;
        PlayerBody.position = StartPoint;
        PlayerBodyController.State = PlayerBodyController.PlayerState.Jumping;
        yield return new WaitForEndOfFrame();

        //Negates the current velocity so double-jumps can work
        PlayerBody.velocity = Vector3.zero;
        PlayerBody.AddForce(Aimer.forward * CurrentChargePower, ForceMode.VelocityChange);
    }

    private void Update()
    {
        if (AimStarted)
        {
            //Count up
            CurrentChargeTime += Time.deltaTime;
            RawChargeTime += Time.deltaTime;

            //Start at a time that syncs with the minimum charge power
            float AdjustedStartTime = (MinJumpPower / MaxJumpPower) * ChargeTime;

            //Clamp the charge time
            CurrentChargeTime = Mathf.Clamp(CurrentChargeTime, AdjustedStartTime, ChargeTime);

            //We have a seperate counter that isn't adjusted, to use for Haptics
            RawChargeTime = Mathf.Clamp(RawChargeTime, 0, ChargeTime);

            //Get the percent to complete
            ChargePercent = Mathf.Clamp(CurrentChargeTime / ChargeTime, 0, 1);
            float RawChargePercent = Mathf.Clamp(RawChargeTime / ChargeTime, 0, 1);

            //Apply the multiplier
            CurrentChargePower = Mathf.Clamp(MaxJumpPower * ChargePercent, MinJumpPower, MaxJumpPower);

            //Manage valid targets
            if (PointingDown(CurrentAimer.forward))
            {
                Line.colorGradient = InvalidColor;
            }
            else 
            {
                Line.colorGradient = ValidColor;
            }

            //Haptics
            if (LeftActive)
            {
                LeftHandInteractor.SendHapticImpulse(RawChargePercent * HapticForceMultiplier, Time.deltaTime);
            }
            if (RightActive)
            {
                RightHandInteractor.SendHapticImpulse(RawChargePercent * HapticForceMultiplier, Time.deltaTime);
            }
        }
        //Resetting after landing
        if ((PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded || ClimbComponent.IsClimbing) && JumpCounter < MaxJumpCount)
        {
            JumpCounter = MaxJumpCount;
        }
    }

    private bool PointingDown(Vector3 Direction)
    {
        bool result = false;

        if (Vector3.Dot(Direction, Vector3.down) > DownwardPointThreshold)
        {
            result = true;
        }

        return result;
    }
}
