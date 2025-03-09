using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomySystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;

    [Header("Economy Settings")]
    public float startingMoney = 1000f;
    public string currencySymbol = "coins";

    [Header("Debug Settings")]
    [Tooltip("For testing: Add money with + and - keys")]
    public bool enableDebugControls = true;
    public float debugMoneyIncrement = 100f;

    private float currentMoney;

    private void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyDisplay();
    }

    private void Update()
    {
        if (enableDebugControls)
        {
            // Debug controls for testing
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                AddMoney(debugMoneyIncrement);
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SpendMoney(debugMoneyIncrement);
            }
        }
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        UpdateMoneyDisplay();
    }

    public bool SpendMoney(float amount)
    {
        if (CanAfford(amount))
        {
            currentMoney -= amount;
            UpdateMoneyDisplay();
            return true;
        }
        return false;
    }

    public bool CanAfford(float amount)
    {
        return currentMoney >= amount;
    }

    public float GetCurrentMoney()
    {
        return currentMoney;
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currencySymbol}{currentMoney:N2}";
        }
    }

    // You can add more economy-related methods here as needed by your friend's AI customer system
    // For example:
    // - GetItemValue(ItemData item)
    // - CalculateProfit(float cost, float markup)
    // - ApplyTax(float amount)
    // etc.
}
