using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Canvas))]
public class AsssignCanvasCamera : MonoBehaviour
{
    [Tooltip("This will assign the canvas's Event Camera to the Main camera.")]
    private Canvas ThisCanvas;

    // Start is called before the first frame update
    void OnEnable()
    {
        ThisCanvas = GetComponent<Canvas>();
        ThisCanvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
