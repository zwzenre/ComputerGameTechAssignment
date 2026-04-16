using UnityEngine;
using UnityEngine.Rendering;

public abstract class BaseWeatherController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Volume weatherVolume;
    [SerializeField] protected float blendSpeed = 1.5f;
    [SerializeField] protected ParticleSystem primaryParticles;

    protected bool isActive;

    public virtual void SetActive(bool active)
    {
        isActive = active;
        // Optimization: Don't check every frame, only toggle particles when state changes
        ToggleParticles(primaryParticles, isActive);
    }

    protected virtual void Start()
    {
        if (weatherVolume != null) weatherVolume.weight = 0f;
        ToggleParticles(primaryParticles, false);
    }

    protected virtual void Update()
    {
        if (weatherVolume != null)
        {
            float target = isActive ? 1f : 0f;
            // Only update if we haven't reached the target
            if (!Mathf.Approximately(weatherVolume.weight, target))
            {
                weatherVolume.weight = Mathf.MoveTowards(weatherVolume.weight, target, blendSpeed * Time.deltaTime);
            }
        }
    }

    protected void ToggleParticles(ParticleSystem ps, bool play)
    {
        if (ps == null) return;
        if (play && !ps.isPlaying)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        else if (!play && ps.isPlaying)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}