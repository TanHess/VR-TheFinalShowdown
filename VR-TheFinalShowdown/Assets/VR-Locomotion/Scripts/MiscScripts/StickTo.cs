using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickTo : MonoBehaviour
{

    [SerializeField] GameObject ThisObject;
    public GameObject SticksToThisObject;
    [SerializeField] Vector3 WithThisOffset;
    [SerializeField] Vector3 rotationMask = new Vector3(1, 1, 1);
    [SerializeField] Vector3 TranslationMask = new Vector3(1, 1, 1);
    [SerializeField] float AndRotatesAtThisSpeed;
    [SerializeField] bool OnlyRotateHorizontal;
    [SerializeField] bool PerformUsingFixedUpdate = true;
    [SerializeField] bool PerformUsingLateUpdate = false;
    [SerializeField] bool PerformUsingUpdate = false;
    [SerializeField] bool PerformUsingParenting = false;
    // Use this for initialization
    void Start()
    {
        if (ThisObject == null)
        {
            ThisObject = this.gameObject;

            if (PerformUsingParenting)
            {
                ThisObject.transform.parent = SticksToThisObject.transform;
            }
        }
    }

    public void SetAttachPoint(Transform Target)
    {
        SticksToThisObject = Target.gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PerformUsingFixedUpdate)
        {
            ThisObject.transform.position = SticksToThisObject.transform.position;
            ThisObject.transform.position = Vector3.Scale(ThisObject.transform.position, TranslationMask);
            ThisObject.transform.localPosition += WithThisOffset;

            ThisObject.transform.rotation = Quaternion.Slerp(ThisObject.transform.rotation, Quaternion.Euler(SticksToThisObject.transform.eulerAngles.x, SticksToThisObject.transform.eulerAngles.y, SticksToThisObject.transform.eulerAngles.z), Time.deltaTime * AndRotatesAtThisSpeed);
            Vector3 lookAtRotation = ThisObject.transform.rotation.eulerAngles;
            ThisObject.transform.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, rotationMask));
            if (OnlyRotateHorizontal)
            {
                ThisObject.transform.rotation = Quaternion.Euler(0, SticksToThisObject.transform.eulerAngles.y, 0);
            }
        }
    }
    private void LateUpdate()
    {
        if (PerformUsingLateUpdate)
        {
            ThisObject.transform.position = SticksToThisObject.transform.position;
            ThisObject.transform.position = Vector3.Scale(ThisObject.transform.position, TranslationMask);
            ThisObject.transform.localPosition += WithThisOffset;

            ThisObject.transform.rotation = Quaternion.Slerp(ThisObject.transform.rotation, Quaternion.Euler(SticksToThisObject.transform.eulerAngles.x, SticksToThisObject.transform.eulerAngles.y, SticksToThisObject.transform.eulerAngles.z), Time.deltaTime * AndRotatesAtThisSpeed);
            Vector3 lookAtRotation = ThisObject.transform.rotation.eulerAngles;
            ThisObject.transform.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, rotationMask));
            if (OnlyRotateHorizontal)
            {
                ThisObject.transform.rotation = Quaternion.Euler(0, SticksToThisObject.transform.eulerAngles.y, 0);
            }
        }
    }
    private void Update()
    {
        if (PerformUsingUpdate)
        {
            ThisObject.transform.position = SticksToThisObject.transform.position;
            ThisObject.transform.position = Vector3.Scale(ThisObject.transform.position, TranslationMask);
            ThisObject.transform.localPosition += WithThisOffset;

            ThisObject.transform.rotation = Quaternion.Slerp(ThisObject.transform.rotation, Quaternion.Euler(SticksToThisObject.transform.eulerAngles.x, SticksToThisObject.transform.eulerAngles.y, SticksToThisObject.transform.eulerAngles.z), Time.deltaTime * AndRotatesAtThisSpeed);
            Vector3 lookAtRotation = ThisObject.transform.rotation.eulerAngles;
            ThisObject.transform.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, rotationMask));
            if (OnlyRotateHorizontal)
            {
                ThisObject.transform.rotation = Quaternion.Euler(0, SticksToThisObject.transform.eulerAngles.y, 0);
            }
        }
    }
}
