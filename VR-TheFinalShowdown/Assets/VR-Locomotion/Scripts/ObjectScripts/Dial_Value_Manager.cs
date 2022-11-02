using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(XR_Dial))]
public class Dial_Value_Manager : MonoBehaviour
{
    private XR_Dial Dial;

    private float PreviousValue;

    [Range(0, 1)] public List<float> DesiredValues;

    public UnityEvent MaxValueReached = new UnityEvent();
    public UnityEvent MinValueReached = new UnityEvent();
    public UnityEvent DesiredAngleReached = new UnityEvent();

    public UnityEvent MaxValueUndone = new UnityEvent();
    public UnityEvent MinValueUndone = new UnityEvent();
    public UnityEvent DesiredAngleUndone = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        Dial = GetComponent<XR_Dial>();
        Dial.AngleChangedEvent.AddListener(AngleChanged);

        AngleChanged(); // Just in case we start off on one of the values
    }

    void AngleChanged()
    {
        #region Perform events if we reach a value
        if (Dial.CurrentValue == 1.0f)
        {
            MaxValueReached.Invoke();
        }

        if (Dial.CurrentValue == 0.0f)
        {
            MinValueReached.Invoke();
        }

        if (DesiredValues.Contains(Dial.CurrentValue))
        {
            DesiredAngleReached.Invoke();
        }
        #endregion

        #region Perform events if we were on a value, but now we're not
        if (PreviousValue == 1.0f)
        {
            MaxValueUndone.Invoke();
        }

        if (PreviousValue == 0.0f)
        {
            MinValueUndone.Invoke();
        }

        if (DesiredValues.Contains(PreviousValue))
        {
            DesiredAngleUndone.Invoke();
        }
        #endregion 

        //--------------------------------------------------------

        PreviousValue = Dial.CurrentValue; //Store this for next time
    }

}
