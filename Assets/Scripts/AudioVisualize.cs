using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class AudioVisualize : MonoBehaviour
{
    public AudioSource audioSource;
    public CustomPcxPointController customPcxPointController;
    public float freqMin = 0.0f;
    public float freqMax = 1000.0f;
    public float multiply = 1.0f;

    [Header("Preview")]
    public BoolReactiveProperty preview = new BoolReactiveProperty(true);
    public float previewScale = 1.0f;
    public Transform targetTransform;

    private float[] spectrum = new float[1024];
    private int step;

    private float cacheValue = -1.0f;
    private int cacheFrameCount = -1;

    private void Start()
    {
        step = AudioSettings.outputSampleRate / spectrum.Length;

        preview.Subscribe(b =>
        {
            if (targetTransform != null)
            {
                targetTransform.gameObject.SetActive(b);
            }
        }).AddTo(this);
    }

    private void Update()
    {
        if (preview.Value)
        {
            var total = GetValue();
            var ls = targetTransform.localScale;
            ls.y = total * previewScale;
            targetTransform.localScale = ls;
        }

        customPcxPointController.audioSignal.Value = GetValue();
    }

    public float GetValue()
    {
        if (cacheFrameCount == Time.frameCount)
        {
            return cacheValue;
        }

        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        var begin = Mathf.Clamp(Mathf.FloorToInt(freqMin / step), 0, spectrum.Length);
        var end = Mathf.Clamp(Mathf.FloorToInt(freqMax / step), 0, spectrum.Length);
        var total = 0f;
        for (var i = begin; i < end; i++)
        {
            total += spectrum[i];
        }

        total *= multiply;

        cacheFrameCount = Time.frameCount;
        cacheValue = total;

        return total;
    }
}
