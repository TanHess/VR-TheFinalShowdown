using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap_Assignment : MonoBehaviour
{
    [Serializable]
    private class MinimapElement
    {
        public Transform TargetObject;
        public Sprite IconSprite;
        public Color IconColor =  Color.white;
        public float IconScale = 1.0f;
    }

    [SerializeField] GameObject IconPrefab;
    [SerializeField] MinimapElement[] Minimaps;

    
    // Start is called before the first frame update
    void Start()
    {
        PopulateSprites();
    }

    //Attaches Minimap icons to the object
    void PopulateSprites()
    {
        for (int i = 0; i < Minimaps.Length; i++)
        {
            GameObject IconClone = Instantiate(IconPrefab, Minimaps[i].TargetObject.position, Quaternion.identity);
            IconClone.transform.parent = Minimaps[i].TargetObject;
            
            Image IconImage = IconClone.GetComponentInChildren<Image>();
            IconImage.sprite = Minimaps[i].IconSprite;
            IconImage.color = Minimaps[i].IconColor;
            IconImage.transform.localScale *= Minimaps[i].IconScale;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
