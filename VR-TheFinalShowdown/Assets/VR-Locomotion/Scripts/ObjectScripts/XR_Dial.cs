using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class XR_Dial : MonoBehaviour
{
    [SerializeField] Transform DialRoot;
    [Range(0, 180)] [SerializeField] private int snapRotationAmount = 25;
    [Tooltip("The hand has to rotate this much to make the dial turn")]
    [Range(5, 90)] [SerializeField] private float angleTolerance = 15;
    
    [Range(-720, 0)] [SerializeField] private float MinimumAngle = -360;
    [Range(0, 720)] [SerializeField] private float MaximumAngle = 360;

    [Header("The player can only grab it this far before it'll manually release.")]
    [SerializeField] float GrabbingDistance = 0.5f;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.25f;

    private float startAngle;
    private bool requiresStartAngle = true;
    private bool shouldGetHandRotation = false;

    public UnityEvent AngleChangedEvent = new UnityEvent();

    [HideInInspector] public float CurrentAngle;
    [HideInInspector] public float CurrentValue;
    private XRBaseInteractable grabInteractor => GetComponent<XRBaseInteractable>();
    XRBaseInteractor Interactor => (XRBaseInteractor)grabInteractor.GetOldestInteractorSelecting();

    private void OnEnable()
    {
        grabInteractor.selectEntered.AddListener(GrabbedBy);
        grabInteractor.selectExited.AddListener(GrabEnd);

        CurrentValue = GetPercentageBetween(CurrentAngle, MinimumAngle, MaximumAngle); //Grab the current value
    }
    private void OnDisable()
    {
        grabInteractor.selectEntered.RemoveListener(GrabbedBy);
        grabInteractor.selectExited.RemoveListener(GrabEnd);
    }

    private void GrabEnd(SelectExitEventArgs arg0)
    {
        shouldGetHandRotation = false;
        requiresStartAngle = true;
    }

    private void GrabbedBy(SelectEnterEventArgs arg0)
    {
        Interactor.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        shouldGetHandRotation = true;
        startAngle = 0f;
    }

    void ReleaseAtDistance(float MaxDistance)
    {
        if (grabInteractor.isSelected)
        {
            if (Vector3.Distance(transform.position, Interactor.transform.position) > GrabbingDistance)
            {
                Interactor.interactionManager.SelectExit((IXRSelectInteractor)Interactor, (IXRSelectInteractable)grabInteractor);
            }
        }
    }

    void Update()
    {
        ReleaseAtDistance(GrabbingDistance);

        if (shouldGetHandRotation)
        {
            var rotationAngle = GetInteractorRotation(); //gets the current controller angle
            GetRotationDistance(rotationAngle);
        }
    }

    public float GetInteractorRotation() => Interactor.GetComponent<Transform>().eulerAngles.z;

    #region TheMath!
    private void GetRotationDistance(float currentAngle)
    {
        if (!requiresStartAngle)
        {
            var angleDifference = Mathf.Abs(startAngle - currentAngle);

            if (angleDifference > angleTolerance)
            {
                if (angleDifference > 270f) //checking to see if the user has gone from 0-360 - a very tiny movement but will trigger the angletolerance
                {
                    float angleCheck;

                    if (startAngle < currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);

                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialClockwise();
                            startAngle = currentAngle;
                        }
                    }
                    else if (startAngle > currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);

                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialCounterClockwise();
                            startAngle = currentAngle;
                        }
                    }
                }
                else
                {
                    if (startAngle < currentAngle)
                    {
                        RotateDialCounterClockwise();
                        startAngle = currentAngle;
                    }
                    else if (startAngle > currentAngle)
                    {
                        RotateDialClockwise();
                        startAngle = currentAngle;
                    }
                }
            }
        }
        else
        {
            requiresStartAngle = false;
            startAngle = currentAngle;
        }
    }
    #endregion

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;
    private float GetPercentageBetween(float CurrentAngle, float min, float max) => (CurrentAngle - min) / (max - min);

    private void RotateDialClockwise()
    {
        if ((CurrentAngle + snapRotationAmount) <= MaximumAngle) // Keeps us from going beyond the max value
        {
            DialRoot.localEulerAngles = new Vector3(DialRoot.localEulerAngles.x,
                                                  DialRoot.localEulerAngles.y,
                                                  DialRoot.localEulerAngles.z + snapRotationAmount);
            CurrentAngle += snapRotationAmount;
            Interactor.GetComponent<XRDirectInteractor>().SendHapticImpulse(HapticForce, HapticDuration);

            CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
            CurrentValue = GetPercentageBetween(CurrentAngle, MinimumAngle, MaximumAngle);

            AngleChangedEvent.Invoke();
        }

    }

    private void RotateDialCounterClockwise()
    {
        if ((CurrentAngle - snapRotationAmount) >= MinimumAngle) // Keeps us from going beyond the min value
        {
            DialRoot.localEulerAngles = new Vector3(DialRoot.localEulerAngles.x,
                                                  DialRoot.localEulerAngles.y,
                                                  DialRoot.localEulerAngles.z - snapRotationAmount);
            CurrentAngle -= snapRotationAmount;
            Interactor.GetComponent<XRDirectInteractor>().SendHapticImpulse(HapticForce, HapticDuration);

            CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
            CurrentValue = GetPercentageBetween(CurrentAngle, MinimumAngle, MaximumAngle);

            AngleChangedEvent.Invoke();
        }

    }

}
