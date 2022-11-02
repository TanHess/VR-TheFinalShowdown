using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(XR_Dial))]
public class DisplayDialVariables : MonoBehaviour
{
    [SerializeField] private TextMeshPro AngleText;
    [SerializeField] private TextMeshPro ValueText;
    private XR_Dial Dial;

    // Start is called before the first frame update
    void Start()
    {
        Dial = GetComponent<XR_Dial>();
    }

    // Update is called once per frame
    void Update()
    {
        AngleText.text = Dial.CurrentAngle.ToString();
        ValueText.text = Dial.CurrentValue.ToString("F2");
    }
}
