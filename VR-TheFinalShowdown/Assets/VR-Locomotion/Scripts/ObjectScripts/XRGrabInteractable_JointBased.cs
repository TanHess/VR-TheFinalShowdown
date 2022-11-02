using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class XRGrabInteractable_JointBased : MonoBehaviour
{
    private XRBaseInteractable grabInteractor => GetComponent<XRBaseInteractable>();
    private Transform OriginalTransform;

    private Joint HandJoint;
    private Rigidbody ThisBody;

    private Vector3 PrevPos;
    private Vector3 CurrentVel;

    private Quaternion PrevRot;
    private Vector3 CurrentAngVel;

    [SerializeField] Transform AttachPoint;

    private void OnEnable()
    {
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
        ThisBody = GetComponent<Rigidbody>();
        HandJoint = arg0.interactorObject.transform.GetComponent<PhysicsHands_Manager>().MainHandJoint;
        HandJoint.connectedBody = ThisBody;

        if (AttachPoint)
        {
            HandJoint.anchor = AttachPoint.position;
        }

        OriginalTransform = transform.parent;
        transform.SetParent(HandJoint.transform);

    }


    private void GrabEnd(SelectExitEventArgs arg0)
    {
        StartCoroutine(ReleaseRoutine());


    }

    IEnumerator ReleaseRoutine()
    {
        transform.parent = OriginalTransform;
        HandJoint.connectedBody = null;

        yield return new WaitForFixedUpdate();

        Debug.Log(CurrentVel);

        ThisBody.AddForce(CurrentVel,ForceMode.VelocityChange);
        ThisBody.AddTorque(CurrentAngVel, ForceMode.VelocityChange);
        yield return null;
    }

    private void LateUpdate()
    {
        CurrentVel = GetVelocity(transform);
        CurrentAngVel = GetAngularVelocity(transform);

      //  Debug.Log("GOING");
    }

    Vector3 GetVelocity(Transform Target)
    {
        Vector3 FinalVel;

        FinalVel = (Target.position - PrevPos) / Time.fixedDeltaTime;
        PrevPos = Target.position;

        return FinalVel;
    }

    Vector3 GetAngularVelocity(Transform Target)
    {
        Vector3 FinalAngVel;

        Quaternion deltaRotation = Target.rotation * Quaternion.Inverse(PrevRot);

        PrevRot = Target.rotation;

        deltaRotation.ToAngleAxis(out var angle, out var axis);

        // Do the weird thing to account for have a range of -180 to 180
        if (angle > 180)
            angle -= 360;

        angle *= Mathf.Deg2Rad;

        FinalAngVel = (1.0f / Time.fixedDeltaTime) * angle * axis;

        return FinalAngVel;
    }

}
