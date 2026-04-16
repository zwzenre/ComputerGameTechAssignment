using UnityEngine;

public class SunnyVisualController : BaseWeatherController
{
    [SerializeField] private Material heatHazeMaterial;
    [SerializeField] private float activeHaze = 0.005f;
    [SerializeField] private float transitionSpeed = 0.0025f;

    private float currentHaze;
    private static readonly int HazePropID = Shader.PropertyToID("_HeatDistortion");

    protected override void Update()
    {
        base.Update(); // Handle Volume

        float target = isActive ? activeHaze : 0f;
        if (!Mathf.Approximately(currentHaze, target))
        {
            currentHaze = Mathf.MoveTowards(currentHaze, target, transitionSpeed * Time.deltaTime);
            if (heatHazeMaterial != null) heatHazeMaterial.SetFloat(HazePropID, currentHaze);
        }
    }
}