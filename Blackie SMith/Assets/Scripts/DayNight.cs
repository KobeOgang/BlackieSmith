using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayNight : MonoBehaviour
{
    [Header("References")]
    public Light directionalLight;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

    [Header("Time Settings")]
    [Tooltip("How many real-time seconds for a full day-night cycle")]
    public float dayDuration = 300f; // 5 minutes per day by default
    public float startingHour = 6f; // Start at 6 AM

    [Header("Day/Night Cycle")]
    [Tooltip("X axis rotation at noon")]
    public float noonLightRotation = 90f;
    [Range(0, 1)]
    public float currentTime;
    public int currentDay = 1;

    private float timeMultiplier;
    private bool is24HourFormat = false;

    private void Start()
    {
        // Calculate time multiplier (how fast the day progresses)
        timeMultiplier = 24f / dayDuration;

        // Set initial time
        currentTime = startingHour / 24f;

        // Update UI
        UpdateTimeDisplay();
        UpdateDayDisplay();
    }

    private void Update()
    {
        // Progress time
        currentTime += Time.deltaTime * timeMultiplier / 24f;

        // Check for day change
        if (currentTime >= 1f)
        {
            currentTime = 0f;
            currentDay++;
            UpdateDayDisplay();
        }

        // Update directional light
        UpdateLightRotation();

        // Update time display
        UpdateTimeDisplay();
    }

    private void UpdateLightRotation()
    {
        float sunRotation = Mathf.Lerp(-90, 270, currentTime);
        directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -30f, 0f);

        // Optional: Adjust light intensity based on time of day
        float intensity = Mathf.Cos((currentTime * 2f - 1f) * Mathf.PI) * 0.5f + 0.5f;
        directionalLight.intensity = Mathf.Lerp(0.1f, 1f, intensity);
    }

    private void UpdateTimeDisplay()
    {
        // Convert current time to hours and minutes
        float totalHours = currentTime * 24f;
        int hours = Mathf.FloorToInt(totalHours);
        int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

        // Format time string
        string timeString;
        if (is24HourFormat)
        {
            timeString = $"{hours:00}:{minutes:00}";
        }
        else
        {
            string period = hours >= 12 ? "PM" : "AM";
            int displayHours = hours % 12;
            if (displayHours == 0) displayHours = 12;
            timeString = $"{displayHours:00}:{minutes:00} {period}";
        }

        timeText.text = timeString;
    }

    private void UpdateDayDisplay()
    {
        dayText.text = $"Day {currentDay}";
    }

    // Public methods for external control
    public void SetDayDuration(float duration)
    {
        dayDuration = duration;
        timeMultiplier = 24f / dayDuration;
    }

    public float GetCurrentHour()
    {
        return currentTime * 24f;
    }

    public void ToggleTimeFormat()
    {
        is24HourFormat = !is24HourFormat;
        UpdateTimeDisplay();
    }
}
