using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections;
public class XR_Movement_Controller : MonoBehaviour
{
    [SerializeField] Transform PlayerHead;

    [SerializeField] BodyAlignmentSystem BodyAlignmentSystem;
    [SerializeField] PlayerBodyController BodyController;

    public Rigidbody PlayerBody;

    [SerializeField] XRController LeftXRController;
    [SerializeField] XRController RightXRController;

    [Tooltip("You have to swing your arms downward more than this many m/s")]
    [Range(0, 5)] [SerializeField] float MinThrust;
    [HideInInspector] public float LerpedSpeed;
    [Range(0, 20)] [SerializeField] float LerpingAcceleration;

    private bool LeftArmSwingDown;
    private bool RightArmSwingDown;
    [Header("This amount of time is allowed to pass between arm swings before Running is false")]
    [Range(0, 1)] [SerializeField] private float SwingLeeway;
    [Header("Activation Action")]
    [SerializeField] InputAction ActivationAction;
    [Header("Deactivation Action")]
    [SerializeField] InputAction DeactivationAction;

    [HideInInspector] private Vector3 LeftVelocity;
    [HideInInspector] private Vector3 RightVelocity;
    [Header("Move Action")]
    [SerializeField] InputAction MoveAction;

    enum WalkingType
    {
        BodyForward,
        HeadForward,
        LeftHandForward,
        RightHandForward
    };

    [SerializeField] WalkingType WalkingMethod = WalkingType.BodyForward;

    [Tooltip("How fast you can walk.")]
    [Range(0, 10)] public float MaxWalkSpeed;
    [Tooltip("Controls how quickly you can strafe compared to your forward walk speed.")]
    [Range(0, 1)] public float StrafeMultipler;
    [Tooltip("How fast you can run.")]
    [Range(0, 20)] public float MaxRunSpeed;
    [HideInInspector] public float CurrentRunSpeed;

    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public Vector3 LeftControllerPosition;
    [HideInInspector] public Vector3 RightControllerPosition;
    [HideInInspector] public Vector3 CurrentDirectionVector;

    [Header("The player's body forward must be pointing at the headset's forward with this amount of accuracy")]
    [Range(0, 1)] [SerializeField] private float PointTolerance = 0.5f;

    [Tooltip("Modfies your turn sharpness as well as the speed that you switch between your walk and run directions.")]
    [Range(0, 1)] [SerializeField] float ChangeDirectionSpeed = 0.25f;

    [HideInInspector] public bool WalkingActivated;
    [HideInInspector] public bool Running;

    private float SwingLeewayCounter;

    public UnityEvent ActivationDebugEvent;
    public UnityEvent DeactivationDebugEvent;
    public UnityEvent MoveActionDebugEvent;

    private void Awake()
    {
        ActivationAction.Enable();
        DeactivationAction.Enable();
        MoveAction.Enable();
        
        ActivationAction.performed += ActivationAction_performed;
        DeactivationAction.performed += DeactivationAction_performed;

        MoveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();

    }

    private void ActivationAction_performed(InputAction.CallbackContext obj)
    {
        WalkingActivated = true;
        MoveInput = Vector2.zero;
        ActivationDebugEvent.Invoke();
    }

    private void DeactivationAction_performed(InputAction.CallbackContext obj)
    {
        WalkingActivated = false;
        MoveInput = Vector2.zero;
        DeactivationDebugEvent.Invoke();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        MoveActionDebugEvent.Invoke();
    }

