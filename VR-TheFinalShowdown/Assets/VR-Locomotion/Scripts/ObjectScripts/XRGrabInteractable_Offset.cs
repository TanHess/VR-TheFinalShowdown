using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractable_Offset : XRGrabInteractable
{
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        MatchAttachmentPoints(args.interactorObject);
    }
    private void MatchAttachmentPoints(IXRInteractor Interactor)
    {
        bool IsDirect = Interactor is XRDirectInteractor;

        attachTransform.position = IsDirect ? Interactor.GetAttachTransform(this).position : transform.position;
        attachTransform.rotation = IsDirect ? Interactor.GetAttachTransform(this).rotation : transform.rotation;
    }


}
