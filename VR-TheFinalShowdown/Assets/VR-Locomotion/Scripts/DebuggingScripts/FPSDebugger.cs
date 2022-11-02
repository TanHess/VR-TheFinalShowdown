using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FPSDebugger : MonoBehaviour
{
    public int FPS { get; private set; }
    public TextMeshPro DisplayText;

    // Update is called once per frame
    void Update()
    {
        float current = (int)(1f / Time.deltaTime);
        if (Time.frameCount % 50 == 0)
        {
            DisplayText.text = current.ToString();
        }
    }
}
