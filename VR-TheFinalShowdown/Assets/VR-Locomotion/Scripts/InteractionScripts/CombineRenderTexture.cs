using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineRenderTexture : MonoBehaviour
{
    [SerializeField] RenderTexture First;
    [SerializeField] RenderTexture Second;
    [SerializeField] RenderTexture Final;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Final = Graphics.Blit(First, Second);
    }
}
