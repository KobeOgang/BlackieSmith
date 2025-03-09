using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public HotbarSystem hotbarSystem;
    public ItemDatabase itemDatabase;

    [Header("Initial Setup")]
    public string hammerItemId = "hammer"; // The ID of your hammer item in the database

    private void Start()
    {
        InitializeHotbar();
    }

    private void InitializeHotbar()
    {
        // Get the hammer item data from the database
        ItemData hammerItemData = itemDatabase.GetItem(hammerItemId);

        if (hammerItemData != null)
        {
            // Add the hammer to the first slot (index 0)
            hotbarSystem.SetItem(0, hammerItemData);
            Debug.Log("Added hammer to hotbar slot 1");
        }
        else
        {
            Debug.LogError($"Hammer item with ID '{hammerItemId}' not found in the database!");
        }
    }
}
