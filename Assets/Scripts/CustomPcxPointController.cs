using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

[RequireComponent(typeof(MeshRenderer))]
public class CustomPcxPointController : MonoBehaviour
{
    [RangeReactiveProperty(0.01f, 10.0f)]
    public FloatReactiveProperty pointSize = new FloatReactiveProperty(5.0f);


    [RangeReactiveProperty(0.0f, 50.0f)]
    public FloatReactiveProperty intensity = new FloatReactiveProperty(1.0f);


    [RangeReactiveProperty(0.0f, 10.0f)]
    public FloatReactiveProperty noiseStrength = new FloatReactiveProperty(1.0f);

    [RangeReactiveProperty(0.01f, 10.0f)]
    public FloatReactiveProperty noiseScale = new FloatReactiveProperty(1.0f);

    [RangeReactiveProperty(0.01f, 10.0f)]
    public FloatReactiveProperty noiseSpeed = new FloatReactiveProperty(1.0f);


    [RangeReactiveProperty(0.0f, 50.0f)]
    public FloatReactiveProperty gridStrength = new FloatReactiveProperty(1.0f);

    [RangeReactiveProperty(0.01f, 10.0f)]
    public FloatReactiveProperty gridScale = new FloatReactiveProperty(1.0f);

    [RangeReactiveProperty(0.01f, 10.0f)]
    public FloatReactiveProperty gridSpeed = new FloatReactiveProperty(1.0f);

    [RangeReactiveProperty(0.0f, 1.0f)]
    public FloatReactiveProperty gridHueShift = new FloatReactiveProperty(0.0f);

    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;

        material.SetInteger("_ScreenWidth", Screen.width);
        material.SetInteger("_ScreenHeight", Screen.height);

        var pointSizeId = Shader.PropertyToID("_PointSize");
        pointSize.Subscribe(v => material.SetFloat(pointSizeId, v)).AddTo(this);

        var intensityId = Shader.PropertyToID("_Intensity");
        intensity.Subscribe(v => material.SetFloat(intensityId, v)).AddTo(this);

        var noiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        noiseStrength.Subscribe(v => material.SetFloat(noiseStrengthId, v)).AddTo(this);

        var noiseScaleId = Shader.PropertyToID("_NoiseScale");
        noiseScale.Subscribe(v => material.SetFloat(noiseScaleId, v)).AddTo(this);

        var noiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        noiseSpeed.Subscribe(v => material.SetFloat(noiseSpeedId, v)).AddTo(this);

        var gridStrengthId = Shader.PropertyToID("_GridStrength");
        gridStrength.Subscribe(v => material.SetFloat(gridStrengthId, v)).AddTo(this);

        var gridScaleId = Shader.PropertyToID("_GridScale");
        gridScale.Subscribe(v => material.SetFloat(gridScaleId, v)).AddTo(this);

        var gridSpeedId = Shader.PropertyToID("_GridSpeed");
        gridSpeed.Subscribe(v => material.SetFloat(gridSpeedId, v)).AddTo(this);

        var gridHueShiftId = Shader.PropertyToID("_GridHueShift");
        gridHueShift.Subscribe(v => material.SetFloat(gridHueShiftId, v)).AddTo(this);
    }
}

