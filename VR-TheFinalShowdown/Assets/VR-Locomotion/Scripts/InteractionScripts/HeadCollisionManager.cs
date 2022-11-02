using UnityEngine;

public class HeadCollisionManager : MonoBehaviour
{

    [SerializeField] Rigidbody PlayerBody;
    [SerializeField] LayerMask ObjectsToHit;  
    [SerializeField] LayerMask WaterLayer;
    [SerializeField] LayerMask ColorTriggerLayer;
    [SerializeField] float HeadCollsionRadius = 0.25f;

    [Header("FadeSpeed = 1 is 1 second")]
    [SerializeField] float FadeSpeed = 1;

    [SerializeField] GameObject HeadCollisionObjectRoot;

    public static bool HeadColliding;
    public static bool HeadUnderWater;
    private bool InColorFade;
    private float ColorTriggerFadeTime;

    [SerializeField] Color HeadHitColor;

    void DetectHitSolid()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, HeadCollsionRadius, ObjectsToHit);
        if (hitColliders.Length > 0 && !HeadColliding)
        {
            ColorFilter_Controller.HeadCollisionChange(HeadHitColor, FadeSpeed, true);
            HeadColliding = true;
            HeadCollisionObjectRoot.SetActive(true);

            PlayerBody.velocity = Vector3.zero;
        }

        if (hitColliders.Length == 0 && HeadColliding)
        {
            ColorFilter_Controller.HeadCollisionChange(ColorFilter_Controller.CurrentColor, FadeSpeed, false);
            HeadColliding = false;
            HeadCollisionObjectRoot.SetActive(false);
        }
    }
    void DetectHitWater()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, HeadCollsionRadius, WaterLayer);
        if (hitColliders.Length > 0 && !HeadUnderWater)
        {
            HeadUnderWater = true;
        }

        if (hitColliders.Length == 0 && HeadUnderWater)
        {
            HeadUnderWater = false;
        }
    }

    void DetectHitColorTrigger()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, HeadCollsionRadius, ColorTriggerLayer);
        
        if (hitColliders.Length > 0 && !InColorFade)
        {
            ColorFadeTrigger TriggerComponent = hitColliders[0].GetComponent<ColorFadeTrigger>();
            ColorTriggerFadeTime = TriggerComponent.ColorFadeTime;
            Color FadeColor = TriggerComponent.HeadsetFadeColor;

            ColorFilter_Controller.ColorFilterChange(FadeColor, ColorTriggerFadeTime, true);

            InColorFade = true;
        }

        if (hitColliders.Length == 0 && InColorFade)
        {
            ColorFilter_Controller.ColorFilterChange(ColorFilter_Controller.CurrentColor, ColorTriggerFadeTime, false);
            InColorFade = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DetectHitSolid();
        DetectHitWater();
        DetectHitColorTrigger();
    }
}
