using UnityEngine;

public class SnowVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private ParticleSystem snowParticleSystem;

    [Header("Snow Shader")]
    [SerializeField] private Material[] snowMaterial;
    [SerializeField] private string snowPropertyName = "SnowBalance";
    [SerializeField] private float snowyTargetValue = 5f;
    [SerializeField] private float nonSnowyTargetValue = 0f;
    [SerializeField] private float snowChangeSpeed = 0.25f;

    private float currentSnowValue = 0f;

    private void Start()
    {
        if (weatherManager == null)
        {
            Debug.LogError("SnowVisualController: WeatherManager reference is missing.");
            enabled = false;
            return;
        }

        bool isSnowy = weatherManager.CurrentState.name == "Snowy";
        currentSnowValue = isSnowy ? snowyTargetValue : nonSnowyTargetValue;

        ApplySnowValue(currentSnowValue);
        UpdateSnowParticles(isSnowy);
    }

    private void Update()
    {
        bool isSnowy = weatherManager.CurrentState.name == "Snowy";
        float targetSnowValue = isSnowy ? snowyTargetValue : nonSnowyTargetValue;

        currentSnowValue = Mathf.MoveTowards(
            currentSnowValue,
            targetSnowValue,
            snowChangeSpeed * Time.deltaTime
        );

        ApplySnowValue(currentSnowValue);
        UpdateSnowParticles(isSnowy);
    }

    private void ApplySnowValue(float value)
    {
        for (int i = 0; i < snowMaterial.Length; i++)
        {
            if (snowMaterial[i] == null) continue;

            snowMaterial[i].SetFloat(snowPropertyName, value);
        }
    }

    private void UpdateSnowParticles(bool isSnowy)
    {
        if (snowParticleSystem == null) return;

        if (isSnowy)
        {
            if (!snowParticleSystem.isPlaying)
            {
                snowParticleSystem.Play();
            }
        }
        else
        {
            if (snowParticleSystem.isPlaying)
            {
                snowParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
