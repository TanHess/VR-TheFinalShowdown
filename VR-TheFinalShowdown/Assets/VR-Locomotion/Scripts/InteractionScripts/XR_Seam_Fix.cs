using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XR_Seam_Fix : MonoBehaviour
{
    [SerializeField] Rigidbody PlayerBody;
    Vector3 ModdedVelocity;
    // Start is called before the first frame update


    void OnCollisionEnter(Collision Col)
    {
        Debug.Log("STOP");
        //PlayerBody.velocity = ModdedVelocity;
    }


    // Update is called once per frame
    void Update()
    {
        if (PlayerBodyController.State == PlayerBodyController.PlayerState.Grounded)
        {
            Vector3 ModdedVelocity = PlayerBody.velocity;
            ModdedVelocity.y = 0;
        }
    }
}
