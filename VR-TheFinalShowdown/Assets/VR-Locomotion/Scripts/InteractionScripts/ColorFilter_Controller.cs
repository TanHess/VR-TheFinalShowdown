using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorFilter_Controller : MonoBehaviour
{
    [SerializeField] MeshRenderer ColorFilterPanel;
    [SerializeField] MeshRenderer FadePanel;
    [SerializeField] MeshRenderer HeadCollisionPanel;

    public Color DefaultColor = Color.white;
    public static Color CurrentColor;
    static ColorFilter_Controller Instance;

    [HideInInspector] public Color CurrentEndColor = Color.white;

    bool ColorFading;
    Coroutine ColorCo;
    // Start is called before the first frame update
    void Awake()
    {
        CurrentColor = DefaultColor;
        Instance = this;
        Instance.ColorFilterPanel.material.color = DefaultColor;
        Instance.FadePanel.material.color = DefaultColor;
        Instance.HeadCollisionPanel.material.color = DefaultColor;

    }

    public static void ColorFilterChange(Color DesiredColor, float FadeSeconds, bool FadeInBool)
    {
        if (Instance.ColorFading && Instance.ColorCo != null)
        {
            Instance.StopCoroutine(Instance.ColorCo);
        }
        

        if (FadeInBool)
        {
            Instance.ColorCo = Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, true, Instance.ColorFilterPanel.material));
        }
        else
        {
            Instance.ColorCo = Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, false, Instance.ColorFilterPanel.material));
        }

        return;
    }

    public static void HeadCollisionChange(Color DesiredColor, float FadeSeconds, bool FadeInBool)
    {
        if (FadeInBool)
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, true, Instance.HeadCollisionPanel.material));
        }
        else
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, false, Instance.HeadCollisionPanel.material));
        }

        return;
    }

    public static void FadePanelChange(Color DesiredColor, float FadeSeconds, bool FadeInBool)
    {
        if (FadeInBool)
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, true, Instance.FadePanel.material));
        }
        else
        {
            Instance.StartCoroutine(Instance.ColorRoutine(DesiredColor, FadeSeconds, false, Instance.FadePanel.material));
        }

        return;
    }

    IEnumerator ColorRoutine(Color DesiredEndColor,float FadeSeconds, bool FadeIn, Material DesiredMat)
    {
        yield return new WaitForEndOfFrame();

        ColorFading = true;

        Color StartColor = DesiredMat.color;
        float TargetAmount = (FadeIn) ? 1 : 0;
        Color EndColor = new Color(DesiredEndColor.r, DesiredEndColor.g, DesiredEndColor.b, TargetAmount);

        Color FinalColor;
        float CurrentTime = 0;
        while (CurrentTime < FadeSeconds)
        {
            CurrentTime += Time.deltaTime;
            FinalColor = Color.Lerp(StartColor, EndColor, CurrentTime / FadeSeconds);
            DesiredMat.color = FinalColor;
            yield return new WaitForEndOfFrame();
        }

        DesiredMat.color = DesiredEndColor;

        ColorFading = false;

    }
}




