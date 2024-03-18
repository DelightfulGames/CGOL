using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlusController : MonoBehaviour
{
    public Image[] VerticalImages;
    public Image[] HorizontalImages;

    private bool horizontalVisible = false;

    private void Update()
    {
        foreach (var image in HorizontalImages)
            image.enabled = horizontalVisible;
        foreach (var image in VerticalImages)
            image.enabled = !horizontalVisible;
        horizontalVisible = !horizontalVisible;
    }
}