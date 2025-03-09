using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ForgeSystem : MonoBehaviour
{
    [Header("Forge Settings")]
    public int maxSlots = 3;
    public Transform[] slotPositions;
    public Transform ingotSpawnPoint;

    [Header("UI References")]
    public GameObject forgeUIPanel;
    public GameObject timerPrefab;
    public Transform timerContainer;
    public float timerSpacing = 30f; // Spacing between timer UI elements

    [Header("References")]
    public ItemDatabase itemDatabase;

    private List<ForgeSlot> activeSlots = new List<ForgeSlot>();
    private List<Transform> availableSlots;
    private HashSet<GameObject> processedOres = new HashSet<GameObject>();

    private void Start()
    {
        availableSlots = new List<Transform>(slotPositions);
        forgeUIPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (availableSlots.Count == 0) return;

        ItemIdentifier item = other.GetComponent<ItemIdentifier>();
        if (item != null && item.itemData.itemType == ItemType.Ore)
        {
            // Check if this ore is already being processed
            if (processedOres.Contains(item.gameObject))
                return;

            PlaceOreInForge(item);
        }
    }

    private void PlaceOreInForge(ItemIdentifier item)
    {
        if (activeSlots.Count >= maxSlots) return;

        // Check one more time if the ore is already being processed
        if (processedOres.Contains(item.gameObject))
            return;

        Transform slot = availableSlots[0];
        availableSlots.RemoveAt(0);

        // Store the item data we need before potentially destroying the object
        string itemId = item.itemData.itemId;
        string displayName = item.itemData.displayName;
        float smeltTime = item.itemData.smeltTime;

        // Create UI timer
        GameObject timerUI = Instantiate(timerPrefab, timerContainer);
        RectTransform timerRect = timerUI.GetComponent<RectTransform>();

        // Position the timer UI vertically based on active slots count
        float yOffset = activeSlots.Count * timerSpacing;
        timerRect.anchoredPosition = new Vector2(0, -yOffset);

        ForgeSlot forgeSlot = new ForgeSlot
        {
            itemId = itemId,
            displayName = displayName,
            slot = slot,
            timerUI = timerUI.GetComponent<TextMeshProUGUI>(),
            remainingTime = smeltTime
        };

        // Add to processed ores set
        processedOres.Add(item.gameObject);
        activeSlots.Add(forgeSlot);

        // Position the ore
        item.transform.position = slot.position;
        item.transform.rotation = slot.rotation;
        item.GetComponent<Rigidbody>().isKinematic = true;
        forgeSlot.oreObject = item.gameObject;

        forgeUIPanel.SetActive(true);
        StartCoroutine(SmeltingProcess(forgeSlot));
    }

    private System.Collections.IEnumerator SmeltingProcess(ForgeSlot slot)
    {
        while (slot.remainingTime > 0)
        {
            slot.remainingTime -= Time.deltaTime;
            slot.timerUI.text = $"{slot.displayName}: {slot.remainingTime:F1}s";
            yield return null;
        }

        // Smelting complete
        CompleteSmeltingProcess(slot);
    }

    private void CompleteSmeltingProcess(ForgeSlot slot)
    {
        // Find corresponding ingot data
        ItemData ingotData = itemDatabase.GetResultingItem(slot.itemId);
        if (ingotData != null)
        {
            // Spawn ingot
            GameObject ingot = Instantiate(ingotData.prefab, ingotSpawnPoint.position, ingotSpawnPoint.rotation);
            ingot.GetComponent<ItemIdentifier>().itemData = ingotData;
        }

        // Cleanup
        if (slot.oreObject != null)
        {
            processedOres.Remove(slot.oreObject);
            Destroy(slot.oreObject);
        }

        Destroy(slot.timerUI.gameObject);
        availableSlots.Add(slot.slot);
        activeSlots.Remove(slot);

        // Reposition remaining timer UIs
        UpdateTimerPositions();

        if (activeSlots.Count == 0)
        {
            forgeUIPanel.SetActive(false);
        }
    }

    private void UpdateTimerPositions()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            RectTransform timerRect = activeSlots[i].timerUI.GetComponent<RectTransform>();
            timerRect.anchoredPosition = new Vector2(0, -i * timerSpacing);
        }
    }
}

[System.Serializable]
public class ForgeSlot
{
    public string itemId;
    public string displayName;
    public GameObject oreObject;
    public Transform slot;
    public TextMeshProUGUI timerUI;
    public float remainingTime;
}
