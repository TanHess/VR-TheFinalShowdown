using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHighlight : MonoBehaviour
{
    [SerializeField] List<ControllerButton> ControllerButtons;

    [Serializable]
    private class ControllerButton
    {
        public string Title;
        public List<MeshRenderer> Components;
        public bool Highlight;
    }

    [SerializeField] Material DefaultMaterial;
    [SerializeField] Material HighlightMaterial;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var Button in ControllerButtons)
        {
            if (Button.Highlight)
            {
                foreach (var Renderer in Button.Components)
                {
                    Renderer.material = HighlightMaterial;
                }
            }
            else
            {
                foreach (var Renderer in Button.Components)
                {
                    Renderer.material = DefaultMaterial;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
