using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class XR_Climb : MonoBehaviour
    {
    [SerializeField] private XRBaseInteractor LeftInteractor;
    [SerializeField] private List<IXRSelectInteractable> LeftHandList;

    [SerializeField] private XRBaseInteractor RightInteractor;
    [SerializeField] private List<IXRSelectInteractable> RightHandList;

    [Header("We use this to perfectly move the player, regardless of playspace size!")]
    [SerializeField] Transform HeadTransform;

    [SerializeField] string TagToLookForWhenClimbing = "Climbable";
    [SerializeField] string TagToLookForWhenReachingTop = "ClimbUp";
    [Header("How long a Climb Up takes")]
    [SerializeField] float ClimbingUpTime = 1.0f;
    [Header("The color that the fade goes to")]
    [SerializeField] private Color FadeColor = Color.black;

    private Transform ActiveHand;

    public bool IsClimbing;
    public ConfigurableJoint ClimbingHandle;

    private Transform ClimbableTransform;

    private Rigidbody PlayerBody;
    [SerializeField] PlayerBodyController BodyController;
    [SerializeField] private Rigidbody JointBody;
    private Vector3 BetweenHandAndPoint;

    private void OnEnable()
        {
           LeftInteractor.selectEntered.AddListener(LeftGrabbed);
           RightInteractor.selectEntered.AddListener(RightGrabbed);

           LeftInteractor.selectExited.AddListener(LeftReleased);
           RightInteractor.selectExited.AddListener(RightReleased);

           PlayerBody = GetComponent<Rigidbody>();
        }

    private void OnDisable()
    {
        LeftInteractor.selectEntered.RemoveListener(LeftGrabbed);
        RightInteractor.selectEntered.RemoveListener(RightGrabbed);

        LeftInteractor.selectExited.RemoveListener(LeftReleased);
        RightInteractor.selectExited.RemoveListener(RightReleased);
    }

    private void LeftGrabbed(SelectEnterEventArgs arg0)
    {
        if (!HeadCollisionManager.HeadColliding)
        {
            if (HasClimbable(LeftHandList))
            {
                EndClimbing(RightInteractor);
                BeginClimbing(LeftInteractor);
            }

            if (HasClimbUpable(LeftHandList))
            {
                ClimbToTop(LeftInteractor);
            }
        }
        
    }

    private void RightGrabbed(SelectEnterEventArgs arg0)
    {
        if (!HeadCollisionManager.HeadColliding)
        {
            if (HasClimbable(RightHandList))
            {
                EndClimbing(LeftInteractor);
                BeginClimbing(RightInteractor);
            }

            if (HasClimbUpable(RightHandList))
            {
                ClimbToTop(RightInteractor);
            }
        }
    }

    private void LeftReleased(SelectExitEventArgs arg0)
    {
        if (IsClimbing)
        {
            EndClimbing(LeftInteractor);
        }
    }

    private void RightReleased(SelectExitEventArgs arg0)
    {
        if (IsClimbing)
        {
            EndClimbing(RightInteractor);
        }
    }

    private void FixedUpdate()
    {
        ClimbingMotion();
    }

    private void LateUpdate()
    {
        //Get the list of Interactables this frame
        LeftHandList = LeftInteractor.interactablesSelected;
        RightHandList = RightInteractor.interactablesSelected;

    }

    bool HasClimbable(List<IXRSelectInteractable> InteractableList)
    {
        //Iterate through the list of Interactables
        foreach (IXRSelectInteractable Interactable in InteractableList)
        {
            //Does one of the ones in our list have the Climbing tag?
            if (Interactable.transform.CompareTag(TagToLookForWhenClimbing))
            {
                ClimbableTransform = Interactable.transform;
                return true;
            }
        }
        return false;
    }

    bool HasClimbUpable(List<IXRSelectInteractable> InteractableList)
    {
        //Iterate through the list of Interactables
        foreach (IXRSelectInteractable Interactable in InteractableList)
        {
            //Does one of the ones in our list have the Climbing Up tag?
            if (Interactable.transform.CompareTag(TagToLookForWhenReachingTop))
            {
                return true;
            }
        }
        return false;
    }


    void BeginClimbing(XRBaseInteractor Interactor)
    {
        //Get the current hand
        ActiveHand = Interactor.transform;
        IsClimbing = true;

        //Orient the climbing handle to where we need it
        ClimbingHandle.transform.rotation = PlayerBody.transform.localRotation;
        ClimbingHandle.transform.position = ActiveHand.position;
        //Get the vector between the handle and the center of the grabbed object
        BetweenHandAndPoint = (ClimbableTransform.position - ClimbingHandle.transform.position);
        //Attach the player to the handle
        ClimbingHandle.connectedBody = PlayerBody;

        PlayerBodyController.State = PlayerBodyController.PlayerState.Climbing;
    }

    void ClimbingMotion()
    {
        if (IsClimbing)
        {
            //This sets the handle to the point where the hand meets the object, even if it's in motion
            JointBody.MovePosition(ClimbableTransform.position - BetweenHandAndPoint);
            //Move based on the antithesis of the hand's movement
            ClimbingHandle.targetPosition = -ActiveHand.localPosition;
        }
    }

    public void EndClimbing(XRBaseInteractor Interactor)
    {
        if (ActiveHand == Interactor.transform)
        {
            IsClimbing = false;
            ClimbingHandle.connectedBody = null;
            ActiveHand = null;

            ClimbingHandle.transform.parent = null;
        }

        PlayerBodyController.State = PlayerBodyController.PlayerState.Falling;

    }

    public void EndClimbingAll()
    {
        IsClimbing = false;
        ClimbingHandle.connectedBody = null;
        ActiveHand = null;

        ClimbingHandle.transform.parent = null;
    }

    void ClimbToTop(XRBaseInteractor Interactor)
    {
        //Recenter the handle for calibration
        ClimbingHandle.transform.position = PlayerBody.position;
        //Get the current hand
        ActiveHand = Interactor.transform;
        ClimbingHandle.transform.position = ActiveHand.position;

        StartCoroutine(ClimbUp());
    }

    IEnumerator ClimbUp()
    {
        PlayerBody.isKinematic = true;

        Vector3 HandPos = ActiveHand.position;
        //Get the difference between the center of the Play Area
        Vector3 BetweenHeadAndPoint = (HeadTransform.position - PlayerBody.position);
        //Negate the Y for simplicity
        BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

        ColorFilter_Controller.FadePanelChange(FadeColor, ClimbingUpTime, true);
        yield return new WaitForSeconds(ClimbingUpTime);
        EndClimbing(LeftInteractor);
        EndClimbing(RightInteractor);

        //Get the position of our grabbing point and the subtract the difference, sending us right on top of the point!
        PlayerBody.position = HandPos - BetweenHeadAndPoint;

        ColorFilter_Controller.FadePanelChange(MobilePostProcessController.CurrentColor, ClimbingUpTime, false);

        PlayerBody.isKinematic = false;
        if (PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded)
        {
            PlayerBody.useGravity = true;
        }
    }
}

