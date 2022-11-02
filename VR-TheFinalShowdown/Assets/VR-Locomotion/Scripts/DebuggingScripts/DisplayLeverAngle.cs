using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[RequireComponent(typeof(XR_Lever_Manager))]
public class DisplayLeverAngle : MonoBehaviour
{ 
    [SerializeField] private TextMeshPro AngleText;
    private XR_Lever_Manager Lever;
    private HingeJoint Hinge;
    // Start is called before the first frame update
    void Start()
    {
        Lever = GetComponent<XR_Lever_Manager>();
        Hinge = Lever.hinge;
    }

    // Update is called once per frame
    void Update()
    {
        AngleText.text = Hinge.angle.ToString("F2");
    }
}
