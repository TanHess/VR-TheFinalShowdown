using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyAlignmentSystem : MonoBehaviour
{
    [Header("These dictate where forward is")]
    public Transform LeftController;
    public Transform RightController;

    [HideInInspector] public Vector3 AverageHandForward;

    [SerializeField] float RotationSpeed;

    public CapsuleCollider BodyCollider;
    [SerializeField] PlayerBodyController BodyController;

    [SerializeField] Transform HeadOrigin;
    [Header("A Raycast will be sent downward to hit the floor, which has this layer")]
    [SerializeField] LayerMask ObjectsToHit;

    [Header("This is where we get what up is")]
    [SerializeField] Transform AlignmentOrienter;

    [Header("Here, we get the different body parts to align certain transforms to")]
    public Transform HeadTransform;
    [Tooltip("The player's height is initially taken as the center of the headset, so add a couple inches")]
    [SerializeField] float TopOfHeadAdjustment = 0.25f;
    public Transform WaistTransform;
    [Tooltip("This helps us align the waist, depending on the height of the player")]
    [SerializeField] float DistancePercentageToWaist = 0.5f;

    enum AlignFootTo // your custom enumeration
    {
        Head,
        CenterOfHands,
        CenterOfHeadANDHands
    };

    [SerializeField] private AlignFootTo FootAlignmentMethod;

    public Transform FootTransform;

    [HideInInspector] public float PlayerHeight;

    [HideInInspector] public bool HeadAndHandsAligned;
    [Tooltip("The dot product of the head and the hands must be higher than this to be true")]
    [SerializeField] float AlignmentTolerance = 0.7f;

    private Vector3 TargetDirection;

    // Update is called once per frame
    void LateUpdate()
    {
        AverageHandForward = LeftController.transform.forward.normalized + RightController.transform.forward.normalized;
        AverageHandForward.y = 0;
        AverageHandForward = AverageHandForward.normalized;

        Vector3 NormalizedHead = ConstrainedForward(HeadTransform).normalized;

        HeadAndHandsAligned = IsFacingSameDirection(AverageHandForward, NormalizedHead, AlignmentTolerance);

        float singlestep = RotationSpeed * Time.deltaTime;

        if (HeadAndHandsAligned)
        {
            TargetDirection = AverageHandForward;
        }

        Quaternion TargetRotation = Quaternion.identity;

        if (TargetDirection != Vector3.zero)
        {
            TargetRotation = Quaternion.LookRotation(TargetDirection);
        }

        //Aligns the foot to the desired point
        AlignFoot();
        ScaleCollider(BodyCollider);
        float DistanceToPlayAreaBase = Vector3.Distance(HeadTransform.position, FootTransform.position);

        float WaistHeight = (DistanceToPlayAreaBase * DistancePercentageToWaist);

        WaistTransform.localPosition = new Vector3(HeadTransform.localPosition.x, WaistHeight, HeadTransform.localPosition.z);
        WaistTransform.rotation = Quaternion.Lerp(WaistTransform.rotation, TargetRotation, singlestep);

    }

    void AlignFoot()
    {
        if (FootAlignmentMethod == AlignFootTo.Head)
        {
            FootTransform.position = new Vector3(HeadTransform.position.x, AlignmentOrienter.position.y, HeadTransform.position.z);
        }

        if (FootAlignmentMethod == AlignFootTo.CenterOfHands)
        {
            Vector3 HandsMidpoint = GetMidpoint(LeftController.position, RightController.position);
            FootTransform.position = new Vector3(HandsMidpoint.x, AlignmentOrienter.position.y, HandsMidpoint.z);
        }
        
        if (FootAlignmentMethod == AlignFootTo.CenterOfHeadANDHands)
        {
            Vector3 HandsMidpoint = GetMidpoint(LeftController.position, RightController.position);
            Vector3 BodyHandsMidpoint = GetMidpoint(HandsMidpoint, HeadTransform.position);

            FootTransform.position = new Vector3(BodyHandsMidpoint.x, AlignmentOrienter.position.y, BodyHandsMidpoint.z);
        }
    }

    void ScaleCollider(CapsuleCollider Col)
    {
        Vector3 TempHeight = HeadTransform.position;
        TempHeight.y = HeadTransform.position.y - TopOfHeadAdjustment;

        Vector3 TempBasePos = FootTransform.position;
        
        TempBasePos.y = FootTransform.position.y;

        Vector3 Midpoint = GetMidpoint(TempBasePos, TempHeight);
        BodyCollider.transform.position = Midpoint;

        Col.height = Vector3.Distance(TempHeight, TempBasePos);

        //Set player height while we're at it!
        if (PlayerHeight < Vector3.Distance(HeadTransform.position, FootTransform.position))
        {
            PlayerHeight = Vector3.Distance(HeadTransform.position, FootTransform.position);
        }
    }

    // Get the forward vector along a constrained Y-Space
    Vector3 ConstrainedForward(Transform focus)
    {
        // Get the natural forward transition
        Vector3 naturalForward = focus.forward;
        // Nullify its y-component
        naturalForward.y = 0;
        // Normalize xz-plane and align it with the chosen y-axis
        Vector3 fixedForward = naturalForward.normalized;

        // This gives us a vector whose xz-plane us normalized
        // while preserving the fixed Y component
        return fixedForward;
    }

    static public bool IsFacingSameDirection(Vector3 FirstVector, Vector3 SecondVector, float Tolerance)
    {
        bool Result = false;
        float FinalDot = Vector3.Dot(SecondVector, FirstVector);

        if (FinalDot > Tolerance)
        {
            Result = true;
        }

        return Result;
    }
    
    private Vector3 GetMidpoint(Vector3 Start, Vector3 End)
    {
        Vector3 Result;
        Result = Vector3.Lerp(Start, End, 0.5f);
        return Result;
    }
}
