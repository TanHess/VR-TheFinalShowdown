using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XR_Movement_Controller))]
public class XR_Walk_Run_Haptics : MonoBehaviour
{
    private XR_Movement_Controller ArmController;
    [SerializeField] XRDirectInteractor LeftHandInteractor;
    [SerializeField] XRDirectInteractor RightHandInteractor;
    [SerializeField] PlayerBodyController BodyController;

    [SerializeField] float MaxStepsPerSecond = 3f;
    [SerializeField] float FootIntervalMultiplier = 2.0f;

    [Range(0, 1)] [SerializeField] float HapticForce = 0.25f;
    [Range(0, 1)] [SerializeField] float HapticDuration = 0.25f;
    private bool Moving;
    enum CurrentFoot { Left, Right, Both };
    private CurrentFoot StepState = CurrentFoot.Right;

    public UnityEvent LeftFootStepEvent;
    public UnityEvent RightFootStepEvent;

    // Start is called before the first frame update
    void Start()
    {
        ArmController = GetComponent<XR_Movement_Controller>();
    }

    void Update()
    {
        float InputY = Mathf.Abs(ArmController.MoveInput.y);

        if (ArmController.WalkingActivated && !Moving && PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded && ArmController.PlayerBody.velocity.magnitude > 0.1f && !HeadCollisionManager.HeadColliding)
        {
            Moving = true;
            StartCoroutine("StepRoutine");
        }
        if ((!ArmController.WalkingActivated && Moving) || PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded || ArmController.PlayerBody.velocity.magnitude < 0.1f || HeadCollisionManager.HeadColliding)
        {
            Moving = false;
            StopCoroutine("StepRoutine");
        }

    }

    IEnumerator StepRoutine()
    {
        while (true)
        {
            float InputY = Mathf.Abs(ArmController.MoveInput.y);
            float TimeToWait = (1/ InputY) * (1 / MaxStepsPerSecond) / (ArmController.LerpedSpeed / ArmController.MaxRunSpeed) / FootIntervalMultiplier;

            TimeToWait = Mathf.Clamp(TimeToWait, 1 / MaxStepsPerSecond, 1);
            
            yield return new WaitForSeconds(TimeToWait);

            switch (StepState)
                {
                    case CurrentFoot.Right:
                        RightHandInteractor.SendHapticImpulse(HapticForce, HapticDuration);
                        StepState = CurrentFoot.Left;
                    
                        RightFootStepEvent.Invoke();

                        break;
                    case CurrentFoot.Left:
                        LeftHandInteractor.SendHapticImpulse(HapticForce, HapticDuration);
                        StepState = CurrentFoot.Right;

                        LeftFootStepEvent.Invoke();

                    break;
                    case CurrentFoot.Both:
                        break;
                }

        }
    }
}
