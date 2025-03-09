using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HotbarSystem : MonoBehaviour
{
    [Header("Hotbar Settings")]
    public int maxSlots = 3;
    public float slotSpacing = 100f;

    [Header("References")]
    public GameObject slotPrefab;
    public Transform slotsContainer;
    public Transform toolSpawnPoint;
    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    private HotbarSlot[] slots;
    private ItemData[] items;
    private int selectedSlot = -1;
    private GameObject equippedTool;

    private void Awake()
    {
        InitializeHotbar();
    }

    private void InitializeHotbar()
    {
        slots = new HotbarSlot[maxSlots];
        items = new ItemData[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * slotSpacing, 0);

            slots[i] = new HotbarSlot
            {
                background = slotObj.GetComponent<Image>(),
                itemIcon = slotObj.transform.GetChild(0).GetComponent<Image>()
            };

            slots[i].background.color = unselectedColor;
            slots[i].itemIcon.enabled = false;
        }
    }

    private void Update()
    {
        // Handle number key input (1-3)
        for (int i = 0; i < maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= maxSlots) return;

        // Update slot visuals
        if (selectedSlot >= 0 && selectedSlot < maxSlots)
        {
            slots[selectedSlot].background.color = unselectedColor;
        }

        // Unequip current tool
        if (equippedTool != null)
        {
            Destroy(equippedTool);
            equippedTool = null;
        }

        selectedSlot = index;
        slots[selectedSlot].background.color = selectedColor;

        // Handle tool equipping (slot 1 is for hammer)
        if (index == 0 && items[0] != null && items[0].itemType == ItemType.Tool)
        {
            EquipTool(items[0]);
        }
    }

    private void EquipTool(ItemData toolData)
    {
        if (toolData.prefab != null)
        {
            equippedTool = Instantiate(toolData.prefab, toolSpawnPoint.position, toolSpawnPoint.rotation);
            equippedTool.transform.parent = toolSpawnPoint;

            // Make the tool kinematic and disable gravity
            Rigidbody rb = equippedTool.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }

    public void SetItem(int slot, ItemData item)
    {
        if (slot < 0 || slot >= maxSlots) return;

        items[slot] = item;
        if (slots[slot].itemIcon != null)
        {
            slots[slot].itemIcon.enabled = item != null;
            if (item != null && item.prefab != null)
            {
                // You would set the icon sprite here based on your item data
                // slots[slot].itemIcon.sprite = item.icon;
            }
        }
    }

    public ItemData GetEquippedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < items.Length)
        {
            return items[selectedSlot];
        }
        return null;
    }

    public bool IsHammerEquipped()
    {
        return selectedSlot == 0 && items[0] != null && items[0].itemType == ItemType.Tool;
    }
}

[System.Serializable]
public class HotbarSlot
{
    public Image background;
    public Image itemIcon;
}
