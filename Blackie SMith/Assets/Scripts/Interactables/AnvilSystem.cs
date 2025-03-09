using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AnvilSystem : MonoBehaviour
{
    [Header("References")]
    public Transform handleSlot;
    public Transform[] ingotSlots;
    public Transform resultSpawnPoint;
    public GameObject craftingProgressUI;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI craftingInfoText;
    public ItemDatabase itemDatabase;

    [Header("Crafting Settings")]
    public int requiredHits = 5;
    public float maxHitInterval = 2f;
    public float snapThreshold = 0.5f;
    public float hammerCooldown = 1f;

    private ItemIdentifier placedHandle;
    private List<ItemIdentifier> placedIngots = new List<ItemIdentifier>();
    private float lastHitTime;
    private int currentHits;
    private bool isCrafting;
    private WeaponType currentWeaponType;
    private MaterialType currentMaterial;

    private void Start()
    {
        craftingProgressUI.SetActive(false);
    }

    public bool TrySnapItem(ItemIdentifier item, out Transform snapPoint)
    {
        snapPoint = null;

        if (item.itemData.itemType == ItemType.Handle && placedHandle == null)
        {
            float distance = Vector3.Distance(item.transform.position, handleSlot.position);
            if (distance < snapThreshold)
            {
                snapPoint = handleSlot;
                placedHandle = item;
                CheckCraftingConditions();
                return true;
            }
        }
        else if (item.itemData.itemType == ItemType.Ingot && placedIngots.Count < ingotSlots.Length)
        {
            // Check if the ingot material matches existing ingots
            if (placedIngots.Count > 0)
            {
                MaterialType existingMaterial = placedIngots[0].itemData.materialType;
                if (item.itemData.materialType != existingMaterial)
                {
                    Debug.Log("Cannot mix different materials!");
                    return false;
                }
            }

            Transform slot = ingotSlots[placedIngots.Count];
            float distance = Vector3.Distance(item.transform.position, slot.position);
            if (distance < snapThreshold)
            {
                snapPoint = slot;
                placedIngots.Add(item);
                CheckCraftingConditions();
                return true;
            }
        }

        return false;
    }

    private void CheckCraftingConditions()
    {
        if (placedHandle == null || placedIngots.Count == 0) return;

        // Determine what can be crafted based on number of ingots
        WeaponType weaponType = GetWeaponTypeFromIngotCount(placedIngots.Count);
        if (weaponType != WeaponType.None)
        {
            currentWeaponType = weaponType;
            currentMaterial = placedIngots[0].itemData.materialType;
            StartCrafting();
            UpdateProgress();
        }
    }

    private WeaponType GetWeaponTypeFromIngotCount(int ingotCount)
    {
        switch (ingotCount)
        {
            case 1: return WeaponType.Dagger;
            case 2: return WeaponType.Sword;
            case 3: return WeaponType.Longsword;
            default: return WeaponType.None;
        }
    }

    private void StartCrafting()
    {
        isCrafting = true;
        currentHits = 0;
        lastHitTime = Time.time;
        craftingProgressUI.SetActive(true);
        UpdateProgress();
    }

    public void OnHammerStrike(Vector3 strikePoint)
    {
        if (!isCrafting)
        {
            Debug.Log("Strike ignored: Not in crafting mode.");
            return;
        }

        float timeSinceLastHit = Time.time - lastHitTime;

        // First, check if the player is hitting too fast
        if (timeSinceLastHit < hammerCooldown)
        {
            Debug.Log("Hammer strike too soon! Wait before hitting again.");
            return;
        }

        // Now check if enough time has passed that the crafting should reset
        if (currentHits > 0 && timeSinceLastHit > maxHitInterval)
        {
            Debug.Log("Crafting reset due to timeout.");
            ResetCrafting();
            return;
        }

        // ✅ Only update lastHitTime after a valid strike
        lastHitTime = Time.time;

        currentHits++;
        Debug.Log($"Hammer Strike Registered! Total Hits: {currentHits}/{requiredHits}");

        UpdateProgress();

        if (currentHits >= requiredHits)
        {
            Debug.Log("Crafting Complete!");
            CompleteCrafting();
        }
    }

    private void UpdateProgress()
    {
        progressText.text = $"Progress: {currentHits}/{requiredHits}";

        if (isCrafting && craftingInfoText != null)
        {
            string materialName = currentMaterial.ToString().ToLower();
            string weaponName = currentWeaponType.ToString().ToLower();
            craftingInfoText.text = $"You are currently crafting {materialName} {weaponName}";
        }
    }

    private void CompleteCrafting()
    {
        WeaponType weaponType = GetWeaponTypeFromIngotCount(placedIngots.Count);
        MaterialType material = placedIngots[0].itemData.materialType;

        // Get the appropriate weapon based on type and material
        ItemData weaponData = itemDatabase.GetWeaponData(weaponType, material);

        if (weaponData != null)
        {
            GameObject weapon = Instantiate(weaponData.prefab, resultSpawnPoint.position, resultSpawnPoint.rotation);
            weapon.GetComponent<ItemIdentifier>().itemData = weaponData;
        }

        // Cleanup
        Destroy(placedHandle.gameObject);
        foreach (var ingot in placedIngots)
        {
            Destroy(ingot.gameObject);
        }

        ResetCrafting();
    }

    private void ResetCrafting()
    {
        isCrafting = false;
        currentHits = 0;
        craftingProgressUI.SetActive(false);
        placedHandle = null;
        placedIngots.Clear();
        currentWeaponType = WeaponType.None;
        currentMaterial = MaterialType.None;
    }
}
