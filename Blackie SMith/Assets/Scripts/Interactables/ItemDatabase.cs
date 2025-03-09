using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item Database", menuName = "Blacksmith/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();
    private Dictionary<string, ItemData> itemLookup;
    private Dictionary<(WeaponType, MaterialType), ItemData> weaponLookup;

    private void OnEnable()
    {
        InitializeLookups();
    }

    private void InitializeLookups()
    {
        itemLookup = new Dictionary<string, ItemData>();
        weaponLookup = new Dictionary<(WeaponType, MaterialType), ItemData>();

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.itemId))
            {
                itemLookup[item.itemId] = item;
            }

            // Add weapons to the weapon lookup
            if (item.itemType == ItemType.Weapon)
            {
                weaponLookup[(item.weaponType, item.materialType)] = item;
            }
        }
    }

    public ItemData GetItem(string itemId)
    {
        if (itemLookup == null)
        {
            InitializeLookups();
        }

        itemLookup.TryGetValue(itemId, out ItemData item);
        return item;
    }

    public ItemData GetWeaponData(WeaponType weaponType, MaterialType material)
    {
        if (weaponLookup == null)
        {
            InitializeLookups();
        }

        weaponLookup.TryGetValue((weaponType, material), out ItemData weapon);
        return weapon;
    }

    public ItemData GetResultingItem(string inputItemId)
    {
        ItemData inputItem = GetItem(inputItemId);
        if (inputItem != null && !string.IsNullOrEmpty(inputItem.resultingItemId))
        {
            return GetItem(inputItem.resultingItemId);
        }
        return null;
    }
}

[System.Serializable]
public class CraftingRecipe
{
    public string inputItemId;
    public string outputItemId;
    public int inputCount;
}
