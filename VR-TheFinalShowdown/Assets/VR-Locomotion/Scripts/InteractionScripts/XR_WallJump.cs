using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class XR_WallJump : MonoBehaviour
{
    [SerializeField] PlayerBodyController BodyController;
    [SerializeField] XR_Climb ClimbComponent;
    [SerializeField] Rigidbody PlayerBody;
    [SerializeField] Transform PlayerHead;

    [SerializeField] XRController LeftXRController;
    [SerializeField] XRController RightXRController;

    [SerializeField] XRDirectInteractor LeftInteractor;
    [SerializeField] XRDirectInteractor RightInteractor;

    [SerializeField] float MinThrust;
    
    public float WallOffJumpPower;
    public float WallUpJumpPower;

    private bool LeftArmSwingDown;
    private bool RightArmSwingDown;

    [SerializeField] InputAction LeftActivationAction;
    [SerializeField] InputAction RightActivationAction;

    [Header("Amount of time before not swinging anymore")]
    [SerializeField] private float SwingLeeway;

    private Vector3 LeftVelocity;
    private Vector3 RightVelocity;
    [HideInInspector] public bool WallJumped;

    private float SwingLeewayCounter;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.25f;

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
        //Debug.Log("Pressed_Left");
        if (PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded && LeftArmSwingDown && ClimbComponent.IsClimbing)
        {
           // Debug.Log("Left Wall Jump");
            ClimbComponent.EndClimbing(RightInteractor);
            PerformWallJump();
            RightInteractor.SendHapticImpulse(HapticForce, HapticDuration);
        }
    }

    private void RightActivationAction_performed(InputAction.CallbackContext obj)
    {
        //Debug.Log("Pressed_Right");
        if (PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded && RightArmSwingDown && ClimbComponent.IsClimbing)
        {
           // Debug.Log("Right Wall Jump");
            ClimbComponent.EndClimbing(LeftInteractor);
            PerformWallJump();
            LeftInteractor.SendHapticImpulse(HapticForce, HapticDuration);
        }
    }

    public void PerformWallJump()
    {
        if (!HeadCollisionManager.HeadColliding)
        {
            WallJumped = true;

            Vector3 HeadForward = XR_Movement_Controller.ConstrainedForward(PlayerHead);
            HeadForward *= WallOffJumpPower;
            HeadForward.y = WallUpJumpPower;
            PlayerBody.AddForce(HeadForward , ForceMode.VelocityChange);

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
        if (WallJumped && PlayerBody.velocity.y > 0)
        {
            WallJumped = false;
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

        //Stop running
        if (LeftVelocity.magnitude < MinThrust && RightVelocity.magnitude < MinThrust && SwingLeewayCounter > SwingLeeway)
        {
            LeftArmSwingDown = false;
            RightArmSwingDown = false;
        }

    }

}
