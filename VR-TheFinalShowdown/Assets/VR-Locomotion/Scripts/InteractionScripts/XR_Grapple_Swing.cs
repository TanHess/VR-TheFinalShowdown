using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class XR_Grapple_Swing : MonoBehaviour
{
    private LineRenderer GrappleBeam;
    private GetGrappleTargets GrappleTargetGetter;
    Vector3 GrapplePoint;
    private Transform CurrentGrapplingHand;
    private bool LeftGrappling;
    private bool RightGrappling;
    private SpringJoint AttachJoint;

    [SerializeField] private Rigidbody PlayerRigidBody;
    [SerializeField] PlayerBodyController BodyController;
    [SerializeField] Transform PlayerHead;

    [Header("Adjusts the properties of the Spring Joint that is created")]
    [Tooltip("Impacts the joint's Max Distance")]
    [SerializeField] private float GrappleMaxDistanceMultiplier = 0.8f;
    [Tooltip("Impacts the joint's Min Distance")]
    [SerializeField] private float GrappleMinDistanceMultiplier = 0.25f;

    [SerializeField] private float Springiness = 4.5f;
    [SerializeField] private float Damper = 7f;
    [SerializeField] private float MassScale = 4.5f;

    [Header("The player must be pointing at the target with this amount of accuracy")]
    [Range(0,1)][SerializeField] private float PointTolerance = 0.5f;
    [Header("Performing the Manual Swing will send the player forward, or back with this amount of force")]
    [SerializeField] private float ManualSwingForce = 2.0f;

    [SerializeField] bool MustBeAirborne;
    [SerializeField] bool BreaksAtPeak;
    [SerializeField] bool BreaksWhenIntersected;
     [SerializeField] LayerMask ObjectsToHit;

    [Header("Can freely grapple any surface with the correct tag.")]
    [SerializeField] bool CanGrappleSurfaces = true;

    [TagSelector] public string SurfacesToGrapple;
    [SerializeField] GameObject SurfaceMarker;
    [Header("Can grapple set points, dictated by the GetGrappleTargets component")]
    [SerializeField] bool CanGrapplePoints = true;

    private GameObject CurrentGrappleTarget;
    private GameObject LeftValidMarker;
    private GameObject RightValidMarker;

    [SerializeField] XRDirectInteractor LeftInteractor;
    [SerializeField] XRDirectInteractor RightInteractor;

    [Header("Input Actions")]

    [SerializeField] InputAction LeftActivationAction;
    [SerializeField] InputAction RightActivationAction;

    [SerializeField] InputAction LeftDeactivationAction;
    [SerializeField] InputAction RightDeactivationAction;

    [SerializeField] InputAction ManualSwingAction;

    [SerializeField] InputAction ManualSwingActivationAction;
    [SerializeField] InputAction ManualSwingDeactivationAction;

    private Vector2 ControlStickVector;
    private bool CanManualSwing;

    [SerializeField] Transform LeftGrappleTransform;
    [SerializeField] Transform RightGrappleTransform;

    [SerializeField] Transform LeftGrappleTip;
    [SerializeField] Transform RightGrappleTip;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;

    public UnityEvent GrappleAttachEvent;
    public UnityEvent GrappleDetatchEvent;

    // Start is called before the first frame update
    void Awake()
    {
        GrappleBeam = GetComponent<LineRenderer>();
        GrappleTargetGetter = GetComponent<GetGrappleTargets>();

        LeftActivationAction.Enable();
        LeftActivationAction.performed += LeftActivationAction_performed;

        RightActivationAction.Enable();
        RightActivationAction.performed += RightActivationAction_performed;

        LeftDeactivationAction.Enable();
        LeftDeactivationAction.performed += LeftDectivationAction_performed;

        RightDeactivationAction.Enable();
        RightDeactivationAction.performed += RightDectivationAction_performed;

        ManualSwingAction.Enable();
        ManualSwingAction.performed += ctx => ControlStickVector = ctx.ReadValue<Vector2>();
        
        ManualSwingActivationAction.Enable();
        ManualSwingActivationAction.performed += ControlStickSwingAction_performed;
        
        ManualSwingDeactivationAction.Enable();
        ManualSwingDeactivationAction.performed += ControlStickSwingDeactivateAction_performed;
    }

    private void LeftActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (!LeftGrappling && !IsTouchingObject(LeftInteractor))
        {
            if (MustBeAirborne && (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded || PlayerBodyController.State == PlayerBodyController.PlayerState.InWater))
            {
                return;
            }

            CurrentGrapplingHand = LeftGrappleTransform;
            if (CanGrapplePoints)
            {
                GameObject Temp = GetTarget(GrappleTargetGetter.AvailableTargets);

                if (Temp != null)
                {
                    if (!CurrentGrappleTarget)
                    {
                        CurrentGrappleTarget = new GameObject("GrappleTarget");
                    }

                    CurrentGrappleTarget.transform.position = GetTarget(GrappleTargetGetter.AvailableTargets).transform.position;
                    CurrentGrappleTarget.transform.parent = GetTarget(GrappleTargetGetter.AvailableTargets).transform;
                    EndGrapple(false);
                    LeftGrappling = true;
                    StartGrapple();
                }
            }

            if (CanGrappleSurfaces && !LeftGrappling) // Prioritize Grapple Points first
            {
                RaycastHit hit;
                if (Physics.Raycast(CurrentGrapplingHand.position, CurrentGrapplingHand.forward, out hit, GrappleTargetGetter.TargetMaxDistance, ObjectsToHit) && hit.collider.CompareTag(SurfacesToGrapple))
                {
                    if (!CurrentGrappleTarget)
                    {
                        CurrentGrappleTarget = new GameObject("GrappleTarget");
                    }

                    CurrentGrappleTarget.transform.position = hit.point;
                    EndGrapple(false);
                    LeftGrappling = true;
                    StartGrapple();
                }

            }
        }
    }

    private void RightActivationAction_performed(InputAction.CallbackContext obj)
    {
        if (!RightGrappling && !IsTouchingObject(RightInteractor))
        {
            if (MustBeAirborne && (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded || PlayerBodyController.State == PlayerBodyController.PlayerState.InWater))
            {
                return;
            }

            CurrentGrapplingHand = RightGrappleTransform;
            if (CanGrapplePoints)
            {
                GameObject Temp = GetTarget(GrappleTargetGetter.AvailableTargets);

                if (Temp != null)
                {
                    if (!CurrentGrappleTarget)
                    {
                        CurrentGrappleTarget = new GameObject("GrappleTarget");
                    }
                    CurrentGrappleTarget.transform.position = GetTarget(GrappleTargetGetter.AvailableTargets).transform.position;
                    CurrentGrappleTarget.transform.parent = GetTarget(GrappleTargetGetter.AvailableTargets).transform;
                    EndGrapple(true);
                    RightGrappling = true;
                    StartGrapple();
                }
            }

            if (CanGrappleSurfaces && !RightGrappling) // Prioritize Grapple Points first
            {
                RaycastHit hit;
                if (Physics.Raycast(CurrentGrapplingHand.position, CurrentGrapplingHand.forward, out hit, GrappleTargetGetter.TargetMaxDistance, ObjectsToHit) && hit.collider.CompareTag(SurfacesToGrapple))
                {
                    if (!CurrentGrappleTarget)
                    {
                        CurrentGrappleTarget = new GameObject("GrappleTarget");
                    }

                    CurrentGrappleTarget.transform.position = hit.point;
                    EndGrapple(true);
                    RightGrappling = true;
                    StartGrapple();
                }
                
            }
        }
    }

    private void LeftDectivationAction_performed(InputAction.CallbackContext obj)
    {
        if (LeftGrappling)
        {
            EndGrapple(true);
        }
    }

    private void RightDectivationAction_performed(InputAction.CallbackContext obj)
    {
        if (RightGrappling)
        {
            EndGrapple(false);
        }
    }

    private void ControlStickSwingAction_performed(InputAction.CallbackContext obj)
    {
        CanManualSwing = true;
    }

    private void ControlStickSwingDeactivateAction_performed(InputAction.CallbackContext obj)
    {
        CanManualSwing = false;
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

    private void HighlightTargets(List<GrappleTargetHolder> GrappleList)
    {
        GrappleTargetHolder[] Targets = GrappleList.ToArray();

        Vector3 LeftPosition = LeftGrappleTransform.position;
        Vector3 RightPosition = RightGrappleTransform.position;
        Vector3 LeftForward = LeftGrappleTransform.TransformDirection(Vector3.forward);
        Vector3 RightForward = RightGrappleTransform.TransformDirection(Vector3.forward);

        float curDot = PointTolerance;
        float DotToBeat = 0;

        for (int i = 0; i < Targets.Length; i++)
        {
            if (Targets[i].enabled && Targets[i].gameObject.activeInHierarchy)
            {
                //Get the vector from the potential target to the player's hand
                Vector3 L_diff = Targets[i].transform.position - LeftPosition;
                //Get its Dot Product compared to the hand
                float L_actDot = Vector3.Dot(LeftForward.normalized, L_diff.normalized);

                //Get the vector from the potential target to the player's hand
                Vector3 R_diff = Targets[i].transform.position - RightPosition;
                //Get its Dot Product compared to the hand
                float R_actDot = Vector3.Dot(RightForward.normalized, R_diff.normalized);

                //What if there are multiple targets that are within range? Compare to the one that we're pointing at most closely
                if (L_actDot > DotToBeat)
                {
                    DotToBeat = L_actDot;
                }

                if (R_actDot > DotToBeat)
                {
                    DotToBeat = R_actDot;
                }

                if (curDot < DotToBeat)
                {
                    Targets[i].HighlightCursor(true);
                }
                else
                {
                    Targets[i].HighlightCursor(false);
                }
            }
        }
    }

    public GameObject GetTarget(List<GrappleTargetHolder> GrappleList)
    {
        GrappleTargetHolder[] Targets = GrappleList.ToArray();
        GameObject TempObject = null;

        Vector3 position = CurrentGrapplingHand.position;
        Vector3 Forward = CurrentGrapplingHand.TransformDirection(Vector3.forward);

        float curDot = PointTolerance;
        float DotToBeat = 0;

        for (int i = 0; i < Targets.Length; i++)
        {
            if (Targets[i] == null)
            {
                GrappleList.Remove(Targets[i]);
                break;
            }
            if (Targets[i].enabled && Targets[i].gameObject.activeInHierarchy)
            {
                //Get the vector from the potential target to the player's hand
                Vector3 diff = Targets[i].transform.position - position;
                //Get its Dot Product compared to the hand
                float actDot = Vector3.Dot(Forward.normalized, diff.normalized);
                
                //What if there are multiple targets that are within range? Compare to the one that we're pointing at most closely 
                //for when we loop around in the for loop
                if (actDot > DotToBeat)
                {
                    DotToBeat = actDot;
                }

                if (curDot < DotToBeat)
                {
                    TempObject = Targets[i].gameObject;
                    //Set the current tolerance equal to the closest one so far in the loop.
                    curDot = DotToBeat;

                }
            }

        }
        return TempObject;
    }

    void StartGrapple()
    {
        GrapplePoint = CurrentGrappleTarget.transform.position;

        AttachJoint = PlayerRigidBody.gameObject.AddComponent<SpringJoint>();
        //Connect to the player's head as the attach point
        AttachJoint.anchor = PlayerHead.localPosition;
        AttachJoint.autoConfigureConnectedAnchor = false;
        //Connect to the grapple point
        AttachJoint.connectedAnchor = GrapplePoint;

        float DistanceFromPoint = Vector3.Distance(CurrentGrapplingHand.position, GrapplePoint);
        AttachJoint.maxDistance = DistanceFromPoint * GrappleMaxDistanceMultiplier;
        AttachJoint.minDistance = DistanceFromPoint * GrappleMinDistanceMultiplier;
        
        AttachJoint.spring = Springiness;
        AttachJoint.damper = Damper;
        AttachJoint.massScale = MassScale;

        GrappleAttachEvent.Invoke();
    }

    private void FixedUpdate()
    {
        if ((LeftGrappling || RightGrappling) && CanManualSwing)
        {
            float StickInputY = ControlStickVector.y;
            float StickInputX = ControlStickVector.x;
            float SwingForceY = ManualSwingForce * StickInputY;
            float SwingForceX = ManualSwingForce * StickInputX;
            Vector3 PlayerForward = XR_Movement_Controller.ConstrainedForward(PlayerHead.transform);

            PlayerRigidBody.AddForce(Quaternion.Euler(0, 90, 0) * PlayerForward * SwingForceX, ForceMode.Acceleration);
            PlayerRigidBody.AddForce(PlayerForward * SwingForceY, ForceMode.Acceleration);
        }   
    }

    void EndGrapple(bool Left)
    {
        if (Left)
        {
            LeftGrappling = false;
            Destroy(AttachJoint);
            GrappleBeam.positionCount = 0;
        }
        if(!Left)
        {
            RightGrappling = false;
            Destroy(AttachJoint);
            GrappleBeam.positionCount = 0;
        }
        ControlStickVector = Vector2.zero;

        GrappleDetatchEvent.Invoke();
    }

    void DrawRope()
    {
        if (!AttachJoint)
        {
            return;
        }

        Vector3 CurrentTip;
        if (LeftGrappling)
        {
            CurrentTip = LeftGrappleTip.position;
        }
        else 
        {
            CurrentTip = RightGrappleTip.position;
        }

        GrappleBeam.positionCount = 2;

        GrappleBeam.SetPosition(0, CurrentTip);
        GrappleBeam.SetPosition(1, GrapplePoint);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        DrawRope();
        HighlightTargets(GrappleTargetGetter.AvailableTargets);
        ValidSurfaceMarkerDisplay();

        if (LeftGrappling || RightGrappling)
        {
            GrapplePoint = CurrentGrappleTarget.transform.position;

            AttachJoint.connectedAnchor = GrapplePoint;
            AttachJoint.anchor = PlayerHead.localPosition;

            TestForIntersection(CurrentGrappleTarget.transform, CurrentGrapplingHand);

            if ((PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded || PlayerBodyController.State == PlayerBodyController.PlayerState.InWater) && MustBeAirborne)
            {
                EndGrapple(true);
                EndGrapple(false);

                if (CurrentGrappleTarget)
                {
                    Destroy(CurrentGrappleTarget); // Destroy the object when we're done with it
                }
            }

            if (CurrentGrapplingHand.position.y > GrapplePoint.y && BreaksAtPeak)
            {
                EndGrapple(true);
                EndGrapple(false);

                if (CurrentGrappleTarget)
                {
                    Destroy(CurrentGrappleTarget); // Destroy the object when we're done with it
                }
            }
        }

        if (LeftGrappling)
        {
            LeftInteractor.SendHapticImpulse(HapticForce, Time.deltaTime);
        }
        if (RightGrappling)
        {
            RightInteractor.SendHapticImpulse(HapticForce, Time.deltaTime);
        }

    }

    void TestForIntersection(Transform StartPoint, Transform EndPoint)
    {
        RaycastHit hit;

        if (Physics.Linecast(StartPoint.position,EndPoint.position, out hit, ObjectsToHit, QueryTriggerInteraction.Ignore) && BreaksWhenIntersected)
        {
            EndGrapple(true);
            EndGrapple(false);

            if (CurrentGrappleTarget)
            {
                Destroy(CurrentGrappleTarget); // Destroy the object when we're done with it
            }
        }
    }

    void ValidSurfaceMarkerDisplay()
    {
        RaycastHit LeftHit;
        if (Physics.Raycast(LeftGrappleTransform.position, LeftGrappleTransform.forward, out LeftHit, GrappleTargetGetter.TargetMaxDistance, ObjectsToHit) && LeftHit.collider.CompareTag(SurfacesToGrapple))
        {
            if (!LeftValidMarker)
            {
                LeftValidMarker = Instantiate(SurfaceMarker, LeftHit.point, Quaternion.identity);
            }
            LeftValidMarker.transform.position = LeftHit.point;
        }
        else
        {
            if (LeftValidMarker)
            {
                Destroy(LeftValidMarker);
            }
        }

        RaycastHit RightHit;
        if (Physics.Raycast(RightGrappleTransform.position, RightGrappleTransform.forward, out RightHit, GrappleTargetGetter.TargetMaxDistance, ObjectsToHit) && RightHit.collider.CompareTag(SurfacesToGrapple))
        {
            if (!RightValidMarker)
            {
                RightValidMarker = Instantiate(SurfaceMarker, RightHit.point, Quaternion.identity);
            }
            RightValidMarker.transform.position = RightHit.point;
        }
        else
        {
            if (RightValidMarker)
            {
                Destroy(RightValidMarker);
            }
        }

        if ((PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded || PlayerBodyController.State == PlayerBodyController.PlayerState.InWater) && MustBeAirborne)
        {
            Destroy(RightValidMarker);
            Destroy(LeftValidMarker);
        }

        if (RightGrappling)
        {
            Destroy(RightValidMarker);
        }

        if (LeftGrappling)
        {
            Destroy(LeftValidMarker);
        }
    }
}
