using UnityEngine;
using UnityEngine.Rendering;

public class WeatherVisual : MonoBehaviour
{
    [System.Serializable]
    public class WeatherVolumeEntry
    {
        public string weatherName;
        public Volume volume;
        public ParticleSystem particleEffect;
    }

    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private WeatherVolumeEntry[] weatherVolume;
    [SerializeField] private float blendSpeed = 2f;

    private void Start()
    {
        if (weatherManager == null)
        {
            Debug.LogError("WeatherVisualController: WeatherManager reference is missing.");
            enabled = false;
            return;
        }

        InitializeEffects();
    }

    private void Update()
    {
        string currentWeather = weatherManager.CurrentState.name;

        for (int i = 0; i < weatherVolume.Length; i++)
        {
            WeatherVolumeEntry entry = weatherVolume[i];
            if (entry == null) continue;

            bool isCurrentWeather = entry.weatherName == currentWeather;

            if (entry.volume != null)
            {
                float targetWeight = isCurrentWeather ? 1f : 0f;
                entry.volume.weight = Mathf.MoveTowards(
                    entry.volume.weight,
                    targetWeight,
                    blendSpeed * Time.deltaTime
                );
            }

            if (entry.particleEffect != null)
            {
                if (isCurrentWeather)
                {
                    if (!entry.particleEffect.isPlaying)
                        entry.particleEffect.Play();
                }
                else
                {
                    if (entry.particleEffect.isPlaying)
                        entry.particleEffect.Stop();
                }
            }
        }
    }

    private void InitializeEffects()
    {
        string currentWeather = weatherManager.CurrentState.name;

        for (int i = 0; i < weatherVolume.Length; i++)
        {
            WeatherVolumeEntry entry = weatherVolume[i];
            if (entry == null) continue;

            if (entry.volume != null)
            {
                entry.volume.weight = entry.weatherName == currentWeather ? 1f : 0f;
            }

            if (entry.particleEffect != null)
            {
                if (entry.weatherName == currentWeather)
                    entry.particleEffect.Play();
                else
                    entry.particleEffect.Stop();
            }
        }
    }
}
