using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConfigurableJointEventManager : MonoBehaviour
{
    [SerializeField] ConfigurableJoint Joint;
    private JointLimits Limits;

    //Event called on min reached
    public UnityEvent OnMinLimitReached;
    //Event called on max reached
    public UnityEvent OnMaxLimitReached;

    //Event called on min left
    public UnityEvent OnMinLimitUndone;
    //Event called on max left
    public UnityEvent OnMaxLimitUndone;

    //State of the joint : either reached min or max or none if in between
    public enum JointState { Min, Max, None }
    [HideInInspector] public JointState CurrentState = JointState.None;
     public int CurrentValue;

    [Range(0, 100)] [SerializeField] int MaxValue = 100;
    [Range(0, 100)] [SerializeField] int MinValue = 0;

    [SerializeField] Transform MinPoint;
    [SerializeField] Transform MaxPoint;

    void Update()
    {
        LimitDetector();
        CurrentValue = ObtainValue();
    }

    public int ObtainValue()
    {
        int FinalValue;
        float DistanceValue = Vector3.Distance(Joint.connectedBody.position, MaxPoint.position);
        float TotalDistance = Vector3.Distance(MinPoint.position, MaxPoint.position);
        float Percentage = 1 - (DistanceValue / TotalDistance);

        FinalValue = (int)(MaxValue * Percentage);

        return FinalValue;
    }

    void LimitDetector()
    {
        //Reached Min
        if (CurrentValue == MinValue)
        {
            if (CurrentState != JointState.Min)
                OnMinLimitReached.Invoke();

            CurrentState = JointState.Min;
        }
        //Reached Max
        else if (CurrentValue == MaxValue)
        {
            if (CurrentState != JointState.Max)
                OnMaxLimitReached.Invoke();

            CurrentState = JointState.Max;
        }

        //No Limit reached
        else
        {
            if (CurrentState == JointState.Max)
            {
                OnMaxLimitUndone.Invoke();
            }

            if (CurrentState == JointState.Min)
            {
                OnMinLimitUndone.Invoke();
            }

            CurrentState = JointState.None;
        }
    }
}
