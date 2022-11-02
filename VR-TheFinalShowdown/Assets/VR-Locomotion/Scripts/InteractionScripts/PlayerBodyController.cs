using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerBodyController : MonoBehaviour
{
    public Rigidbody PlayerBody;
    [SerializeField] private Rigidbody DefaultPhysicsProfile;
    [SerializeField] private Rigidbody WaterPhysicsProfile;

    [SerializeField] Transform WaterDetectOrigin;
    [Header("Ground Detection")]
    [SerializeField] LayerMask GroundLayer;
   
    [Header("At these slope angles, the player will stick to the slope")]
    [Range(0, 90)] [SerializeField] float MaxSlopeAllowance;
    
    [Header("Step Tolerances")]
    [Range(0.1f, 1)] [SerializeField] float StepHeight = 0.25f;
    [Range(0.1f, 0.5f)] [SerializeField] float StepOvershoot = 0.025f;
    [Range(60, 90)] [SerializeField] float MinStepDegrees = 80f;
    private int SlopeModifier = 1;

    [Header("Water Detection")]
    [SerializeField] LayerMask WaterLayer;
    [Range(0, 1)] [SerializeField] float WaterDetectSphereRadius = 0.5f;
    private bool SubmergedClimb;
    //[Range(0, 0.5f)] public float StepSmoothTime;
    [HideInInspector] public RaycastHit GroundHit;

    private GameObject CurrentFloor;

    public BodyAlignmentSystem BodySystem;
    [Header("This keeps the player rig from sliding along the floor. This is used for movement types that don't use thumbstick-walking")]
    [SerializeField] bool SlidingLock;

    [Header("This fraction of the player's height will determine if they're crouching")]
    [Range(0, 1)] [SerializeField] float CrouchingTolerance = 0.75f;
    [SerializeField] bool DebugState;
    PlayerState PreviousState;

    public UnityEvent HitGround;
    public UnityEvent WaterEntered;
    public UnityEvent WaterExited;
    public UnityEvent CrouchEntered;
    public UnityEvent CrouchLeft;

    [HideInInspector] public float RealWorldSpeed;
    [HideInInspector] public Vector3 RealWorldVelocity;
    private Vector3 PrevPos;
    public enum PlayerState
    {
        Grounded, Falling, InWater, Jumping, Climbing
    }
    public static PlayerState State;
    [HideInInspector] public bool Crouching;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerBody == null)
        {
            PlayerBody = GetComponent<Rigidbody>();
        }
        CopyRigidbodyComponent(DefaultPhysicsProfile, PlayerBody, true, false);
        State = PlayerState.Falling;
    }

    private void Update()
    {
        GroundRaycast();
        CalculateInWater();
        CalculateAirborne();
        CalculateCrouching();
        CalculateGrounded();

        CalculateRealWorldSpeed();

        if (DebugState && PreviousState != State)
        {
            Debug.Log("STATE CHANGED TO "+State);
            PreviousState = State;
        }
    }

    private void FixedUpdate()
    {
        StepUpLogic();
    }

    private void CalculateAirborne()
    {
        if (GroundHit.collider == null && State == PlayerState.Grounded && State != PlayerState.Climbing)
        {
            State = PlayerState.Falling;
        }

        if (State == PlayerState.Jumping && PlayerBody.velocity.y <= 0)
        {
            State = PlayerState.Falling;
        }  
    }

    private void StepUpLogic()
    {
        if (State == PlayerState.Grounded)
        {
            Vector3 Dir = PlayerBody.velocity.normalized;
            if (MovingWithoutThumbstick())
            {
                Dir = RealWorldVelocity.normalized;
            }

            Dir.y = 0;

            Ray rayLower = new Ray(BodySystem.FootTransform.position + Vector3.up * 0.05f, Dir);
            Ray rayUpper = new Ray(BodySystem.FootTransform.position + Vector3.up * (StepHeight + 0.01f), Dir);
            Ray rayDown = new Ray(rayUpper.origin + (rayUpper.direction * (BodySystem.BodyCollider.radius + 0.2f)), Vector3.down);
            RaycastHit hitLower, hitUpper, hitDown;

            Debug.DrawRay(rayLower.origin, rayLower.direction * (BodySystem.BodyCollider.radius + 0.05f), Color.red);
            Debug.DrawRay(rayUpper.origin, rayUpper.direction * (BodySystem.BodyCollider.radius + 0.2f), Color.green);
            Debug.DrawRay(rayDown.origin, rayDown.direction, Color.blue);

            if (Physics.Raycast(rayLower, out hitLower, BodySystem.BodyCollider.radius + 0.05f, GroundLayer))
            {
                if (!Physics.Raycast(rayUpper, out hitUpper, BodySystem.BodyCollider.radius + 0.2f, GroundLayer))
                {
                    if (Physics.Raycast(rayDown, out hitDown, 1, GroundLayer))
                    {
                        if (Dir.magnitude < 0.1f)
                        {
                            return;
                        }

                        float SlopeAngle = Vector3.Angle(hitLower.normal, Vector3.up);
                        if (SlopeAngle < MinStepDegrees)
                        {
                            return;
                        }

                        if (DebugState)
                        {
                            Debug.Log("STEP UP");
                        }
                        
                        Vector3 TempPos = PlayerBody.position;
                        TempPos.y = hitDown.point.y + 0.01f;
                        PlayerBody.position = TempPos + (Dir * (BodySystem.BodyCollider.radius+ StepOvershoot));
                    }
                }
            }
        }
       
    }

    void GroundRaycast()
    {
        //Looks 2 frames ahead
        Vector3 StartPoint = PredictiveCorrectionPosition(2);

        Vector3 ForwardHitStartPoint = PlayerBody.position;
        ForwardHitStartPoint.y = PlayerBody.position.y + 0.01f;

        Debug.DrawRay(StartPoint, (BodySystem.BodyCollider.height/2) * Vector3.down, Color.red);
        RaycastHit NewGroundHit;
        if (Physics.Raycast(StartPoint, -Vector3.up, out NewGroundHit, 5, GroundLayer))
        {
            if (CurrentFloor != NewGroundHit.collider.gameObject)
            {
                CurrentFloor = NewGroundHit.collider.gameObject;
                //Disables the collisions for just long enough!

                Physics.IgnoreCollision(BodySystem.BodyCollider, NewGroundHit.collider, true);
                StartCoroutine(LockBuffer(NewGroundHit));
            }
            
            float SlopeAngle = Vector3.Angle(NewGroundHit.normal, Vector3.up);

            if (SlopeAngle > MaxSlopeAllowance)
            {
                //This prevents the Spherecast from overstepping its bounds and allowing us to walk over our tolerance.
                if (SlopeModifier == 1)
                {
                    SlopeModifier = 0;
                }
            }
            else
            {
                if (SlopeModifier == 0)
                {
                    SlopeModifier = 1;
                }
            }
        }

        if (Physics.Raycast(BodySystem.BodyCollider.transform.position, Vector3.down, out GroundHit, ((BodySystem.BodyCollider.height / 2) + 0.1f * SlopeModifier),GroundLayer))
        {
            if (State == PlayerState.Falling && State != PlayerState.InWater && State != PlayerState.Climbing && State != PlayerState.Jumping)
            {
                State = PlayerState.Grounded;
                HitGround.Invoke();

               // PlayerBody.useGravity = false;

                Vector3 TempVel = PlayerBody.velocity;
                TempVel.y = 0;
               // PlayerBody.velocity = TempVel;

                if (DebugState)
                {
                    Debug.Log("LANDED");
                }    
                
            }
        }
    }

    private void CalculateRealWorldSpeed()
    {
        var velocity = transform.TransformDirection((BodySystem.BodyCollider.transform.localPosition - PrevPos) / Time.deltaTime);
        velocity = Vector3.Scale(velocity,new Vector3(1,0,1));

        RealWorldSpeed = velocity.magnitude;
        RealWorldVelocity = velocity;

        PrevPos = BodySystem.BodyCollider.transform.localPosition;
    }

    private bool MovingWithoutThumbstick()
    {
        Vector3 CurrentRBVel = PlayerBody.velocity;
        CurrentRBVel.y = 0;

        if (RealWorldSpeed > 0.1f && CurrentRBVel.magnitude < 0.1f)
        {
            return true;
        }

        return false;
    }

    //This allows us to predict where the RigidBody is going to be in the following frames
    private Vector3 PredictiveCorrectionPosition(int FramesAhead)
    {
        Vector3 velocity = PlayerBody.velocity;
        Vector3 position = BodySystem.BodyCollider.transform.position;

        for (int i = 0; i < FramesAhead; ++i)
        {
            velocity *= (1.0f - Time.fixedDeltaTime * PlayerBody.drag); // drag
            velocity += Physics.gravity * Time.fixedDeltaTime; // gravity
            position += velocity * Time.fixedDeltaTime; // move point
        }

        return position;
    }

    IEnumerator LockBuffer(RaycastHit Hit)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Physics.IgnoreCollision(BodySystem.BodyCollider, Hit.collider, false);
        yield return null;
    }

    private void CalculateGrounded()
    {
        if (State == PlayerState.Grounded)
        {
           float SlopeAngle = Vector3.Angle(GroundHit.normal, Vector3.up);

            if (SlopeAngle < MaxSlopeAllowance && SlopeAngle < 90 && SlopeAngle != 0)
            {
                if (PlayerBody.useGravity)
                {
                    PlayerBody.useGravity = false;
                }
                PlayerBody.velocity = Vector3.ProjectOnPlane(PlayerBody.velocity, GroundHit.normal);

                if (MovingWithoutThumbstick())
                {
                    PlayerBody.AddForce(-GroundHit.normal * RealWorldSpeed * 2, ForceMode.VelocityChange);
                }
            }
            else 
            {
                if (!PlayerBody.useGravity)
                {
                    PlayerBody.useGravity = true;
                }
            }

            if (SlidingLock)
            {
                Vector3 TempVel = PlayerBody.velocity;
                TempVel = Vector3.Scale(TempVel, new Vector3(0, 1, 0));
                PlayerBody.velocity = TempVel;
            }

        }
        else
        {
            if (!PlayerBody.useGravity && (State == PlayerState.Falling || State == PlayerState.Jumping))
            {
                PlayerBody.useGravity = true;
            }
        }
    }

  

    private void CalculateCrouching()
    {
        float CurrentHeight = Vector3.Distance(BodySystem.HeadTransform.position, BodySystem.FootTransform.position);

        if (CurrentHeight / BodySystem.PlayerHeight < CrouchingTolerance)
        {
            if (!Crouching)
            {
                Crouching = true;
                CrouchEntered.Invoke();
            }
        }
        else
        {
            if (State == PlayerState.Grounded)
            {
                Crouching = false;
                CrouchLeft.Invoke();
            }
        }

    }

    private void CalculateInWater()
    {
        Collider[] HitColliders = Physics.OverlapSphere(WaterDetectOrigin.position, WaterDetectSphereRadius, WaterLayer, QueryTriggerInteraction.Collide);

        if (HitColliders.Length > 0)
        {
            if (State != PlayerState.InWater && State != PlayerState.Climbing)
            {
                State = PlayerState.InWater;
                WaterEntered.Invoke();

                //Swaps the physics profiles
                CopyRigidbodyComponent(PlayerBody, WaterPhysicsProfile, true, true);
            }

            if (State == PlayerState.Climbing)
            {
                SubmergedClimb = true;
            }
        }
        else
        {
            if (State == PlayerState.InWater)
            {
                State = PlayerState.Falling;
                WaterExited.Invoke();

                //Swaps the physics profiles
                CopyRigidbodyComponent(PlayerBody, DefaultPhysicsProfile, true, true);
                SubmergedClimb = false;
            }
            
            if (State == PlayerState.Climbing && SubmergedClimb)
            {
                WaterExited.Invoke();

                //Swaps the physics profiles
                CopyRigidbodyComponent(PlayerBody, DefaultPhysicsProfile, true, true);
                SubmergedClimb = false;
            }
        }
    }

    static public void CopyRigidbodyComponent(Rigidbody TargetRB, Rigidbody IncomingRB, bool AlterGravity, bool AlterKinematic)
    {
        TargetRB.mass = IncomingRB.mass;
        TargetRB.drag = IncomingRB.drag;
        TargetRB.angularDrag = IncomingRB.angularDrag;
        if (AlterGravity)
        {
            TargetRB.useGravity = IncomingRB.useGravity;
        }
        if (AlterKinematic)
        {
            TargetRB.isKinematic = IncomingRB.isKinematic;
        }

        TargetRB.constraints = IncomingRB.constraints;
    }
}
