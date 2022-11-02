using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XR_Manual_Teleport : MonoBehaviour
{
    [SerializeField] PlayerBodyController BodyController;

    [SerializeField] float TeleportFadeTime = 0.25f;
    [SerializeField] Color TeleportFadeColor = Color.black;
    [SerializeField] float TeleportVerticalOffset = 0.1f;

    private static XR_Manual_Teleport instance;
    private bool Teleporting;

    private void Awake()
    {
        instance = this;
    }

    public static void ManualTeleportPlayer(Transform Point)
    {
        if (!instance.Teleporting)
        {
            instance.Teleporting = true;
            instance.StartCoroutine(instance.TeleportRoutine(Point));
        }
    }

    private IEnumerator TeleportRoutine(Transform Point)
    {
        yield return new WaitForEndOfFrame();

        //Fade before Teleporting
        ColorFilter_Controller.FadePanelChange(TeleportFadeColor, TeleportFadeTime, true);
        yield return new WaitForSeconds(TeleportFadeTime);

        BodyController.PlayerBody.isKinematic = true;
        Transform Head = BodyController.BodySystem.HeadTransform;
        Vector3 TeleportPoint = Point.position;

        //Get the difference between the center of the Play Area
        Vector3 BetweenHeadAndPoint = (Head.position - BodyController.PlayerBody.position);
        //Negate the Y for simplicity
        BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

        Debug.Log(BetweenHeadAndPoint);

        //Raise us up a hair
        Vector3 FootOffset = new Vector3(0, TeleportVerticalOffset, 0);

        //Get the position of our player point and the subtract the difference, sending us right on top of the point!
        BodyController.PlayerBody.position = (TeleportPoint - BetweenHeadAndPoint) + FootOffset;

        //Fade back to normal
        ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, TeleportFadeTime, false);
        yield return new WaitForSeconds(TeleportFadeTime);

        BodyController.PlayerBody.isKinematic = false;

        Teleporting = false;

        yield return null;

    }
}
