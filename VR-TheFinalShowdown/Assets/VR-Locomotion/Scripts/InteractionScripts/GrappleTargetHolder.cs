using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleTargetHolder : MonoBehaviour {

    bool added = false;

    [SerializeField] private GameObject Icon;
    [SerializeField] private Animator IconHighlight;
    [SerializeField] private string HighlightCommand;

    private void Awake()
    {
        SetIconVisible(false);
    }

    private void Update()
    {
        if (GetGrappleTargets.GrappleList != null && !added)
        {
            GetGrappleTargets.GrappleList.Add(this);
            added = true;
        }
    }

    public void SetIconVisible(bool Value)
    {
        if (Icon != null)
        {
            if (Value && !Icon.activeSelf)
            {
                Icon.SetActive(true);
            }
            if (!Value && Icon.activeSelf)
            {
                Icon.SetActive(false);
            }
        } 
    }

    public void HighlightCursor(bool Value)
    {
        if (IconHighlight != null)
        {
            IconHighlight.SetBool(HighlightCommand, Value);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

}
