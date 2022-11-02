using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    [SerializeField] LayerMask DesiredMask;
    public UnityEvent TriggerEntered;
    public UnityEvent TriggerLeft;

    private Collider TriggerCollider;

    [SerializeField] Color GizmoColor = Color.white;

    private void OnTriggerEnter(Collider other)
    {
        if ((DesiredMask.value & (1 << other.transform.gameObject.layer)) > 0) //Look for desired layer
        {
            TriggerEntered.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((DesiredMask.value & (1 << other.transform.gameObject.layer)) > 0) //Look for desired layer
        {
            TriggerLeft.Invoke();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;

        TriggerCollider = GetComponent<Collider>();

        if (TriggerCollider is MeshCollider)
        {
            Gizmos.DrawWireMesh((TriggerCollider as MeshCollider).sharedMesh, -1, TriggerCollider.transform.position, TriggerCollider.transform.rotation, TriggerCollider.transform.localScale);
        }

        if (TriggerCollider is BoxCollider)
        {
            
            Gizmos.matrix = TriggerCollider.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        if (TriggerCollider is SphereCollider)
        {
            Gizmos.matrix = TriggerCollider.transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, (TriggerCollider as SphereCollider).radius);
        }
    }
#endif
}
