using System.Linq;
using TMPro;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentWeatherText;
    [SerializeField] private TextMeshProUGUI prevWeatherText;
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI humidityText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("State Settings")]
    public string[] states = { "Sunny", "Cloudy", "Rainy", "Snowy", "Foggy", "Stormy" };
    public string currentState = "Cloudy";
    public string prevState = "None";
    public int currentIndex = 1;

    [Header("Environment Variables")]
    public float temperature = 25f;
    public float humidity = 50f;
    public float timer = 30.0f;
    public float countdownTimer = 30.0f;

    // Adjusted Matrix: Boosted Cloudy -> Rainy (0.30) to help exit the Sunny loop
    private float[][] transitionMatrix = new float[][]
    {
        new float[] { 0.55f, 0.30f, 0.08f, 0.01f, 0.03f, 0.03f }, // Sunny
        new float[] { 0.25f, 0.35f, 0.30f, 0.02f, 0.05f, 0.03f }, // Cloudy
        new float[] { 0.15f, 0.20f, 0.45f, 0.05f, 0.05f, 0.10f }, // Rainy
        new float[] { 0.02f, 0.36f, 0.05f, 0.45f, 0.10f, 0.02f }, // Snowy
        new float[] { 0.35f, 0.30f, 0.15f, 0.05f, 0.10f, 0.05f }, // Foggy
        new float[] { 0.05f, 0.15f, 0.45f, 0.05f, 0.10f, 0.20f }  // Stormy
    };

    void Update()
    {
        UpdateEnvironment(Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            WeatherTransition();
            timer = countdownTimer;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentWeatherText != null) currentWeatherText.text = "Current: " + currentState;
        if (prevWeatherText != null) prevWeatherText.text = "Previous: " + prevState;

        temperatureText.text = "Temp: " + temperature.ToString("f1") + "°C";
        humidityText.text = "Humidity: " + humidity.ToString("f1") + "%";

        if (timerText != null)
            timerText.text = "Next Shift: " + Mathf.CeilToInt(timer) + "s";
    }

    void UpdateEnvironment(float deltaTime)
    {
        // 1. Procedural Drift
        temperature += Random.Range(-0.05f, 0.05f);
        humidity += Random.Range(-0.1f, 0.1f);

        // 2. Weather Feedback
        if (currentState == "Sunny") { temperature += 0.01f; humidity -= 0.02f; }
        else if (currentState == "Rainy") { temperature -= 0.01f; humidity += 0.05f; }
        else if (currentState == "Snowy") { temperature -= 0.03f; }

        // 3. Natural Recovery: Pulls temp back toward 20°C so it doesn't stay frozen forever
        float baseTemp = 20.0f;
        float recoverySpeed = 0.005f;
        temperature = Mathf.MoveTowards(temperature, baseTemp, recoverySpeed);

        temperature = Mathf.Clamp(temperature, -15f, 35f);
        humidity = Mathf.Clamp(humidity, 10f, 90f);
    }

    void WeatherTransition()
    {
        float[] probability = (float[])transitionMatrix[currentIndex].Clone();

        // Apply Temperature Logic
        if (temperature > 30f)
        {
            probability[0] += 0.2f;
            probability[3] = 0.0f;
        }
        else if (temperature < 0f)
        {
            if (probability[3] < 0.3f) probability[3] = 0.3f; // Stronger Snow floor
            probability[3] += probability[2];
            probability[2] *= 0.1f; // Don't kill it entirely, just make it rare

            // LOGIC GUARD: Prevent direct jump from Snow to Sun when freezing
            probability[0] *= 0.1f;
        }

        // Apply Humidity Logic
        if (humidity > 70f) { probability[2] += 0.2f; probability[5] += 0.05f; } // Boost Storms too!
        else if (humidity < 25f) { probability[0] += 0.15f; }

        probability = NormalizeProbability(probability);

        float randomNum = Random.value;
        float cumulative = 0.0f;

        for (int i = 0; i < probability.Length; i++)
        {
            cumulative += probability[i];
            if (randomNum <= cumulative)
            {
                // Only update if the state is DIFFERENT
                if (states[i] != currentState)
                {
                    prevState = currentState; // Handover
                    currentState = states[i];
                    currentIndex = i;

                    Debug.Log("Weather Changed: " + prevState + " -> " + currentState);

                    // Call Person C's script here:
                    // visualsController.TransitionTo(currentState);
                }
                else
                {
                    Debug.Log("Weather Stayed: " + currentState);
                }
                break;
            }
        }
    }

    float[] NormalizeProbability(float[] prob)
    {
        float totalSum = prob.Sum();
        if (totalSum <= 0) return Enumerable.Repeat(1.0f / prob.Length, prob.Length).ToArray();
        for (int i = 0; i < prob.Length; i++) prob[i] /= totalSum;
        return prob;
    }
}