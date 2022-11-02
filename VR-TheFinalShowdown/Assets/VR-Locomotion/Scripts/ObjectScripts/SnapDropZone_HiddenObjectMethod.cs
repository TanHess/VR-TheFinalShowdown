using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapDropZone_HiddenObjectMethod : MonoBehaviour
{
    [SerializeField] private XRInteractionManager InteractionManager;
    [SerializeField] private Transform AttachTransform;

    [SerializeField] private LayerMask Layer;
    [Header("Disable other Snap Zones to mitigate issues")]
    [SerializeField] private List<SphereCollider> ListOfOtherDropZonesToDisable;

    [SerializeField] bool LookForMatchingName;
    [ConditionalHide("LookForMatchingName", true)] [SerializeField] string DesiredName;

    public GameObject CurrentAttachedObject;

    private IXRInteractable CurrentInteractable;
    private IEnumerator AttachRoutine;
    private IEnumerator DetachRoutine;
    [SerializeField] private Transform ObjectStorage;

    private bool ReadyToSnap;
    public bool HasStoredObject;

    [Header("The object stores a duplicate mimicing the appearance of our stored object")]
    [SerializeField] GameObject DittoObject;
    [SerializeField] XRSimpleInteractable DittoInteractable;
    private Vector3 OriginalDittoScale;

    public GameObject DummyDuplicate;
    private Bounds DummyBounds;
    public UnityEvent AttachedEvent;
    public UnityEvent DetachedEvent;

    public UnityEvent CorrectEvent;
    public UnityEvent IncorrectEvent;

    private void Start()
    {
        InteractionManager = FindObjectOfType<XRInteractionManager>();

        //Store these so we can put Ditto back to default later
        OriginalDittoScale = DittoObject.transform.localScale;
        DittoObject.transform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        DittoInteractable.selectEntered.AddListener(Detatch);
    }

    private void OnDisable()
    {
        DittoInteractable.selectEntered.RemoveListener(Detatch);

        //If the zone gets disabled while a valid object is near it, reset it anyway
        if (!HasStoredObject)
        {
            ResetZoneImmediate();
        } 
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = AttachTransform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
    }

   
#endif
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & Layer) != 0 && !HasStoredObject)
        {

            if (TryGetInteractable(other, out IXRInteractable interactable))
            {
                if (CurrentInteractable == null)
                {
                    //Assign our grabbable to work with
                    CurrentInteractable = interactable;
                    //Make our Ditto visible
                    DittoObject.transform.localScale = OriginalDittoScale;
                }
            }

            //This makes sure that it can't store an object unless it's being grabbed by the player
            if (CurrentInteractable != null && !ReadyToSnap)
            {
                if (interactable is IXRSelectInteractable selectInteractable)
                {
                    if (selectInteractable.isSelected)
                    {
                        ReadyToSnap = true;
                    }
                } 
            }
            //This makes other drop zones disable themselves so they can't get in the way. First come, first served!
            for (int i = 0; i < ListOfOtherDropZonesToDisable.Count; i++)
            {
                ListOfOtherDropZonesToDisable[i].enabled = false;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        //We changed our mind and left the trigger before we let go of it. 
        //We check to make sure that the object that left it is the same that went in.
        
        if (TryGetInteractable(other, out IXRInteractable interactable))
        {
            StartCoroutine(ResetZoneRoutine_EndOfFrame());
        }
    }

    private void ResetZoneImmediate()
    {
        if (CurrentInteractable != null && !HasStoredObject)
        {
            //Make the Ditto disappear
            DittoObject.transform.localScale = Vector3.zero;
            CurrentInteractable = null;
            ReadyToSnap = false;
            //Enable the other Zones again.
            for (int i = 0; i < ListOfOtherDropZonesToDisable.Count; i++)
            {
                ListOfOtherDropZonesToDisable[i].enabled = true;
            }

            DetachedEvent.Invoke();
        }
    }

    private IEnumerator ResetZoneRoutine_EndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (CurrentInteractable != null && !HasStoredObject && isActiveAndEnabled)
        {
            //Make the Ditto disappear
            DittoObject.transform.localScale = Vector3.zero;
            CurrentInteractable = null;
            ReadyToSnap = false;
            //Enable the other Zones again.
            for (int i = 0; i < ListOfOtherDropZonesToDisable.Count; i++)
            {
                ListOfOtherDropZonesToDisable[i].enabled = true;
            }

            DetachedEvent.Invoke();
        }

        yield return null;
            
    }

    private void OnTriggerStay(Collider other)
    {
        //We've gotta make sure that the CurrentInteractable is still valid while we check for its layer.
        if (((1 << other.gameObject.layer) & Layer) != 0 && CurrentInteractable != null)
        {
            //This checks to see if the object was released by the player while it's inside the trigger.
            if (CurrentInteractable is IXRSelectInteractable selectInteractable)
            {
                if (!selectInteractable.isSelected && ReadyToSnap)
                {
                    ReadyToSnap = false;
                    Attach(CurrentInteractable);
                }
            }
        }
    }

    public virtual void Attach(IXRInteractable DesiredInteractable)
    {
        AttachRoutine = AttachCoroutine(DesiredInteractable);
        StartCoroutine(AttachRoutine);
    }

    protected virtual IEnumerator AttachCoroutine(IXRInteractable DesiredInteractable)
    {
        //Force us to wait in case we are trying this too early from another script
        while (HasStoredObject)
        {
            yield return new WaitForEndOfFrame();
        }
        //Just in case CurrentInteractable comes from elsewhere!
        CurrentInteractable = DesiredInteractable;

        //Make our Ditto visible in case it isn't.
        DittoObject.transform.localScale = OriginalDittoScale;
        DittoObject.GetComponent<MeshRenderer>().enabled = false;

        //Create a clone
        DummyDuplicate = Instantiate(CurrentInteractable.transform.gameObject);
        DummyDuplicate.transform.position = DittoObject.transform.position;
        DummyBounds = GetCompoundedBounds(DummyDuplicate.transform);


        Bounds TempBounds = DummyBounds;
        while (TempBounds.size.magnitude > DittoObject.transform.lossyScale.magnitude)
        {
            DummyDuplicate.transform.localScale *= 0.99f;
            TempBounds = GetCompoundedBounds(DummyDuplicate.transform);
        }
        //First, get rid of all XR components
        foreach (var comp in DummyDuplicate.GetComponents<Component>())
        {
            if ((comp is XRBaseInteractable))
            {
                Destroy(comp);
            }
        }
        //Then, get rid of all other components except visual ones and the Transform
        foreach (var comp in DummyDuplicate.GetComponents<Component>())
        {
            if (!(comp is Transform) && !(comp is MeshFilter) && !(comp is Renderer))
            {
                Destroy(comp);
            }
        }

        //Align the clone to the center of the game object and its children
        DummyDuplicate.transform.parent = DittoObject.transform;
        DummyDuplicate.transform.localRotation = Quaternion.identity;
        DummyDuplicate.transform.localPosition = -DittoObject.transform.InverseTransformPoint(GetCompoundedBounds(DummyDuplicate.transform).center);

        //Meanwhile store the real one in a blank, disabled GameObject!
        CurrentInteractable.transform.parent = ObjectStorage;
        CurrentInteractable.transform.position = AttachTransform.position;
        CurrentInteractable.transform.localRotation = AttachTransform.rotation;

        //Restore the other drop zones
        for (int i = 0; i < ListOfOtherDropZonesToDisable.Count; i++)
        {
            ListOfOtherDropZonesToDisable[i].enabled = true;
        }

        HasStoredObject = true;

        AttachedEvent.Invoke();

        if (LookForMatchingName)
        {
            if (MatchingName(CurrentInteractable))
            {
                CorrectEvent.Invoke();
            }
            else
            {
                IncorrectEvent.Invoke();
            }
        }
        yield return null;
    }

    public virtual void Detatch(SelectEnterEventArgs args)
    {
        DetachRoutine = DetachCoroutine(args);
        StartCoroutine(DetachRoutine);
    }

    public virtual void DummyReset()
    {
        //Put our Ditto back to normal
        DittoObject.GetComponent<MeshRenderer>().enabled = true;
        Destroy(DummyDuplicate);

        //Bring our object back from its storage object
        if (CurrentInteractable.transform.parent != null)
        {
            CurrentInteractable.transform.parent = null;
        }
    }

    protected virtual IEnumerator DetachCoroutine(SelectEnterEventArgs args)
    {
        yield return new WaitForEndOfFrame();
       
        if (CurrentInteractable != null)
        {
            DummyReset();

            //Snap the object to your hand and auto-grab it
            CurrentInteractable.transform.position = AttachTransform.position;

            //Cancels the interaction with the Ditto so there is no conflict with the OnSelectExit
            InteractionManager.CancelInteractableSelection((IXRSelectInteractable)DittoInteractable);

            yield return new WaitForEndOfFrame();
            InteractionManager.SelectEnter(args.interactorObject, (IXRSelectInteractable)CurrentInteractable);

            CurrentInteractable = null;
            HasStoredObject = false;

        }

        yield return null;
    }
    private bool TryGetInteractable(Collider Other, out IXRInteractable interactable)
    {
        InteractionManager.TryGetInteractableForCollider(Other, out interactable);
        return interactable != null;
    }

    private bool MatchingName(IXRInteractable interactable)
    {
        return interactable.transform.name.Contains(DesiredName);
    }

    private Bounds GetCompoundedBounds(Transform Root)
    {
        Renderer[] rends = Root.GetComponentsInChildren<Renderer>();
        Bounds bounds = rends[0].bounds;
        foreach (Renderer rend in rends)
        {
            //Don't include particles
            if (!rend.GetComponent<ParticleSystemRenderer>())
            {
                bounds = bounds.GrowBounds(rend.bounds);
            }
            
        }
        Bounds finalBounds = bounds;

        return finalBounds;
    }
}
public static class BoundsExtension
{
    public static Bounds GrowBounds(this Bounds a, Bounds b)
    {
        Vector3 max = Vector3.Max(a.max, b.max);
        Vector3 min = Vector3.Min(a.min, b.min);

        a = new Bounds((max + min) * 0.5f, max - min);
        return a;
    }
}
