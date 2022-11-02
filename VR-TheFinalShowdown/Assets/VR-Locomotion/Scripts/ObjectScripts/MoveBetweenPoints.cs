using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [SerializeField] private List<Transform> Waypoints;
    [SerializeField] float Duration = 1;
    private float TimeProgress;

    [SerializeField] AnimationCurve MovementCurve;

    [SerializeField] Rigidbody Body;

    private Transform Target;
    private Transform PreviousPoint;
    [Header("Swap these around to start from a particular point")]
    [SerializeField] private int CurrentTargetIndex = 1;
    [SerializeField] private int PreviousTargetIndex = 0;
    private void Start()
    {
        PreviousPoint = Waypoints[PreviousTargetIndex];
        Target = Waypoints[CurrentTargetIndex];

        Body.MovePosition(PreviousPoint.position);
        Body.MoveRotation(PreviousPoint.rotation);
    }

    void FixedUpdate()
    {
        TimeProgress += Time.fixedDeltaTime;

        float FinalTimeValue = MovementCurve.Evaluate(TimeProgress / Duration);
        Body.MovePosition(Vector3.Lerp(PreviousPoint.position, Target.position, FinalTimeValue));
        Body.MoveRotation(Quaternion.Lerp(PreviousPoint.rotation, Target.rotation, FinalTimeValue));

        if (TimeProgress > Duration)
        {
            TimeProgress = TimeProgress-Duration;
            PreviousTargetIndex = CurrentTargetIndex;
            CurrentTargetIndex++;

            if (CurrentTargetIndex == Waypoints.Count)
            {
                PreviousTargetIndex = CurrentTargetIndex-1;
                CurrentTargetIndex = 0;
            }

            PreviousPoint = Waypoints[PreviousTargetIndex];
            Target = Waypoints[CurrentTargetIndex];

        }
    }

}
