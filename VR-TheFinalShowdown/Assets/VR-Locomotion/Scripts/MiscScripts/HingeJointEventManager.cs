using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HingeJointEventManager : MonoBehaviour
{
    [SerializeField] HingeJoint Joint;
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
    [HideInInspector] public int CurrentValue;

    [Header("The hinge has to be within this angle to qualify as reached")]
    [SerializeField] int AngleThreshold = 5;


    // Start is called before the first frame update
    void Start()
    {
        Limits = Joint.limits;
        CurrentValue = (int)Joint.angle;
    }

    void Update()
    {
        LimitDetector();
        CurrentValue = (int)Joint.angle;
    }

    void LimitDetector()
    {
        //Reached Min
        if (CurrentValue <= (int)Limits.min + AngleThreshold)
        {
            if (CurrentState != JointState.Min)
                OnMinLimitReached.Invoke();

            CurrentState = JointState.Min;
        }
        //Reached Max
        else if (CurrentValue >= (int)Limits.max - AngleThreshold)
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
