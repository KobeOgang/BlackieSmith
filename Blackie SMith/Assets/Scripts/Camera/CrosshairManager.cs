using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    [Header("Crosshair Images")]
    public Image defaultCrosshair;    // Reference to the dot crosshair UI image
    public Image handCrosshair;       // Reference to the hand crosshair UI image

    [Header("References")]
    public GrabSystem grabSystem;     // Reference to the GrabSystem
    public float checkRate = 0.05f;   // How often to check for grabbable objects (in seconds)

    private bool isCrosshairHidden = false;

    private void Start()
    {
        // Ensure we start with the default crosshair
        if (defaultCrosshair && handCrosshair)
        {
            defaultCrosshair.enabled = true;
            handCrosshair.enabled = false;
        }

        // Start checking for grabbable objects
        InvokeRepeating(nameof(CheckForGrabbableObject), 0f, checkRate);
    }

    private void CheckForGrabbableObject()
    {
        if (!defaultCrosshair || !handCrosshair) return;

        // Don't check if crosshair is hidden
        if (isCrosshairHidden) return;

        // Check if the player is currently holding an object
        if (grabSystem.IsGrabbing())
        {
            ShowHandCrosshair();
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Check if we're looking at a grabbable object
        if (Physics.Raycast(ray, out hit, grabSystem.grabDistance, grabSystem.grabbableLayer))
        {
            // Check if the object has a rigidbody and is not kinematic
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ShowHandCrosshair();
                return;
            }
        }

        ShowDefaultCrosshair();
    }

    private void ShowHandCrosshair()
    {
        defaultCrosshair.enabled = false;
        handCrosshair.enabled = true;
    }

    private void ShowDefaultCrosshair()
    {
        defaultCrosshair.enabled = true;
        handCrosshair.enabled = false;
    }

    public void HideCrosshair()
    {
        if (defaultCrosshair)
        {
            defaultCrosshair.enabled = false;
        }
        if (handCrosshair)
        {
            handCrosshair.enabled = false;
        }
        isCrosshairHidden = true;
    }

    public void ShowCrosshair()
    {
        if (defaultCrosshair)
        {
            defaultCrosshair.enabled = true;
        }
        if (handCrosshair)
        {
            handCrosshair.enabled = false;
        }
        isCrosshairHidden = false;
    }
}
