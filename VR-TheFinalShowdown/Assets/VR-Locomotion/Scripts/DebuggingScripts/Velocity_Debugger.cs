using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Velocity_Debugger : MonoBehaviour
{
    [SerializeField] Rigidbody BodyToPointTo;

    [SerializeField] Transform ForwardVisualIndicator;
    [SerializeField] Transform CrossVisualIndicator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 CurrentVelocity = BodyToPointTo.velocity;
        if (CurrentVelocity != Vector3.zero)
        {
            Vector3 PerpVector = Vector3.Cross(CurrentVelocity, Vector3.up);

            ForwardVisualIndicator.rotation = Quaternion.LookRotation(CurrentVelocity, Vector3.up);
            CrossVisualIndicator.rotation = Quaternion.LookRotation(PerpVector, Vector3.up);
        }
    }
        
}
