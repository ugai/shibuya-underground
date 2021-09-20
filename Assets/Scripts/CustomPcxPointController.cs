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


    [RangeReactiveProperty(0.0f, 1.0f)]
    public FloatReactiveProperty audioSignal = new FloatReactiveProperty(0.0f);

    public Vector3ReactiveProperty effectTargetPositionBottom = new Vector3ReactiveProperty(Vector3.zero);
    public Vector3ReactiveProperty effectTargetPositionLetterH = new Vector3ReactiveProperty(Vector3.zero);
    public Vector3ReactiveProperty effectTargetPositionLetterE = new Vector3ReactiveProperty(Vector3.zero);
    public Vector3ReactiveProperty effectTargetPositionLetterN = new Vector3ReactiveProperty(Vector3.zero);

    public Transform effectTargetBottom;
    public Transform effectTargetLetterH;
    public Transform effectTargetLetterE;
    public Transform effectTargetLetterN;

    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;

        material.SetInteger("_ScreenWidth", Screen.width);
        material.SetInteger("_ScreenHeight", Screen.height);

        {
            var id = Shader.PropertyToID("_PointSize");
            pointSize.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_Intensity");
            intensity.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_NoiseStrength");
            noiseStrength.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_NoiseScale");
            noiseScale.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_NoiseSpeed");
            noiseSpeed.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_GridStrength");
            gridStrength.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_GridScale");
            gridScale.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_GridSpeed");
            gridSpeed.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_GridHueShift");
            gridHueShift.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_AudioSignal");
            audioSignal.Subscribe(v => material.SetFloat(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_EffectTargetPositionBottom");
            effectTargetPositionBottom.Subscribe(v => material.SetVector(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_EffectTargetPositionLetterH");
            effectTargetPositionLetterH.Subscribe(v => material.SetVector(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_EffectTargetPositionLetterE");
            effectTargetPositionLetterE.Subscribe(v => material.SetVector(id, v)).AddTo(this);
        }
        {
            var id = Shader.PropertyToID("_EffectTargetPositionLetterN");
            effectTargetPositionLetterN.Subscribe(v => material.SetVector(id, v)).AddTo(this);
        }
    }

    private void Update()
    {
        effectTargetPositionBottom.Value = effectTargetBottom.transform.position;
        effectTargetPositionLetterH.Value = effectTargetLetterH.transform.position;
        effectTargetPositionLetterE.Value = effectTargetLetterE.transform.position;
        effectTargetPositionLetterN.Value = effectTargetLetterN.transform.position;
    }
}
