using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
public class XR_Joint_Grabber : MonoBehaviour
{
    [Header("The player can only take the grab handle this far before it'll manually release.")]
    [SerializeField] float GrabbingDistance = 1;

    [Header("The grabbable handle will return here after letting go.")]
    [SerializeField] Transform HandleOrigin;
    private XRBaseInteractable grabInteractor => GetComponent<XRBaseInteractable>();
    XRBaseInteractor Interactor => (XRBaseInteractor)grabInteractor.GetOldestInteractorSelecting();
    private void OnEnable()
    {
        grabInteractor.selectExited.AddListener(GrabEnd);
    }
    private void OnDisable()
    {
        grabInteractor.selectExited.RemoveListener(GrabEnd);
    }

    private void GrabEnd(SelectExitEventArgs arg0)
    {
        StartCoroutine(DetachGrab());
    }


    private IEnumerator DetachGrab()
    {
        //Wait for the grab to complete, then reposition next frame
        yield return new WaitForFixedUpdate();
        
        transform.rotation = HandleOrigin.rotation;
        transform.position = HandleOrigin.position;

        yield return null;
    }

    void ReleaseAtDistance(float MaxDistance)
    {
        if (grabInteractor.isSelected)
        {
            if (Vector3.Distance(HandleOrigin.position, Interactor.transform.position) > GrabbingDistance)
            {
                Interactor.interactionManager.SelectExit((IXRSelectInteractor)Interactor, (IXRSelectInteractable)grabInteractor);
            }
        }
    }

    private void LateUpdate()
    {
        ReleaseAtDistance(GrabbingDistance);
    }

    
}
