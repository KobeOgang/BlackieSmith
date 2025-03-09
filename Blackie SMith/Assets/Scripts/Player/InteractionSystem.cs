using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("UI References")]
    public GameObject itemNamePanel;
    public TextMeshProUGUI itemNameText;

    [Header("References")]
    public Camera playerCamera;
    public GrabSystem grabSystem;
    public AnvilSystem anvilSystem;
    public HotbarSystem hotbarSystem;
    private ItemIdentifier hoveredItem;
    private Outline hoveredOutline;


    private void Start()
    {
       
    }

    private void Update()
    {
        HandleInteractionRaycast();
        HandleItemPlacement();

        // Only handle hammer strike if we're not grabbing an item
        if (!grabSystem.IsGrabbing())
        {
            HandleHammerStrike();
        }
    }

    private void HandleInteractionRaycast()
    {
        // Skip raycast if we're already grabbing something
        if (grabSystem.IsGrabbing()) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            ItemIdentifier item = hit.collider.GetComponent<ItemIdentifier>();

            // If no ItemIdentifier on the hit collider, try to find it in the parent
            if (item == null)
            {
                item = hit.collider.GetComponentInParent<ItemIdentifier>();
            }

            if (item != null)
            {
                if (hoveredItem != item)
                {
                    // Unhighlight previous item
                    if (hoveredOutline != null)
                        hoveredOutline.enabled = false;

                    // Highlight new item
                    hoveredItem = item;
                    hoveredOutline = item.GetComponent<Outline>();
                    if (hoveredOutline != null)
                        hoveredOutline.enabled = true;

                    // Update UI
                    if (item.itemData != null)
                    {
                        itemNameText.text = item.itemData.displayName;
                        itemNamePanel.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemIdentifier on {item.gameObject.name} has no ItemData assigned!");
                        itemNamePanel.SetActive(false);
                    }
                }
            }
        }
        else
        {
            // Nothing hovered
            if (hoveredOutline != null)
                hoveredOutline.enabled = false;

            hoveredItem = null;
            itemNamePanel.SetActive(false);
        }
    }

    private void HandleItemPlacement()
    {
        if (!grabSystem.IsGrabbing()) return;

        ItemIdentifier heldItem = grabSystem.GetHeldItem();
        if (heldItem == null) return;

        Transform snapPoint;
        if (anvilSystem.TrySnapItem(heldItem, out snapPoint))
        {
            // Snap the item to the anvil
            heldItem.transform.position = snapPoint.position;
            heldItem.transform.rotation = snapPoint.rotation;
            heldItem.GetComponent<Rigidbody>().isKinematic = true;
            grabSystem.ReleaseItem();
        }
    }

    private void HandleHammerStrike()
    {
        HammerEquipSystem hammerSystem = FindObjectOfType<HammerEquipSystem>();

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("LMB Pressed"); // Check if LMB is actually detected

            if (hammerSystem != null && hammerSystem.IsHammerEquipped())
            {
                Debug.Log("Hammer is Equipped"); // Check if hammer detection works

                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, interactionDistance))
                {
                    Debug.Log($"Raycast hit: {hit.collider.gameObject.name}"); // Check what object is hit

                    if (hit.collider.GetComponent<AnvilSystem>() != null)
                    {
                        Debug.Log("Anvil Hit Detected!"); // If this doesn't show, the anvil isn't being detected

                        anvilSystem.OnHammerStrike(hit.point);
                    }
                }
                else
                {
                    Debug.Log("Raycast didn't hit anything.");
                }
            }
            else
            {
                Debug.Log("Hammer is NOT equipped.");
            }
        }
    }
}
