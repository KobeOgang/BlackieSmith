using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopUI;
    public Button backButton;

    [Header("Item Spawning")]
    public Transform spawnPoint;
    public ItemDatabase itemDatabase;

    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public float price;
    }
    public ShopItem[] shopItems;

    [Header("References")]
    public EconomySystem economySystem;
    public CrosshairManager crosshairManager;
    public PlayerCam cameraController;

    private bool isShopOpen = false;

    private void Start()
    {
        // Hide shop UI at start
        shopUI.SetActive(false);

        // Add listener to back button
        backButton.onClick.AddListener(CloseShop);
    }

    private void Update()
    {
        // Check for E key press
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Cast a ray to check if player is looking at the book
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!isShopOpen)
                    {
                        OpenShop();
                    }
                }
            }
        }
    }

    public void OpenShop()
    {
        isShopOpen = true;
        shopUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide crosshair
        if (crosshairManager != null)
        {
            crosshairManager.HideCrosshair();
        }

        // Lock camera movement
        if (cameraController != null)
        {
            cameraController.LockMovement();
        }
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show crosshair
        if (crosshairManager != null)
        {
            crosshairManager.ShowCrosshair();
        }

        // Unlock camera movement
        if (cameraController != null)
        {
            cameraController.UnlockMovement();
        }
    }

    // Call this method from UI button OnClick events
    public void TryBuyItem(string itemId)
    {
        // Find the item price
        ShopItem item = System.Array.Find(shopItems, x => x.itemId == itemId);
        if (item == null)
        {
            Debug.LogError($"Item with ID {itemId} not found in shop items!");
            return;
        }

        // Check if player has enough money
        if (economySystem.CanAfford(item.price))
        {
            // Get the item data
            ItemData itemData = itemDatabase.GetItem(itemId);
            if (itemData != null)
            {
                // Spawn the item
                GameObject spawnedItem = Instantiate(itemData.prefab, spawnPoint.position, spawnPoint.rotation);
                spawnedItem.GetComponent<ItemIdentifier>().itemData = itemData;

                // Deduct money
                economySystem.SpendMoney(item.price);
            }
            else
            {
                Debug.LogError($"Item with ID {itemId} not found in database!");
            }
        }
        else
        {
            Debug.Log("Not enough money!");
            // You could show a UI message here
        }
    }
}
