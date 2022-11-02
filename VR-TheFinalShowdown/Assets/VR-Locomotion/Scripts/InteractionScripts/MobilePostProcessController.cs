using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobilePostProcessController : MonoBehaviour
{
    private MobilePostProcessing LUTControllerComponent;

    [SerializeField] List<MobilePostProcessing> PostProcessProfiles;
    public static Color CurrentColor;
    [SerializeField] Rigidbody PlayerBody;
    public bool ShouldVignette;
    [Header("Disable the component when not in use. This means no passive effects.")]
    public bool ShouldDisableWhenIdle;
    [SerializeField] float MaxVignetteSpeed;
    [SerializeField] float MaxVignettePercentage = 0.75f;
    [SerializeField] float VignetteFadeSpeed;

    static MobilePostProcessController Instance;

    [SerializeField] List<Texture2D> LUTList;

    // Start is called before the first frame update
    void Awake()
    {
        LUTControllerComponent = GetComponent<MobilePostProcessing>();
        DisablePostProcessing(true);

        CurrentColor = LUTControllerComponent.Color;
        Instance = this;
    }

    public static void FadeToLUT(int DesiredLUT, float LUTSeconds, bool FadeInBool)
    {
        Instance.LUTControllerComponent.SourceLut = Instance.LUTList[DesiredLUT];
        if (FadeInBool)
        {
            Instance.StartCoroutine(Instance.LUTRoutine(LUTSeconds,true));
        }
        else 
        {
            Instance.StartCoroutine(Instance.LUTRoutine(LUTSeconds,false));
        }
        
        return;
    }

    public static void FadeToColor(Color DesiredColor, float FadeSeconds, bool FadeInBool)
    {
        CurrentColor = Instance.LUTControllerComponent.Color;
        if (FadeInBool)
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, true));
        }
        else
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, false));
        }

        return;
    }

    public static void SettingsSwitcher(int DesiredProfile, float FadeSeconds)
    {
        
        Instance.StartCoroutine(Instance.PostProcessRoutine(FadeSeconds*2, DesiredProfile));

        return;
    }

    public void ToggleVignette()
    {
        ShouldVignette = !ShouldVignette;
    }
    void VignetteManager()
    {
        float VignetteFactor = PlayerBody.velocity.magnitude / MaxVignetteSpeed;
        VignetteFactor = Mathf.Clamp(VignetteFactor, 0, MaxVignettePercentage);
        LUTControllerComponent.VignetteAmount = Mathf.Lerp(LUTControllerComponent.VignetteAmount, VignetteFactor, Time.deltaTime * VignetteFadeSpeed);
        return;
    }

    IEnumerator LUTRoutine(float FadeSeconds, bool FadeIn)
    {
        yield return new WaitForEndOfFrame();

        float CurrentAmount;
        float StartAmount = LUTControllerComponent.LutAmount;
        float TargetAmount = (FadeIn) ? 1 : 0;

        float CurrentTime = 0;
        while (CurrentTime < FadeSeconds)
        {
            CurrentTime += Time.deltaTime;
            CurrentAmount = Mathf.Lerp(StartAmount, TargetAmount, CurrentTime / FadeSeconds);
            LUTControllerComponent.LutAmount = CurrentAmount;
            yield return new WaitForEndOfFrame();
        }

    }

    IEnumerator ColorRoutine(Color DesiredEndColor,float FadeSeconds, bool FadeIn)
    {
        yield return new WaitForEndOfFrame();

        Color StartColor = LUTControllerComponent.Color;
        float TargetAmount = (FadeIn) ? 1 : 0;
        Color EndColor = new Color(DesiredEndColor.r, DesiredEndColor.g, DesiredEndColor.b, TargetAmount);
        Color FinalColor;
        float CurrentTime = 0;
        while (CurrentTime < FadeSeconds)
        {
            CurrentTime += Time.deltaTime;
            FinalColor = Color.Lerp(StartColor, EndColor, CurrentTime / FadeSeconds);
            LUTControllerComponent.Color = FinalColor;
            yield return new WaitForEndOfFrame();
        }
        LUTControllerComponent.Color = DesiredEndColor;

    }
    IEnumerator PostProcessRoutine(float FadeSeconds, int DesiredProfile)
    {

        yield return new WaitForEndOfFrame();

        //Fade to black
        FadeToColor(Color.black, FadeSeconds, true);
        //Get the Color of the incoming profile, so we know what to fade from later
        Color TempColor = PostProcessProfiles[DesiredProfile].Color;
        //Wait for the full fade before we change
        yield return new WaitForSeconds(FadeSeconds);
        //Kill the current profile and replace it with the one from the list
        Destroy(GetComponent<MobilePostProcessing>());
        LUTControllerComponent = ComponentCopy.AddComponent(gameObject, PostProcessProfiles[DesiredProfile]);
        LUTControllerComponent.Color = Color.black;
        //Fade to the new profile
        FadeToColor(TempColor, FadeSeconds, false);

    }




    // Update is called once per frame
    void Update()
    {
        if (ShouldVignette)
        {
            VignetteManager();
        }
        else 
        {
            if (LUTControllerComponent.VignetteAmount != 0)
            {
                LUTControllerComponent.VignetteAmount = 0;
            }
        } 
    }

    public void DisablePostProcessing(bool Toggle)
    {
        
    }

    IEnumerator DisableComponentRoutine(float TimeToRemainToggled)
    {
        yield return new WaitForEndOfFrame();

        if (ShouldDisableWhenIdle)
        {
            yield return new WaitForSeconds(TimeToRemainToggled);

            Instance.enabled = true;
        }

       
    }
}




