using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GizmoHighlighter : MonoBehaviour
{
    private Collider TriggerCollider;
    [SerializeField] Color GizmoColor = Color.white;

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
