using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[CanSelectMultiple(true)]
[RequireComponent(typeof(Rigidbody))]
public class XRGrabInteractable_MultipleGrab : XRGrabInteractable
{
    private bool CanGrab => interactorsSelecting.Count == 1;
    private bool CanDrop => interactorsSelecting.Count == 0;

    private enum GrabState
    {
        Single_Grabbed,
        Multi_Grabbed,
        Released,
    }

    private GrabState CurrentGrabState;

    IXRSelectInteractor FirstInteractor;
    IXRSelectInteractor SecondInteractor;

    private bool TrackedRotation;
    private bool TrackedPosition;

    private GameObject GrabPivotPoint;
    private Transform OriginalParent;

    private void Start()
    {
        CurrentGrabState = GrabState.Released;
        TrackedRotation = trackRotation;
        TrackedPosition = trackPosition;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (interactorsSelecting.Count == 1)
        {
            if (interactorsSelecting[0] is XRDirectInteractor Interactor)
            {
                //Handles the first time it's grabbed
                if (CurrentGrabState == GrabState.Released)
                {
                    FirstInteractor = Interactor;

                    CurrentGrabState = GrabState.Single_Grabbed;

                    attachTransform.position = FirstInteractor.transform.position;
                    attachTransform.rotation = FirstInteractor.transform.rotation;
                }
                //Handles switching from multi to single grab
                if (CurrentGrabState == GrabState.Multi_Grabbed)
                {
                    CurrentGrabState = GrabState.Single_Grabbed;
                    transform.parent = OriginalParent;
                    Destroy(GrabPivotPoint);

                    trackRotation = TrackedRotation;
                    trackPosition = TrackedPosition;

                    //Returns us to the original hand
                    if (Interactor.transform == FirstInteractor.transform)
                    {
                        attachTransform.position = FirstInteractor.transform.position;
                        attachTransform.rotation = FirstInteractor.transform.rotation;
                    }
                    
                    //Switches to the other hand
                    if (Interactor.transform == SecondInteractor.transform)
                    {
                        attachTransform.position = SecondInteractor.transform.position;
                        attachTransform.rotation = SecondInteractor.transform.rotation;
                    }    
                }
            }
        }
        if (interactorsSelecting.Count == 2)
        {
            if (CurrentGrabState == GrabState.Single_Grabbed)
            {
                CurrentGrabState = GrabState.Multi_Grabbed;

                FirstInteractor = interactorsSelecting[0];
                SecondInteractor = interactorsSelecting[1];

                OriginalParent = transform.parent;

                GrabPivotPoint = new GameObject("GrabPoint");
                GrabPivotPoint.transform.position = Vector3.Lerp(FirstInteractor.transform.position, SecondInteractor.transform.position, 0.5f);
                GrabPivotPoint.transform.rotation = transform.rotation;

                trackRotation = false;
                trackPosition = false;
            }

            Vector3 currentHandPosition1 = FirstInteractor.transform.position; // current first hand position
            Vector3 currentHandPosition2 = SecondInteractor.transform.position; // current second hand position

            Vector3 HandDirection = (currentHandPosition1 - currentHandPosition2).normalized; // direction vector of current first and second hand position 
            Vector3 CenterPoint = Vector3.Lerp(currentHandPosition1, currentHandPosition2, 0.5f);

            GrabPivotPoint.transform.rotation = Quaternion.LookRotation(HandDirection, Vector3.up);
            GrabPivotPoint.transform.position = CenterPoint;

            if (transform.parent != GrabPivotPoint.transform)
            {
                transform.parent = GrabPivotPoint.transform; //Set the parent if not done already. This allows for easy rotation relative to its starting point.
            }
        }
    }

    protected override void Grab()
    {
        if (CanGrab)
            base.Grab();
    }

    protected override void Drop()
    {
        if (CanDrop)
        {
            base.Drop();
            CurrentGrabState = GrabState.Released;
        }
            
    }

}
