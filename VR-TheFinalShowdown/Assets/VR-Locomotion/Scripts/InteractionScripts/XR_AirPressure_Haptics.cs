using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerBodyController))]

public class XR_AirPressure_Haptics : MonoBehaviour
{
    [SerializeField] XRDirectInteractor LeftHandInteractor;
    [SerializeField] XRDirectInteractor RightHandInteractor;
    private PlayerBodyController BodyController;

    [Header("Rushing Air Haptics Min")]
    [Range(0, 20)] [SerializeField] float MinWindSpeed;

    [Header("Rushing Air Haptics Max")]
    [Range(0, 20)] [SerializeField] float MaxWindSpeed;

    [Range(0, 5)] [SerializeField] float HapticForceMultiplier = 1.0f;

    bool Falling;

    // Start is called before the first frame update
    void Start()
    {
        BodyController = GetComponent<PlayerBodyController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PlayerBodyController.State != PlayerBodyController.PlayerState.Grounded)
        {
            if (!Falling)
            {
                Falling = true;
            }

            float AirSpeed = Mathf.Abs(BodyController.PlayerBody.velocity.y);
           // Debug.Log(AirSpeed);
            if (AirSpeed > MinWindSpeed)
            {
                float SpeedFactor = (AirSpeed - MinWindSpeed) / (MaxWindSpeed);

                //Debug.Log(SpeedFactor);

                LeftHandInteractor.SendHapticImpulse(SpeedFactor * HapticForceMultiplier, Time.deltaTime);
                RightHandInteractor.SendHapticImpulse(SpeedFactor * HapticForceMultiplier, Time.deltaTime);
            }
        }
    }
}
