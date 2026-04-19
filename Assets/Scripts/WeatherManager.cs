using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { Sunny, Cloudy, Rainy, Snowy, Foggy, Stormy }

    [System.Serializable]
    public class WeatherState
    {
        public WeatherType type;
        public string name;
        public float minTemp, maxTemp, minHumidity, maxHumidity;
        public BaseWeatherController visualController;

        public WeatherState(WeatherType t, string n, float minT, float maxT, float minH, float maxH)
        {
            type = t;
            name = n;
            minTemp = minT;
            maxTemp = maxT;
            minHumidity = minH;
            maxHumidity = maxH;
        }
    }

    [Header("Simulation Data")]
    public List<WeatherState> weatherStates = new List<WeatherState>();
    public int currentIndex = 1; // Default weather Cloudy
    public int nextStateIndex = -1; // Show Next State
    public float temperature = 25f;
    public float humidity = 50f;
    public float countdownTimer = 30f;

    private float timer;

    private readonly float[][] transitionMatrix =
    {
        new float[] { 0.55f, 0.30f, 0.08f, 0.01f, 0.03f, 0.03f }, // Sunny
        new float[] { 0.25f, 0.35f, 0.30f, 0.02f, 0.05f, 0.03f }, // Cloudy
        new float[] { 0.15f, 0.20f, 0.45f, 0.05f, 0.05f, 0.10f }, // Rainy
        new float[] { 0.02f, 0.36f, 0.05f, 0.45f, 0.10f, 0.02f }, // Snowy
        new float[] { 0.35f, 0.30f, 0.15f, 0.05f, 0.10f, 0.05f }, // Foggy
        new float[] { 0.05f, 0.15f, 0.45f, 0.05f, 0.10f, 0.20f }  // Stormy
    };

    public WeatherState CurrentState => weatherStates[currentIndex];
    public WeatherState NextState => nextStateIndex >= 0 && nextStateIndex < weatherStates.Count ? 
        weatherStates[nextStateIndex] : null;
    public float RemainingTime => timer;

    private void Start()
    {
        currentIndex = Mathf.Clamp(currentIndex, 0, weatherStates.Count - 1);
        timer = countdownTimer;
        nextStateIndex = CalculateNextState();

        UpdateVisuals();
    }

    //private void InitializeStates()
    //{
    //    if (weatherStates.Count == 0)
    //    {
    //        weatherStates.Add(new WeatherState(WeatherType.Sunny, "Sunny", 20, 35, 10, 40));
    //        weatherStates.Add(new WeatherState(WeatherType.Cloudy, "Cloudy", 15, 25, 30, 60));
    //        weatherStates.Add(new WeatherState(WeatherType.Rainy, "Rainy", 5, 20, 70, 95));
    //        weatherStates.Add(new WeatherState(WeatherType.Snowy, "Snowy", -15, 0, 40, 80));
    //        weatherStates.Add(new WeatherState(WeatherType.Foggy, "Foggy", 0, 15, 80, 100));
    //        weatherStates.Add(new WeatherState(WeatherType.Stormy, "Stormy", 10, 25, 85, 100));
    //    }
    //}

    private void Update()
    {
        UpdateEnvironment(Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ApplyNextState();
            timer = countdownTimer;
            nextStateIndex = CalculateNextState();
        }

        UpdateVisuals();
    }

    public void UpdateCurrentStateRanges(float minTemp, float maxTemp, float minHumidity, float maxHumidity)
    {
        WeatherState current = weatherStates[currentIndex];
        current.minTemp = minTemp;
        current.maxTemp = maxTemp;
        current.minHumidity = minHumidity;
        current.maxHumidity = maxHumidity;

        Debug.Log($"Rules updated for {current.name}");
    }

    private void UpdateEnvironment(float deltaTime)
    {
        WeatherState current = weatherStates[currentIndex];

        temperature += Random.Range(-0.05f, 0.05f);
        humidity += Random.Range(-0.1f, 0.1f);

        if (temperature < current.minTemp) temperature += 0.01f;
        if (temperature > current.maxTemp) temperature -= 0.01f;

        if (humidity < current.minHumidity) humidity += 0.02f;
        if (humidity > current.maxHumidity) humidity -= 0.02f;

        temperature = Mathf.Clamp(temperature, -30f, 50f);
        humidity = Mathf.Clamp(humidity, 0f, 100f);
    }

    private int CalculateNextState()
    {
        float[] probability = (float[])transitionMatrix[currentIndex].Clone();

        if (temperature < 0f)
        {
            probability[3] += probability[2];
            probability[2] *= 0.1f;
        }
        
        if (temperature > 30f)
        {
            probability[0] += 0.2f; // Sunny
        }

        if (humidity > 80f)
        {
            probability[2] += 0.2f; // Rainy
            probability[5] += 0.1f; // Stormy
        }

        if (humidity < 30f)
        {
            probability[2] *= 0.5f;
        }

        probability = NormalizeProbability(probability);

        float randomNum = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < probability.Length; i++)
        {
            cumulative += probability[i];
            if (randomNum <= cumulative)
            {
                return i;
            }
        }

        return currentIndex;
    }

    private void ApplyNextState()
    {
        if (nextStateIndex < 0 || nextStateIndex >= weatherStates.Count)
            return;

        currentIndex = nextStateIndex;
        Debug.Log("Weather Changed to: " + weatherStates[currentIndex].name);
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < weatherStates.Count; i++)
        {
            if (weatherStates[i].visualController != null)
            {
                // Direct call is much faster than SendMessage or strings
                weatherStates[i].visualController.SetActive(i == currentIndex);
            }
        }
    }

    private float[] NormalizeProbability(float[] prob)
    {
        float totalSum = prob.Sum();

        if (totalSum <= 0f)
        {
            return Enumerable.Repeat(1f / prob.Length, prob.Length).ToArray();
        }

        for (int i = 0; i < prob.Length; i++)
        {
            prob[i] /= totalSum;
        }

        return prob;
    }

    public void SetNextState(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weatherStates.Count)
            return;

        nextStateIndex = newIndex;
        Debug.Log("Next weather set to: " + weatherStates[nextStateIndex].name);
    }
}
