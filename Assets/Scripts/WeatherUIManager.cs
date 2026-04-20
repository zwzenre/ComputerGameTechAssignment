using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeatherUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeatherManager weatherManager;

    [Header("UI References - Info")]
    [SerializeField] private TextMeshProUGUI currentWeatherText;
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI humidityText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI rangeDisplayText;
    [SerializeField] private TextMeshProUGUI nextWeatherText;

    [Header("UI References - Editing")]
    [SerializeField] private TMP_InputField minTempInput;
    [SerializeField] private TMP_InputField maxTempInput;
    [SerializeField] private TMP_InputField minHumidInput;
    [SerializeField] private TMP_InputField maxHumidInput;
    [SerializeField] private TMP_InputField timerInput;

    [Header("Next State Control")]
    [SerializeField] private TMP_Dropdown nextStateDropdown;

    private int lastStateIndex = -1;

    private void Start()
    {
        if (weatherManager == null)
        {
            Debug.LogError("WeatherUIManager: WeatherManager reference is missing.");
            enabled = false;
            return;
        }

        SetupDropdown();
        SyncUIWithCurrentState();
        UpdateDisplayUI();
    }

    private void Update()
    {
        if (weatherManager.currentIndex != lastStateIndex)
        {
            SyncUIWithCurrentState();
        }

        UpdateDisplayUI();
    }

    private void SetupDropdown()
    {
        if (nextStateDropdown == null) return;

        nextStateDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (var state in weatherManager.weatherStates)
        {
            options.Add(state.name);
        }

        nextStateDropdown.AddOptions(options);

        if (weatherManager.nextStateIndex >= 0)
            nextStateDropdown.value = weatherManager.nextStateIndex;
        else
            nextStateDropdown.value = weatherManager.currentIndex;

        nextStateDropdown.RefreshShownValue();
    }

    private void SyncUIWithCurrentState()
    {
        WeatherManager.WeatherState current = weatherManager.CurrentState;
        lastStateIndex = weatherManager.currentIndex;

        rangeDisplayText.text =
            $"Temp Range: {current.minTemp}°C - {current.maxTemp}°C\n" +
            $"Humidity Range: {current.minHumidity}% - {current.maxHumidity}%";

        minTempInput.text = current.minTemp.ToString();
        maxTempInput.text = current.maxTemp.ToString();
        minHumidInput.text = current.minHumidity.ToString();
        maxHumidInput.text = current.maxHumidity.ToString();
        timerInput.text = weatherManager.countdownTimer.ToString();

        if (nextStateDropdown != null && weatherManager.nextStateIndex >= 0)
        {
            nextStateDropdown.value = weatherManager.nextStateIndex;
            nextStateDropdown.RefreshShownValue();
        }
    }

    private void UpdateDisplayUI()
    {
        currentWeatherText.text = "Current State: " + weatherManager.CurrentState.name;
        temperatureText.text = "Temp: " + weatherManager.temperature.ToString("f1") + "°C";
        humidityText.text = "Humidity: " + weatherManager.humidity.ToString("f1") + "%";
        timerText.text = "Next Shift: " + Mathf.CeilToInt(weatherManager.RemainingTime) + "s";

        if (weatherManager.NextState != null)
            nextWeatherText.text = "Next State: " + weatherManager.NextState.name;
        else
            nextWeatherText.text = "Next State: -";
        
    }

    public void OnNextStateDropdownChanged()
    {
        if (nextStateDropdown == null) return;

        weatherManager.SetNextState(nextStateDropdown.value);
        UpdateDisplayUI();
    }

    public void UpdateStateRangesFromUI()
    {
        WeatherManager.WeatherState current = weatherManager.CurrentState;

        float minTemp = current.minTemp;
        float maxTemp = current.maxTemp;
        float minHumidity = current.minHumidity;
        float maxHumidity = current.maxHumidity;

        if (float.TryParse(minTempInput.text, out float parsedMinTemp))
            minTemp = parsedMinTemp;

        if (float.TryParse(maxTempInput.text, out float parsedMaxTemp))
            maxTemp = parsedMaxTemp;

        if (float.TryParse(minHumidInput.text, out float parsedMinHumidity))
            minHumidity = parsedMinHumidity;

        if (float.TryParse(maxHumidInput.text, out float parsedMaxHumidity))
            maxHumidity = parsedMaxHumidity;

        // Validation
        minTemp = Mathf.Clamp(minTemp, -15f, 35f);
        maxTemp = Mathf.Clamp(maxTemp, -15f, 35f);

        if (maxTemp < minTemp + 5f)
        {
            if (minTemp + 5f <= 35f)
            {
                maxTemp = minTemp + 5f;
            }
            else
            {
                minTemp = 30f;
                maxTemp = 35f;
            }
        }

        minHumidity = Mathf.Clamp(minHumidity, 0f, 100f);
        maxHumidity = Mathf.Clamp(maxHumidity, 0f, 100f);

        if (maxHumidity < minHumidity + 10f)
        {
            if (minHumidity + 10f <= 100f)
            {
                maxHumidity = minHumidity + 10f;
            }
            else
            {
                minHumidity = 90f;
                maxHumidity = 100f;
            }
        }

        weatherManager.UpdateCurrentStateRanges(minTemp, maxTemp, minHumidity, maxHumidity);
        SyncUIWithCurrentState();
    }

    public void UpdateTimerFromUI()
    {
        if (float.TryParse(timerInput.text, out float newTime))
        {
            newTime = Mathf.Clamp(newTime, 1f, 120f);
            weatherManager.countdownTimer = newTime;

            Debug.Log("Timer updated to: " + newTime);
        }
    }
}
