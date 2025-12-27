using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    public Light2D light2D;
    public float baseIntensity = 1f;
    public float amplitude = 0.3f;
    public float speed = 5f;
    public float colorChangeAmount = 0.3f;
    public GameManager gameManager;
    public float maxSpeedMultiplier = 2f;

    private float timeOffset1 = 0f;
    private float timeOffset2 = 0f;
    private float timeOffset3 = 0f;
    private float timeOffsetColor1 = 0f;
    private float timeOffsetColor2 = 0f;
    private Color baseColor;

    void Start()
    {
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
        }

        baseIntensity = light2D.intensity;
        baseColor = light2D.color;
        timeOffset1 = Random.Range(0f, Mathf.PI * 2f);
        timeOffset2 = Random.Range(0f, Mathf.PI * 2f);
        timeOffset3 = Random.Range(0f, Mathf.PI * 2f);
        timeOffsetColor1 = Random.Range(0f, Mathf.PI * 2f);
        timeOffsetColor2 = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float ageAgeDoRatio = gameManager != null ? gameManager.AgeAgeDo / 100f : 0f;
        float currentSpeed = speed * (1f + ageAgeDoRatio * (maxSpeedMultiplier - 1f));

        float time = Time.time * currentSpeed;
        float wave1 = Mathf.Sin(time * 1f + timeOffset1) * amplitude * 0.5f;
        float wave2 = Mathf.Sin(time * 1.7f + timeOffset2) * amplitude * 0.3f;
        float wave3 = Mathf.Sin(time * 2.3f + timeOffset3) * amplitude * 0.2f;
        float intensity = baseIntensity + wave1 + wave2 + wave3;
        light2D.intensity = Mathf.Max(1f, intensity);

        float colorTime = Time.time * currentSpeed * 0.5f;
        float hueShift1 = Mathf.Sin(colorTime * 0.8f + timeOffsetColor1) * colorChangeAmount;
        float hueShift2 = Mathf.Sin(colorTime * 1.3f + timeOffsetColor2) * colorChangeAmount * 0.5f;
        float hueShift = hueShift1 + hueShift2;
        
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        h += hueShift;
        if (h >= 1f) h -= 1f;
        if (h < 0f) h += 1f;
        s = Mathf.Max(0.5f, s);
        light2D.color = Color.HSVToRGB(h, s, v);
    }
}
