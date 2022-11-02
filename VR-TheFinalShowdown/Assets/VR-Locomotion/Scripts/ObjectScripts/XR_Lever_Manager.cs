using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
public class XR_Lever_Manager : MonoBehaviour
{
    public HingeJoint hinge;
    [SerializeField] private bool UpdateAnchorPerFrame;
    [SerializeField] private Transform HingeRoot;

    //angle threshold to trigger if we reached limit
    [Tooltip("The angle has to be within this amount compared to the Limit")]
    public float angleBetweenThreshold = 5f;
    //State of the hinge joint : either reached min or max or none if in between
    [HideInInspector] public HingeJointState hingeJointState = HingeJointState.None;

    //Event called on min reached
    public UnityEvent OnMinLimitReached;
    //Event called on max reached
    public UnityEvent OnMaxLimitReached;

    //Event called on min left
    public UnityEvent OnMinLimitUndone;
    //Event called on max left
    public UnityEvent OnMaxLimitUndone;

    [Header("The player can only grab it this far before it'll manually release.")]
    [SerializeField] float GrabbingDistance = 0.5f;
    [SerializeField] Transform GrabbingRoot;


    public enum HingeJointState { Min, Max, None }

    [SerializeField] XRBaseInteractable grabInteractor;
    XRBaseInteractor Interactor => (XRBaseInteractor)grabInteractor.GetOldestInteractorSelecting();

    void ReleaseAtDistance(float MaxDistance)
    {
        if (grabInteractor.isSelected)
        {
            if (Vector3.Distance(GrabbingRoot.position, Interactor.transform.position) > GrabbingDistance)
            {
                Interactor.interactionManager.SelectExit((IXRSelectInteractor)Interactor,(IXRSelectInteractable)grabInteractor);
            }
        }
    }

    private void FixedUpdate()
    {
        ReleaseAtDistance(GrabbingDistance);

        float angleWithMinLimit = Mathf.Abs(hinge.angle - hinge.limits.min);
        float angleWithMaxLimit = Mathf.Abs(hinge.angle - hinge.limits.max);

        //Reached Min
        if (angleWithMinLimit < angleBetweenThreshold)
        {
            if (hingeJointState != HingeJointState.Min)
                OnMinLimitReached.Invoke();

            hingeJointState = HingeJointState.Min;
        }
        //Reached Max
        else if (angleWithMaxLimit < angleBetweenThreshold)
        {
            if (hingeJointState != HingeJointState.Max)
                OnMaxLimitReached.Invoke();

            hingeJointState = HingeJointState.Max;
        }

        //No Limit reached
        else
        {
            if (hingeJointState == HingeJointState.Max)
            {
                OnMaxLimitUndone.Invoke();
            }

            if (hingeJointState == HingeJointState.Min)
            {
                OnMinLimitUndone.Invoke();
            }

            hingeJointState = HingeJointState.None;
        }
        
        if (UpdateAnchorPerFrame)
        {
            hinge.connectedAnchor = HingeRoot.position;
        }
          
    }
}
