using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsIgnore : MonoBehaviour
{
    [SerializeField] Collider ThisCollider;
    [SerializeField] List<Collider> CollidersToIgnore;
    Rigidbody RB;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var Collider in CollidersToIgnore)
        {
            Physics.IgnoreCollision(ThisCollider, Collider);
        }
    }
}
