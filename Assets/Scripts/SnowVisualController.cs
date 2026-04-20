using UnityEngine;

public class SnowVisualController : BaseWeatherController
{
    [SerializeField] private Material[] snowMaterials;
    [SerializeField] private float snowyTarget = 5f;
    [SerializeField] private float changeSpeed = 0.25f;

    private float currentSnow;
    private static readonly int SnowPropID = Shader.PropertyToID("_SnowBalance");

    protected override void Start()
    {
        base.Start();
        ResetSnow();
    }
    protected override void Update()
    {
        base.Update();

        float target = isActive ? snowyTarget : 0f;
        if (!Mathf.Approximately(currentSnow, target))
        {
            currentSnow = Mathf.MoveTowards(currentSnow, target, changeSpeed * Time.deltaTime);
            ApplySnow(currentSnow);
        }
    }

    private void OnDisable()
    {
        ResetSnow();
    }

    private void OnApplicationQuit()
    {
        ResetSnow();
    }

    private void ResetSnow()
    {
        currentSnow = 0f;
        ApplySnow(0f);
    }

    private void ApplySnow(float value)
    {
        foreach (var mat in snowMaterials)
        {
            if (mat != null)
            {
                mat.SetFloat(SnowPropID, value);
            }
        }
    }
}
