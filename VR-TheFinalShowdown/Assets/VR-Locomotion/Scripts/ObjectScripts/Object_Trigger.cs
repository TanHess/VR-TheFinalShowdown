using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
public class Object_Trigger : XRBaseInteractor
{
    private IXRInteractable CurrentInteractable;
   
    [SerializeField] bool RequireMatchingName;
    [ConditionalHide("RequireMatchingName",true)] [SerializeField] string DesiredName;

    [Tooltip("You have to place the object inside. No throwing it in from far away.")]
    [SerializeField] bool RequireManualPlacement;

    public UnityEvent DefaultEnteredEvent;

    public UnityEvent CorrectEvent;
    public UnityEvent IncorrectEvent;

    public UnityEvent ResetEvent;

    [SerializeField] private bool HasItem;
    [SerializeField] private bool HoldingInside;
    private void OnTriggerEnter(Collider other)
    {
        if (TryGetInteractable(other, out IXRInteractable interactable))
        {
            if (interactable is IXRSelectInteractable selectInteractable)
            {
                if (RequireManualPlacement && selectInteractable.isSelected)
                {
                    HoldingInside = true;
                }
            }

            
        }

        if (((1 << other.gameObject.layer) & interactionLayers) != 0 && !HasItem)
        {
            SetInteractable(other);
        }   
    }
    private void OnTriggerExit(Collider other)
    {
        if (TryGetInteractable(other, out IXRInteractable interactable))
        {
            if (interactable is IXRSelectInteractable selectInteractable)
            {
                if (selectInteractable.isSelected)
                {
                    if (RequireManualPlacement)
                    {
                        HoldingInside = false;
                    }
                }
            }

            
        }

        if (((1 << other.gameObject.layer) & interactionLayers) != 0 && HasItem)
        {
            ClearInteractable(other);
        }
    }

    private void SetInteractable(Collider other)
    {
        if (TryGetInteractable(other, out IXRInteractable interactable))
        {
            if (RequireManualPlacement && !HoldingInside)
            {
                return;
            }

            if (CurrentInteractable == null && interactable is IXRSelectInteractable selectInteractable)
            {
                if (!selectInteractable.isSelected)
                {
                    CurrentInteractable = interactable;
                    HasItem = true;

                    DefaultEnteredEvent.Invoke();

                    if (RequireMatchingName)
                    {
                        if (MatchingName(interactable))
                        {
                            CorrectEvent.Invoke();
                        }
                        else
                        {
                            IncorrectEvent.Invoke();
                        }
                    }
                }
                
            }
        }
    }

    private void ClearInteractable(Collider other)
    {
        if (TryGetInteractable(other, out IXRInteractable interactable))
        {
            if (CurrentInteractable == interactable)
            {
                HasItem = false;
                HoldingInside = false;
                CurrentInteractable = null;
                ResetEvent.Invoke();
            }
        }
    }

    private bool MatchingName(IXRInteractable interactable)
    {
        return interactable.transform.name.Contains(DesiredName);
    }

    //Did we get an interactable associated with the passed-in collider?
    private bool TryGetInteractable(Collider Other, out IXRInteractable interactable)
    {
        interactionManager.TryGetInteractableForCollider(Other, out interactable);
        return interactable != null;
    }

    public override void GetValidTargets(List<IXRInteractable> validTargets)
    {
        validTargets.Clear();
        validTargets.Add(CurrentInteractable);
    }

    //We don't want this interactor to actually be able to select stuff!
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return false;
    }
}
