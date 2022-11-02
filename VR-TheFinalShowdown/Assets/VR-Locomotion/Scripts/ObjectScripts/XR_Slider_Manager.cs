using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class XR_Slider_Manager : MonoBehaviour
{   
    public enum SliderState { Min, Max, Secret, None }

    //Event called on each change
    public UnityEvent OnValueChange;

    //Event called on min reached
    public UnityEvent OnMinLimitReached;
    //Event called on max reached
    public UnityEvent OnMaxLimitReached;
    //Event called on secret point reached
    public UnityEvent OnSecretReached;

    //Event called on min left
    public UnityEvent OnMinLimitUndone;
    //Event called on max left
    public UnityEvent OnMaxLimitUndone;
    //Event called on secret point left
    public UnityEvent OnSecretUndone;

    [Header("The player can only grab it this far before it'll manually release.")]
    [SerializeField] float GrabbingDistance = 0.5f;

    [SerializeField] Transform MinPoint;
    [SerializeField] Transform MaxPoint;

    [Header("Fires off events at these values")]
    [Range(0, 100)] [SerializeField] int MaxValue = 100;
    [Range(0, 100)] [SerializeField] int MinValue = 0;
    [Range(0, 100)] public List<float> DesiredValues;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.25f;

    //State of the joint : either reached min or max or none if in between
    [HideInInspector] public SliderState SliderCurrentState = SliderState.None;

    [SerializeField] XRBaseInteractable grabInteractor;
    XRBaseInteractor Interactor => (XRBaseInteractor)grabInteractor.GetOldestInteractorSelecting();

    public int CurrentValue;
    private int PreviousValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ReleaseAtDistance(GrabbingDistance);
        LimitDetector();
        ValueChange();
        CurrentValue = ObtainValue();
    }

    public int ObtainValue()
    {
        int FinalValue;
        float DistanceValue = Vector3.Distance(grabInteractor.transform.position, MaxPoint.position);
        float TotalDistance = Vector3.Distance(MinPoint.position, MaxPoint.position);
        float Percentage = 1 - (DistanceValue / TotalDistance);

        FinalValue = (int)(MaxValue * Percentage);

        return FinalValue;
    }

    void ValueChange()
    {
        if (PreviousValue != CurrentValue && grabInteractor.isSelected)
        {
            Interactor.GetComponent<XRDirectInteractor>().SendHapticImpulse(HapticForce, HapticDuration);
            PreviousValue = CurrentValue;

            OnValueChange.Invoke();
        }
        
    }

    void LimitDetector()
    {
        //Reached Min
        if (CurrentValue == MinValue)
        {
            if (SliderCurrentState != SliderState.Min)
                OnMinLimitReached.Invoke();

            SliderCurrentState = SliderState.Min;
        }
        //Reached Max
        else if (CurrentValue == MaxValue)
        {
            if (SliderCurrentState != SliderState.Max)
                OnMaxLimitReached.Invoke();

            SliderCurrentState = SliderState.Max;
        }

        //Reached Secret
        else if (DesiredValues.Contains(CurrentValue))
        {
            if (SliderCurrentState != SliderState.Secret)
                OnSecretReached.Invoke();

            SliderCurrentState = SliderState.Secret;
        }

        //No Limit reached
        else
        {
            if (SliderCurrentState == SliderState.Max)
            {
                OnMaxLimitUndone.Invoke();
            }

            if (SliderCurrentState == SliderState.Min)
            {
                OnMinLimitUndone.Invoke();
            }

            if (SliderCurrentState == SliderState.Secret)
            {
                OnSecretUndone.Invoke();
            }

            SliderCurrentState = SliderState.None;
        }
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

}
