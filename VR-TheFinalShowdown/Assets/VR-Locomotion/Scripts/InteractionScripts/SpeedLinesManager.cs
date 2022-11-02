using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLinesManager : MonoBehaviour
{
    [SerializeField] Rigidbody PlayerBody;

    [SerializeField] ParticleSystemRenderer JumpParticleRender;
    [SerializeField] string MaterialColorType = "_Color";
    [SerializeField] float MinSpeed = 5;
    [SerializeField] float MaxSpeed = 10;

    [SerializeField] XR_Climb ClimbComponent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (PlayerBody != null)
        {
            float SpeedValueTemp = (PlayerBody.velocity.magnitude / (MaxSpeed));
            if (PlayerBody.velocity.magnitude < MinSpeed || ClimbComponent.IsClimbing || HeadCollisionManager.HeadUnderWater)
            {
                SpeedValueTemp = 0;
            }

            Color ParticleColor = JumpParticleRender.material.GetColor(MaterialColorType);
            Color NewColor = new Color(ParticleColor.r, ParticleColor.g, ParticleColor.b, SpeedValueTemp);
            JumpParticleRender.material.SetColor(MaterialColorType, NewColor);
        }
    }
}
