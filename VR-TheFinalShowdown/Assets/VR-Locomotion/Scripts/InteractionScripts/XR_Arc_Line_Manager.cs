using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XR_Arc_Line_Manager : MonoBehaviour
{
    XR_Arc_Launch ArcLaunch;
    [SerializeField] LineRenderer LineRenderer;

    // Number of points on the line
    [SerializeField] int numPoints = 50;

    // distance between those points on the line
    [SerializeField] float timeBetweenPoints = 0.1f;

    // The physics layers that will cause the line to stop being drawn
    [SerializeField] LayerMask CollidableLayers;

    void Start()
    {
        ArcLaunch = GetComponent<XR_Arc_Launch>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ArcLaunch.AimStarted)
        {
            Transform Waist = ArcLaunch.BodyController.BodySystem.WaistTransform;

            //Get the difference between the center of the Play Area
            Vector3 BetweenHeadAndPoint = (Waist.position - ArcLaunch.BodyController.PlayerBody.position);
            //Negate the Y for simplicity
            BetweenHeadAndPoint = new Vector3(BetweenHeadAndPoint.x, 0, BetweenHeadAndPoint.z);

            LineRenderer.positionCount = (int)numPoints;
            List<Vector3> points = new List<Vector3>();
            Vector3 startingPosition = ArcLaunch.CurrentAimer.position;
            Vector3 startingVelocity = ArcLaunch.CurrentAimer.forward * ArcLaunch.CurrentChargePower;
            for (float t = 0; t < numPoints; t += timeBetweenPoints)
            {
                Vector3 newPoint = (startingPosition) + t * startingVelocity;
                newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
                points.Add(newPoint);

                if (Physics.OverlapSphere(newPoint, 0.1f, CollidableLayers).Length > 0)
                {
                    LineRenderer.positionCount = points.Count;
                    break;
                }
            }

            LineRenderer.SetPositions(points.ToArray());
        }
        
    }
}
