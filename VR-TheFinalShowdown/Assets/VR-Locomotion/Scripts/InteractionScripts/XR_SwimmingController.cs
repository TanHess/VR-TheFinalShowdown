using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class XR_SwimmingController : MonoBehaviour
{
    [SerializeField] PlayerBodyController BodyController;
    [SerializeField] Rigidbody PlayerBody;

    [SerializeField] Transform PlayerHead;

    [SerializeField] float StrokeForce;
    [SerializeField] float BasicMoveForce;

    [SerializeField] XRDirectInteractor LeftXRInteractor;
    [SerializeField] XRDirectInteractor RightXRInteractor;

    private Vector3 LeftVelocity;
    private Vector3 RightVelocity;

    private Vector3 LeftStrokePreviousPos;
    private Vector3 RightStrokePreviousPos;

    [SerializeField] InputAction BasicSwimActivationAction;
    [SerializeField] InputAction BasicSwimDeactivationAction;

    [SerializeField] InputAction MoveAxisAction;
    Vector2 MoveInput;
    bool BasicSwimActive;

    public UnityEvent StrokeEvent;
    [Header("Stroke above this force to invoke the Stroke Event")]
    [SerializeField] float SwimStrokeEventThreshold = 0.25f;
    private bool StrokeBool;

    private void Awake()
    {
        BasicSwimActivationAction.Enable();
        BasicSwimActivationAction.performed += BasicSwimStart;

        BasicSwimDeactivationAction.Enable();
        BasicSwimDeactivationAction.performed += BasicSwimEnd;

        MoveAxisAction.Enable();
        MoveAxisAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
    }

  
    private void BasicSwimStart(InputAction.CallbackContext obj)
    {
        BasicSwimActive = true;
    }

    private void BasicSwimEnd(InputAction.CallbackContext obj)
    {
        BasicSwimActive = false;
        MoveInput = Vector2.zero;
    }

    void Swimming()
    {
        float StickInputY = MoveInput.y;
        float StickInputX = MoveInput.x;

        //Can't swim if you're in a wall
        if (HeadCollisionManager.HeadColliding)
        {
            StickInputY = 0;
            StickInputX = 0;
        }

        //Get Hand Velocity
        LeftVelocity = (LeftXRInteractor.transform.position - LeftStrokePreviousPos) / Time.deltaTime;
        LeftStrokePreviousPos = LeftXRInteractor.transform.position;

        RightVelocity = (RightXRInteractor.transform.position - RightStrokePreviousPos) / Time.deltaTime;
        RightStrokePreviousPos = RightXRInteractor.transform.position;

        if (PlayerBodyController.State == PlayerBodyController.PlayerState.InWater && BasicSwimActive)
        {
            float LeftVelocityMagnitude = 0;
            float RightVelocityMagnitude = 0;

            if (BodyAlignmentSystem.IsFacingSameDirection(-LeftVelocity, PlayerHead.forward, 0.5f))
            {
                LeftVelocityMagnitude = Mathf.Abs(LeftVelocity.magnitude);
            }

            if (BodyAlignmentSystem.IsFacingSameDirection(-RightVelocity, PlayerHead.forward, 0.5f))
            {
                RightVelocityMagnitude = Mathf.Abs(RightVelocity.magnitude);
            }

            float ArmSwimFactor = LeftVelocityMagnitude + RightVelocityMagnitude;

            if (HeadCollisionManager.HeadColliding)
            {
                ArmSwimFactor = 0;
            }

            //Makes us swim using the head forward
            PlayerBody.AddForce(PlayerHead.forward * BasicMoveForce * StickInputY, ForceMode.Acceleration);
            PlayerBody.AddForce(Quaternion.Euler(0, 90, 0) * PlayerHead.forward * BasicMoveForce * StickInputX, ForceMode.Acceleration);

            //Swim faster by using your arms
            PlayerBody.AddForce(PlayerHead.forward * ArmSwimFactor * StrokeForce, ForceMode.Acceleration);

            //Perform the Stroke Event
            if (ArmSwimFactor >= SwimStrokeEventThreshold && !StrokeBool && !HeadCollisionManager.HeadColliding)
            {
                StrokeBool = true;
                StrokeEvent.Invoke();
            }
            if (ArmSwimFactor < SwimStrokeEventThreshold && StrokeBool)
            {
                StrokeBool = false;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Swimming();
    }
}
