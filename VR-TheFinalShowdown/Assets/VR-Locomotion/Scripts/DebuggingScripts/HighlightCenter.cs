using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightCenter : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        Bounds bounds = rends[0].bounds;
        foreach (Renderer rend in rends)
        {
            bounds = bounds.GrowBounds(rend.bounds);
        }
        Vector3 center = bounds.center;

        //Debug.Log(bounds.size.magnitude);

        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.DrawWireSphere(center, 0.025f);
    }
#endif
}
