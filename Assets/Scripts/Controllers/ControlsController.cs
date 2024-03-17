using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ControlsController : MonoBehaviour
{
    private Camera mainCamera;
    public Slider slider;

    private void Awake()
    {
        mainCamera = FindObjectOfType<Camera>();
    }

    public void OnSliderChanged()
    {
        mainCamera.fieldOfView = slider.value;
    }

    private void Start()
    {
        slider.value = math.clamp(mainCamera.fieldOfView, 10.0f, 160.0f);
    }
}