    void FixedUpdate()
    {
        //Gets the Controller Velocity
        LeftXRController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out LeftVelocity);
        RightXRController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out RightVelocity);

        float StickInputY = MoveInput.y;
        float StickInputX = MoveInput.x;

        //Modfies your turn sharpness as well as the speed that you switch between your walk and run directions.
        if (Running)
        {
            CurrentDirectionVector = Vector3.Lerp(CurrentDirectionVector, ConstrainedForward(PlayerHead), ChangeDirectionSpeed);
        }
        //Changes what dictates your forward direction while walking.
        else 
        {
            if (WalkingMethod == WalkingType.BodyForward)
            {
                CurrentDirectionVector = Vector3.Lerp(CurrentDirectionVector, BodyAlignmentSystem.AverageHandForward, ChangeDirectionSpeed);
            }
            if (WalkingMethod == WalkingType.LeftHandForward)
            {
                CurrentDirectionVector = Vector3.Lerp(CurrentDirectionVector, ConstrainedForward(BodyAlignmentSystem.LeftController.transform), ChangeDirectionSpeed);
            }
            if (WalkingMethod == WalkingType.RightHandForward)
            {
                CurrentDirectionVector = Vector3.Lerp(CurrentDirectionVector, ConstrainedForward(BodyAlignmentSystem.RightController.transform), ChangeDirectionSpeed);
            }
            if (WalkingMethod == WalkingType.HeadForward)
            {
                CurrentDirectionVector = Vector3.Lerp(CurrentDirectionVector, ConstrainedForward(PlayerHead), ChangeDirectionSpeed);
            }

        }

        if (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded && !HeadCollisionManager.HeadColliding)
        {
            if (Running)
            {   
                //Makes us run using the head's forward
                PlayerBody.AddForce(CurrentDirectionVector * LerpedSpeed * StickInputY, ForceMode.VelocityChange); 
            }
            else
            {
                if (WalkingMethod == WalkingType.BodyForward)
                {
                    //Get the Dot Product compared to the head forward. This extra checks prevents walking backward when your hands are pointing back.
                    float BodyDot = Vector3.Dot(BodyAlignmentSystem.AverageHandForward.normalized, ConstrainedForward(PlayerHead).normalized);

                    if (BodyDot > PointTolerance)
                    {
                        //Makes us walk using the general body forward
                        PlayerBody.AddForce(CurrentDirectionVector * LerpedSpeed * StickInputY, ForceMode.VelocityChange);
                        PlayerBody.AddForce(Quaternion.Euler(0, 90, 0) * CurrentDirectionVector * LerpedSpeed * StrafeMultipler * StickInputX, ForceMode.VelocityChange);
                    }
                    else
                    {
                        MoveInput.y = 0.0f;
                    }
                }
                else 
                {
                    //Makes us walk using the other body methods
                    PlayerBody.AddForce(CurrentDirectionVector * LerpedSpeed * StickInputY, ForceMode.VelocityChange);
                    PlayerBody.AddForce(Quaternion.Euler(0, 90, 0) * CurrentDirectionVector * LerpedSpeed * StrafeMultipler * StickInputX, ForceMode.VelocityChange);
                }
                
            }

            //Cap XZ velocity
            if (PlayerBody.velocity.magnitude > LerpedSpeed)
            {
                Vector3 CappedVelocity = new Vector3(PlayerBody.velocity.normalized.x * LerpedSpeed, PlayerBody.velocity.y, PlayerBody.velocity.normalized.z * LerpedSpeed);
                PlayerBody.velocity = CappedVelocity;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Get if we swung down
        if (LeftVelocity.y < -MinThrust && RightVelocity.y > MinThrust && !LeftArmSwingDown)
        {
            LeftArmSwingDown = true;
            RightArmSwingDown = false;
            Running = true;
            //Reset the Swing Leeway counter
            SwingLeewayCounter = 0;
        }

        if (RightVelocity.y < -MinThrust && LeftVelocity.y > MinThrust && !RightArmSwingDown)
        {
            LeftArmSwingDown = false;
            RightArmSwingDown = true;
            Running = true;
            //Reset the Swing Leeway counter
            SwingLeewayCounter = 0;
        }
        //We can't run backwards and we can't run while crouching
        if (Running && MoveInput.y > 0 && !BodyController.Crouching)
        {
            CurrentRunSpeed = MaxRunSpeed;
        }
        //Count up the Leeway counter
        SwingLeewayCounter += Time.deltaTime;

        //Stop running                                                                                                      //We can't run backwards!
        if ((LeftVelocity.magnitude < MinThrust && RightVelocity.magnitude < MinThrust && SwingLeewayCounter > SwingLeeway) || MoveInput.y < 0)
        {
            CurrentRunSpeed = MaxWalkSpeed;
            Running = false;
            LeftArmSwingDown = false;
            RightArmSwingDown = false;
        }

        if (!WalkingActivated)
        {
            CurrentRunSpeed = 0;
        }

        LerpedSpeed = Mathf.Lerp(LerpedSpeed, CurrentRunSpeed, Time.fixedDeltaTime * LerpingAcceleration);

    }

    // Get the forward vector along a constrained Y-Space
   static public Vector3 ConstrainedForward(Transform focus)
    {
        // Get the natural forward transition
        Vector3 naturalForward = focus.forward;
        // Nullify its y-component
        naturalForward.y = 0;
        // Normalize xz-plane and align it with the chosen y-axis
        Vector3 fixedForward = naturalForward.normalized;

        // This gives us a vector whose xz-planar speed is normalized
        // while preserving the fixed Y component
        return fixedForward;
    }

}
