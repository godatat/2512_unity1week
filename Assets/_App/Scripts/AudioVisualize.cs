/*
using UnityEngine;

public class AudioVisualize : MonoBehaviour
{
    [Header("Spectrum")]
    [Tooltip("FFT sample size (power of two). 512 is a good default.")]
    public int sampleSize = 512;
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    [Header("Bands")]
    [Tooltip("Number of frequency bands.")]
    public int bandCount = 8;

    [Header("Behavior")]
    [Tooltip("Overall amplitude multiplier.")]
    public float scaleMultiplier = 50f;

    [Range(0f, 0.99f)]
    [Tooltip("Smoothing factor. Higher = smoother but slower response.")]
    public float smoothing = 0.8f;

    [Header("Debug")]
    public bool debugLogs = false;
    public int logInterval = 30;

    // Internal state
    private float[] spectrum;
    private float[] bandValues;
    private float[] smoothValues;

    private int frameCounter = 0;
    private bool isRunning = true;

    // =========================
    // Unity lifecycle
    // =========================

    void Awake()
    {
        sampleSize = Mathf.ClosestPowerOfTwo(Mathf.Max(64, sampleSize));
        bandCount = Mathf.Max(1, bandCount);

        spectrum     = new float[sampleSize];
        bandValues   = new float[bandCount];
        smoothValues = new float[bandCount];

        if (debugLogs)
        {
            Debug.Log(
                $"AudioVisualize Awake | sampleSize={sampleSize}, bands={bandCount}, " +
                $"scaleMultiplier={scaleMultiplier}, smoothing={smoothing}"
            );
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // Get mixed/master spectrum (WebGL-safe)
        AudioListener.GetSpectrumData(spectrum, 0, fftWindow);

        // --- Build bands (log-like) ---
        int spectrumIndex = 0;

        for (int b = 0; b < bandCount; b++)
        {
            int sampleCount = 1 << b; // 2^b
            sampleCount = Mathf.Clamp(sampleCount, 1, sampleSize - spectrumIndex);

            float sum = 0f;
            int used = 0;

            for (int i = 0; i < sampleCount && spectrumIndex < sampleSize; i++, spectrumIndex++)
            {
                sum += spectrum[spectrumIndex];
                used++;
            }

            float v = (used > 0) ? (sum / used) : 0f;

            // ★ WebGL / WebAudio 対策（超重要）
            v = Mathf.Sqrt(v);

            bandValues[b] = v * scaleMultiplier;
        }

        // --- Smoothing ---
        for (int i = 0; i < bandCount; i++)
        {
            smoothValues[i] = Mathf.Lerp(
                smoothValues[i],
                bandValues[i],
                1f - smoothing
            );
        }

        // --- Debug ---
        if (debugLogs)
        {
            frameCounter++;
            if (frameCounter >= logInterval)
            {
                frameCounter = 0;

                float maxSpec = 0f;
                for (int i = 0; i < spectrum.Length; i++)
                    if (spectrum[i] > maxSpec) maxSpec = spectrum[i];

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("AudioVisualize | ");
                for (int i = 0; i < bandCount; i++)
                    sb.Append($"[{i}:{smoothValues[i]:F4}] ");
                sb.Append($"maxSpec={maxSpec:F6}");

                Debug.Log(sb.ToString());
            }
        }
    }

    // =========================
    // Public API
    // =========================

    /// <summary>Number of frequency bands.</summary>
    public int BandCount => smoothValues != null ? smoothValues.Length : 0;

    /// <summary>Get smoothed band value.</summary>
    public float GetBandValue(int index)
    {
        if (smoothValues == null) return 0f;
        if (index < 0 || index >= smoothValues.Length) return 0f;
        return smoothValues[index];
    }

    /// <summary>Get raw (unsmoothed) band value.</summary>
    public float GetRawBandValue(int index)
    {
        if (bandValues == null) return 0f;
        if (index < 0 || index >= bandValues.Length) return 0f;
        return bandValues[index];
    }

    /// <summary>Start FFT sampling.</summary>
    public void StartVisualize()
    {
        isRunning = true;
        if (debugLogs) Debug.Log("AudioVisualize StartVisualize");
    }

    /// <summary>Stop FFT sampling.</summary>
    public void StopVisualize()
    {
        isRunning = false;
        if (debugLogs) Debug.Log("AudioVisualize StopVisualize");
    }
}
*/