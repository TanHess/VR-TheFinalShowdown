using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGrappleTargets : MonoBehaviour
{
    public static List<GrappleTargetHolder> GrappleList = new List<GrappleTargetHolder>();

    public List<GrappleTargetHolder> AvailableTargets = new List<GrappleTargetHolder>();

    public float TargetMaxDistance = 25;
    public float TargetMinDistance = 5;
    [Header("This determines how closely the player must look at the grapple target.")]
    [Range(0, 1)] [SerializeField] private float ViewTolerance = 0.5f;
    [SerializeField] private Transform PlayerHead;
    [SerializeField] private Transform PlayerRoot; 

    public List<GrappleTargetHolder> GetAvailableTargets(List<GrappleTargetHolder> homingList, float maxDistance, float minDistance)
    {
        GrappleTargetHolder[] Targets = homingList.ToArray();
        List<GrappleTargetHolder> TempList = new List<GrappleTargetHolder>();

        Vector3 position = PlayerRoot.position;
        Vector3 Forward = PlayerHead.TransformDirection(Vector3.forward);

        float curDot = ViewTolerance;

        for (int i = 0; i < Targets.Length; i++)
        {

            if (Targets[i] == null)
            {
                homingList.Remove(Targets[i]);
                break;
            }
            if (Targets[i].enabled && Targets[i].gameObject.activeInHierarchy)
            {
                Vector3 diff = Targets[i].transform.position - position;
                float CurrentDistance = Vector3.Distance(Targets[i].transform.position, position);
                float actDot = Vector3.Dot(Forward.normalized, diff.normalized);

                if (CurrentDistance < maxDistance && CurrentDistance > minDistance && curDot < actDot)
                {
                    if (!TempList.Contains(Targets[i]))
                    {
                        TempList.Add(Targets[i]);
                        Targets[i].SetIconVisible(true);
                    }
                }
                else 
                {
                    Targets[i].SetIconVisible(false);
                    Targets[i].HighlightCursor(false);
                }
            }

        }
        return TempList;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        AvailableTargets = GetAvailableTargets(GrappleList, TargetMaxDistance, TargetMinDistance);
    }
}
