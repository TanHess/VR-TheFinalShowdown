using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClimbingDebugManager : MonoBehaviour
{
    [SerializeField] XRSimpleInteractable ClimbingComponent;
    [SerializeField] Color GizmoColor = Color.cyan;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (ClimbingComponent != null)
        {
            Gizmos.color = GizmoColor;

            foreach (Collider col in ClimbingComponent.colliders)
            {
                if (col is MeshCollider)
                {
                   Gizmos.DrawWireMesh((col as MeshCollider).sharedMesh, -1, col.transform.position, col.transform.rotation, col.transform.localScale);
                }

                if (col is BoxCollider)
                {
                    Gizmos.matrix = col.transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one);
                }

                if (col is SphereCollider)
                {
                    Gizmos.matrix = col.transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero, (col as SphereCollider).radius);
                }

            }
           

        }
        
        
        
    }
#endif
}
