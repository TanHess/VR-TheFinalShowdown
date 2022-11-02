using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
public class XR_Physical_Button : MonoBehaviour
{
    private XRBaseInteractable grabInteractor => GetComponent<XRBaseInteractable>();
    public bool Pressed;
    private bool BeginPress;
    private bool BeginUnPress;
    [SerializeField] Transform StartPoint;
    [SerializeField] Transform EndPoint;
    [SerializeField] float ButtonPressSpeed = 5;
    [SerializeField] float ButtonPressThreshold = 0.01f;

    public UnityEvent PressedEvent;
    public UnityEvent UnPressedEvent;
    private void OnEnable()
    {
        transform.position = StartPoint.position;

        grabInteractor.hoverEntered.AddListener(HoveredStart);
        grabInteractor.hoverExited.AddListener(HoverEnd);

    }
    private void OnDisable()
    {
        grabInteractor.hoverEntered.RemoveListener(HoveredStart);
        grabInteractor.hoverExited.RemoveListener(HoverEnd);
    }

    private void HoveredStart(HoverEnterEventArgs arg0)
    {
        if (!BeginPress)
        {
            BeginPress = true;
            BeginUnPress = false;
        }
    }

    private void HoverEnd(HoverExitEventArgs arg0)
    {
        if (!BeginUnPress)
        {
            BeginUnPress = true;
            BeginPress = false;
        }
    }

    private void Update()
    {
        if (BeginPress && !Pressed)
        {
            ButtonPress();
            BeginUnPress = false;
        }

        if (BeginUnPress)
        {
            ButtonUnPress();
            BeginPress = false;
        }
    }
    private void ButtonPress()
    {
        float step = ButtonPressSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, EndPoint.position, step);

        if (Vector3.Distance(transform.position, EndPoint.position) < ButtonPressThreshold)
        {
            Pressed = true;
            PressedEvent.Invoke();
        }
    }
    private void ButtonUnPress()
    {
        float step = ButtonPressSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, StartPoint.position, step);

        if (Vector3.Distance(transform.position, StartPoint.position) < ButtonPressThreshold)
        {
            Pressed = false;
            UnPressedEvent.Invoke();
        }
    }
}
