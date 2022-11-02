using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(XR_Slider_Manager))]
public class DisplaySliderValues : MonoBehaviour
{
    [SerializeField] private TextMeshPro ValueText;
    private XR_Slider_Manager Slider;

    // Start is called before the first frame update
    void Start()
    {
        Slider = GetComponent<XR_Slider_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        ValueText.text = Slider.ObtainValue().ToString();
    }

}
