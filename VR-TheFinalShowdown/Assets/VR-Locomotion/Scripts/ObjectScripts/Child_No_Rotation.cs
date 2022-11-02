using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Child_No_Rotation : MonoBehaviour
{

    private void LateUpdate()
    {
        //Initial Counteraction
        transform.rotation = Quaternion.Euler(transform.parent.rotation.x * -1.0f, transform.parent.rotation.y * -1.0f, transform.parent.rotation.z * -1.0f);
    }
}
