using UnityEngine;

public class StormyVisualController : BaseWeatherController
{
    [Header("Storm Specifics")]
    [SerializeField] private ParticleSystem lightningBoltParticles;

    public override void SetActive(bool active)
    {
        // 1. Let the base class handle the Volume and the primary Storm particles (Rain/Wind)
        base.SetActive(active);

        // 2. Handle the second Particle System (The Lightning Flashes)
        ToggleParticles(lightningBoltParticles, active);
    }
}