using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class XR_MirrorStep : MonoBehaviour
{
    [SerializeField] private Vector2 MinimumPlayAreaSize = new Vector2(2.5f,2.5f);

    private float playAreaSizeX;
    private float playAreaSizeZ;
    [SerializeField] private List<GameObject> TurnTriggers;
    [Range(0, 3)] [SerializeField] private float TriggerThickness;
    [Range(0, 3)] [SerializeField] private float TriggerHeight;

    [SerializeField] private Transform HeadTransform;
    [SerializeField] Rigidbody PlayerBody;

    [SerializeField] Color TurnFadeColor = Color.black;
    [Range(0, 2)] [SerializeField] float TurnFadeTime = 0.25f;
    [Range(0, 3)] [SerializeField] float CooldownTime = 2.0f;

    private XRInputSubsystem SubSystem;
    private bool Turning;
    private bool CoolingDown;
    // Start is called before the first frame update
    public void Start()
    {
        StartCoroutine(SetupRoutine());
        SubSystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRInputSubsystem>();

        if (SubSystem != null)
        {
            SubSystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            SubSystem.TryRecenter();
        }
    }

    IEnumerator SetupRoutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(2f);
        SetBoundaryPoints();
    }

    public void MirrorTurn()
    {
        if (!Turning && !CoolingDown)
        {
            StartCoroutine(TurnWorldAction());
        }
        
    }

    public void SetTurning(bool TurnVar)
    {
        Turning = TurnVar;
    }

    private IEnumerator TurnWorldAction()
    {
        ColorFilter_Controller.FadePanelChange(TurnFadeColor, TurnFadeTime, true);

        yield return new WaitForSeconds(TurnFadeTime);
        transform.RotateAround(HeadTransform.position, transform.up, 180);
        CoolingDown = true;
        ColorFilter_Controller.FadePanelChange(ColorFilter_Controller.CurrentColor, TurnFadeTime, false);

        yield return new WaitForSeconds(CooldownTime);
        CoolingDown = false;
        yield return null;
    }

    public void SetBoundaryPoints()
    {
        if (DeviceSetupManager.IsOculus)
        {
            Vector3 Dimensions;
            Unity.XR.Oculus.Boundary.GetBoundaryDimensions(Unity.XR.Oculus.Boundary.BoundaryType.PlayArea, out Dimensions);

            playAreaSizeX = Mathf.Abs(Dimensions.x);
            playAreaSizeZ = Mathf.Abs(Dimensions.z);
        }
        else 
        {
            List<Vector3> Points = new List<Vector3>();
            SubSystem.TryGetBoundaryPoints(Points);

            playAreaSizeX = Mathf.Abs(Points[0].x);
            playAreaSizeZ = Mathf.Abs(Points[0].z);
        }

        if (playAreaSizeX < MinimumPlayAreaSize.x)
        {
            playAreaSizeX = MinimumPlayAreaSize.x;
        }
        if (playAreaSizeZ < MinimumPlayAreaSize.y)
        {
            playAreaSizeZ = MinimumPlayAreaSize.y;
        }

        TurnTriggers[0].transform.localPosition = new Vector3(0, TriggerHeight / 2, (playAreaSizeZ / 2) - (TriggerThickness / 2));
        TurnTriggers[1].transform.localPosition = new Vector3(0, TriggerHeight / 2, (-playAreaSizeZ / 2) + (TriggerThickness / 2));
        TurnTriggers[2].transform.localPosition = new Vector3(playAreaSizeX / 2 - (TriggerThickness / 2), TriggerHeight / 2, 0);
        TurnTriggers[3].transform.localPosition = new Vector3((-playAreaSizeX / 2) + (TriggerThickness / 2), TriggerHeight / 2, 0);

        TurnTriggers[0].transform.localScale = new Vector3(playAreaSizeX - TriggerThickness, TriggerHeight, TriggerThickness);
        TurnTriggers[1].transform.localScale = new Vector3(playAreaSizeX - TriggerThickness, TriggerHeight, TriggerThickness);
        TurnTriggers[2].transform.localScale = new Vector3(TriggerThickness, TriggerHeight, playAreaSizeZ - TriggerThickness);
        TurnTriggers[3].transform.localScale = new Vector3(TriggerThickness, TriggerHeight, playAreaSizeZ - TriggerThickness);
    }

}
