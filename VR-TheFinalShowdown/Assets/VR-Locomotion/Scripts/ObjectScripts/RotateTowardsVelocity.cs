using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsVelocity : MonoBehaviour
{
    [SerializeField] Rigidbody RB;
    [SerializeField] float RotationSpeed;
    [SerializeField] Transform ObjectToRotate;
    [SerializeField] Vector3 rotationMask = new Vector3(1,1,1);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (RB != null)
        {
            float singlestep = RotationSpeed * Time.deltaTime;
            Vector3 Direction = Vector3.RotateTowards(ObjectToRotate.forward, RB.velocity, singlestep, 0.0f);

            ObjectToRotate.rotation = Quaternion.LookRotation(Direction);

            Vector3 lookAtRotation = Quaternion.LookRotation(Direction).eulerAngles;
            ObjectToRotate.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, rotationMask));
        }
    }
}
