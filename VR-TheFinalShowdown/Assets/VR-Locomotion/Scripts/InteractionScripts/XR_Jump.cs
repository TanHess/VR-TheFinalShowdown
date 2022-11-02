using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class XR_Jump : MonoBehaviour
{
    [SerializeField] PlayerBodyController BodyController;

    [SerializeField] Rigidbody PlayerBody;

    [SerializeField] XRController LeftXRController;
    [SerializeField] XRController RightXRController;

    [SerializeField] XRDirectInteractor LeftInteractor;
    [SerializeField] XRDirectInteractor RightInteractor;

    [SerializeField] float MinThrust;
    
    public float JumpPower;

    private bool LeftArmSwingDown;
    private bool RightArmSwingDown;

    [Header("Assists in premature 'grounded' misfires on slopes")]
    [SerializeField] float InitialStartHeight = 0.1f;

    [Header("Jump Action")]
    [SerializeField] InputAction LeftActivationAction;
    [SerializeField] InputAction RightActivationAction;

    [Header("Amount of time before not swinging anymore")]
    [SerializeField] private float SwingLeeway;

    private Vector3 LeftVelocity;
    private Vector3 RightVelocity;
    [HideInInspector] public bool Jumped;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.25f;

    private float SwingLeewayCounter;

    public UnityEvent JumpEvent;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        LeftActivationAction.Enable();
        LeftActivationAction.performed += LeftActivationAction_performed;

        RightActivationAction.Enable();
        RightActivationAction.performed += RightActivationAction_performed;
    }

    private void LeftActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded && LeftArmSwingDown && !IsTouchingObject(LeftInteractor))
        {
            PerformJump();
            RightInteractor.SendHapticImpulse(HapticForce, HapticDuration);
        }
    }

    private void RightActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded && RightArmSwingDown && !IsTouchingObject(RightInteractor))
        {
            PerformJump();
            LeftInteractor.SendHapticImpulse(HapticForce, HapticDuration);
        }
    }

    public void PerformJump()
    {
        if (!HeadCollisionManager.HeadColliding)
        {
            Jumped = true;

            //Raise us up a hair to take us off the ground just enough to prevent grounded misfires
            Vector3 StartPoint = PlayerBody.position;
            StartPoint.y += InitialStartHeight;
            PlayerBody.position = StartPoint;

            PlayerBody.AddForce(Vector3.up * JumpPower, ForceMode.VelocityChange);
            
            JumpEvent.Invoke();
            PlayerBodyController.State = PlayerBodyController.PlayerState.Jumping;
        }
    }

    void FixedUpdate()
    {
        //Gets the Controller Velocity
        LeftXRController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out LeftVelocity);
        RightXRController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out RightVelocity);
    }

    public bool IsTouchingObject(XRBaseInteractor Interactor)
    {
        List<IXRHoverInteractable> lists = new List<IXRHoverInteractable>();
        lists = Interactor.interactablesHovered;
        if (lists.Count > 0)
        {
            return true;
        }

            return false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Jumped && PlayerBody.velocity.y > 0)
        {
            Jumped = false;
        }

        //Get if we swung down
        if (LeftVelocity.y < -MinThrust && !LeftArmSwingDown)
        {
            LeftArmSwingDown = true;
            RightArmSwingDown = false;
        }

        if (RightVelocity.y < -MinThrust && !RightArmSwingDown)
        {
            LeftArmSwingDown = false;
            RightArmSwingDown = true;
        }

        //Count up the Leeway counter
        SwingLeewayCounter += Time.deltaTime;

        if (LeftVelocity.magnitude < MinThrust && RightVelocity.magnitude < MinThrust && SwingLeewayCounter > SwingLeeway)
        {
            LeftArmSwingDown = false;
            RightArmSwingDown = false;
        }

    }

}
