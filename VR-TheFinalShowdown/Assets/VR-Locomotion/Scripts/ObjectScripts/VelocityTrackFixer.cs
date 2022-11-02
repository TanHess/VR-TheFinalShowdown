using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VelocityTrackFixer : MonoBehaviour
{
    Rigidbody RB;
    [SerializeField] float AngVel = 50;
    private XRGrabInteractable grabInteractor => GetComponent<XRGrabInteractable>();
    private XRBaseInteractable.MovementType TempType;
    private Collision ThisCollision;
    [SerializeField] LayerMask HitLayer;

    // Start is called before the first frame update
    void Start()
    {
        TempType = grabInteractor.movementType;
    }

    private void OnEnable()
    {
        RB = GetComponent<Rigidbody>();
        RB.maxAngularVelocity = AngVel;

        grabInteractor.selectEntered.AddListener(Grabbed);
        grabInteractor.selectExited.AddListener(GrabEnd);

    }
    private void OnDisable()
    {
        grabInteractor.selectEntered.RemoveListener(Grabbed);
        grabInteractor.selectExited.RemoveListener(GrabEnd);
    }

    private void Grabbed(SelectEnterEventArgs arg0)
    {
        
    }

    private void GrabEnd(SelectExitEventArgs arg0)
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & HitLayer) != 0)
        {
            ThisCollision = collision;
            grabInteractor.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision == ThisCollision)
        {
            grabInteractor.movementType = TempType;
        }
        
    }
}
