using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class LogoMotion : MonoBehaviour
{
    public AudioVisualize audioVisualize;

    [RangeReactiveProperty(0.0f, 30.0f)]
    public FloatReactiveProperty emission = new FloatReactiveProperty(0.0f);

    public ColorReactiveProperty emissionColor = new ColorReactiveProperty(Color.black);

    [Header("Color Scheme")]
    [ColorUsage(true, true)]
    public Color emissionColorBlack = Color.black;

    [ColorUsage(true, true)]
    public Color emissionColorWhite = Color.white;

    [ColorUsage(true, true)]
    public Color emissionColorPrimary = Color.magenta;

    private readonly List<Transform> transforms = new List<Transform>();
    private readonly List<Vector3> baseLocalPositions = new List<Vector3>();
    private Material sharedMaterial;

    private void Start()
    {
        var emissionPropId = Shader.PropertyToID("_Emission");
        var emissionColorPropId = Shader.PropertyToID("EmissionColor");

        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var tf = transform.GetChild(i);
            transforms.Add(tf);
            baseLocalPositions.Add(tf.localPosition);
        }

        var mr = transforms[0].GetComponent<MeshRenderer>();
        var mat = mr.sharedMaterial;
        emission.Subscribe(v => mat.SetFloat(emissionPropId, v)).AddTo(this);
        emissionColor.Subscribe(v => mat.SetColor(emissionColorPropId, v)).AddTo(this);

        // self
        transforms.Add(transform);
        baseLocalPositions.Add(transform.localPosition);
    }

    private void Update()
    {
        var av = audioVisualize.GetValue();

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].localPosition = baseLocalPositions[i] +
                Vector3.up * av * Mathf.PerlinNoise(i * 10.0f, Time.time);
        }

        emission.Value = 1.0f + av * av * 0.8f;
    }

    // For Signal Emitter
    public void SetEmissionColorBlack() { emissionColor.Value = emissionColorBlack; }
    public void SetEmissionColorWhite() { emissionColor.Value = emissionColorWhite; }
    public void SetEmissionColorPrimary() { emissionColor.Value = emissionColorPrimary; }
}
