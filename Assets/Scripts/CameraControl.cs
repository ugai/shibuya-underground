using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CameraControl : MonoBehaviour
{
    public float rotDegreePerSec = 360.0f;
    public float moveUnitPerSec = 10.0f;
    public float nextViewpointMoveSec = 3.0f;
    public float autoPlayIntervalSec = 3.0f;
    public CustomPcxPointController customPcxPointController;
    public AudioVisualize audioVisualize;
    public PlayableDirector playableDirector;

    private LocRot defaultLocRot;
    private Vector3 angle;
    private bool locked = true;
    private Coroutine viewPointTransitionCoroutine;

    private struct LocRot
    {
        public Vector3 loc;
        public Vector3 rot;

        public LocRot(float x, float y, float z, float pitch, float yaw, float roll)
        {
            loc = new Vector3(x, y, z);
            rot = new Vector3(pitch, yaw, roll);
        }

        public LocRot(Vector3 location, Vector3 rotation)
        {
            loc = location;
            rot = rotation;
        }

        public static LocRot From(Transform trans)
        {
            var loc = trans.localPosition;
            var rot = trans.localEulerAngles;
            return new LocRot(loc, rot);
        }

        public void SetTo(Transform trans)
        {
            trans.localPosition = loc;
            trans.localEulerAngles = rot;
        }
    }

    private int presetIndex = 0;
    private List<LocRot> presetLocRot = new List<LocRot>
    {
        new LocRot(46.0f, 2.0f, -6.0f, 0.0f, -12.0f, 0.0f),
        new LocRot(35.0f, -2.6f, 5.36f, 0.0f, 0.0f, 0.0f),
        
        new LocRot(47.0f, -8.0f, 15.0f, 1.5f, -144.0f, 0.0f),
        //new LocRot(46.5f, -7.9f, 16.0f, 1.6f, -156.0f, 90.0f),
        
        new LocRot(54.5f, -8.0f, 34.0f, 0.0f, -133.0f, 0.0f),
        new LocRot(38.0f, 33.0f, 22.0f, 90.0f, -90.0f, 15.0f),

        //new LocRot(54.0f, 3.5f, 120.0f, -0.8f, 178.0f, 0.0f),
        //new LocRot(84.0f, 5.5f, 150.0f, 3.0f, 109.0f, 0.0f),
        //new LocRot(92.0f, -8.2f, 204.0f, -1.0f, 34.0f, 0.0f),
        //new LocRot(33.0f, 6.0f, 61.0f, -1.0f, -62.0f, 0.0f),
        //new LocRot(-109.0f, 6.2f, 62.0f, 0.0f, -73.0f, 0.0f),
    };

    private void Awake()
    {
        defaultLocRot = LocRot.From(transform);

#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
    }

    private void Update()
    {
        // Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(0);
            return;
        }

        // Toggle perspective/ortho
        if (Input.GetKeyDown(KeyCode.V))
        {
            Camera.main.orthographic = !Camera.main.orthographic;
        }

        // Next viewpoint
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextViewPoint();
            return;
        }

        // Toggle play/pause timeline
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playableDirector != null)
            {
                if (playableDirector.state == PlayState.Paused)
                {
                    playableDirector.Play();
                }
                else
                {
                    playableDirector.Pause();
                }
            }
        }

        // Reset position and rotation
        if (Input.GetKey(KeyCode.R))
        {
            defaultLocRot.SetTo(transform);
            return;
        }

        // Update position and rotation
        if (Input.GetKeyDown(KeyCode.F))
        {
            locked = !locked;
        }
        if (!locked)
        {
            RotationUpdate();
            PositionUpdate();
        }
    }

    public void SlowMove()
    {
        StartCoroutine(SlowMoveCoroutine());
    }

    public IEnumerator SlowMoveCoroutine()
    {
        var srcPos = transform.localPosition;
        var srcRot = transform.localRotation;

        bool isHorizontalView = Mathf.Abs(srcRot.eulerAngles.x) <= 45.0f;

        float rot = 3.0f;
        var dstPos = srcPos + transform.right * 0.3f;
        var dstRot = srcRot * (isHorizontalView ?
            Quaternion.Euler(0.0f, -rot, 0.0f) :
            Quaternion.Euler(0.0f, 0.0f, rot));

        var cameraShakeDir = isHorizontalView ? transform.up : Vector3.up;

        var intensityOrig = customPcxPointController.intensity.Value;

        // transition
        float tPerSec = 1.0f / autoPlayIntervalSec;
        float t = 0.0f; // [0.0, 1.0]
        while (t < 1.0f)
        {
            var a = audioVisualize.GetValue();

            var tSmooth = Mathf.SmoothStep(0.0f, 1.0f, t);
            transform.localPosition = Vector3.Lerp(srcPos, dstPos, tSmooth) + cameraShakeDir * a * 0.2f;
            transform.localRotation = Quaternion.Lerp(srcRot, dstRot, tSmooth);

            customPcxPointController.intensity.Value = intensityOrig + a * 0.2f;
            yield return null;

            t = Mathf.Clamp01(t + (tPerSec * Time.deltaTime));
        }

        transform.localPosition = dstPos;
        transform.localRotation = dstRot;

        customPcxPointController.intensity.Value = intensityOrig;
    }

    public void NextViewPoint()
    {
        if (++presetIndex >= presetLocRot.Count)
        {
            presetIndex = 0;
        }

        if (viewPointTransitionCoroutine != null)
        {
            StopCoroutine(viewPointTransitionCoroutine);
        }
        viewPointTransitionCoroutine = StartCoroutine(NextViewPointCoroutine(presetLocRot[presetIndex]));
    }

    private IEnumerator NextViewPointCoroutine(LocRot dst)
    {
        var srcPos = transform.localPosition;
        var srcRot = transform.localRotation;

        var dstPos = dst.loc;
        var dstRot = Quaternion.Euler(dst.rot);

        // shader params
        var pointSizeOrig = customPcxPointController.pointSize.Value;
        var intensityOrig = customPcxPointController.intensity.Value;
        var noiseStrengthOrig = customPcxPointController.noiseStrength.Value;
        var gridStrengthOrig = customPcxPointController.gridStrength.Value;

        // transition
        float tPerSec = 1.0f / nextViewpointMoveSec;
        float t = 0.0f; // [0.0, 1.0]
        while (t < 1.0f)
        {
            var a = audioVisualize.GetValue();

            var tSmooth = Mathf.SmoothStep(0.0f, 1.0f, t);
            transform.localPosition = Vector3.Lerp(srcPos, dstPos, tSmooth) + transform.up * a * 0.2f;
            transform.localRotation = Quaternion.Lerp(srcRot, dstRot, tSmooth);

            var x = (t - 0.5f) * 2.0f; // [-1.0, 1.0]
            var v = 1.0f - Mathf.Abs(x); // [0.0, 1.0, 0.0]
            var p = Mathf.Clamp01(v * 4.0f);
            const float delay = 0.2f;
            var pDelay = Mathf.Clamp01((v - delay) / delay);
            customPcxPointController.pointSize.Value = pointSizeOrig - p * (pointSizeOrig - 0.6f);
            customPcxPointController.intensity.Value = intensityOrig - p * (intensityOrig - 0.2f);
            customPcxPointController.noiseStrength.Value = (a * a * a * a) * (v * v * tSmooth) * 3.0f;
            customPcxPointController.gridStrength.Value = (pDelay > 0.0f ? Mathf.Max(5.0f, pDelay * 30.0f) : 0.0f);
            yield return null;

            t = Mathf.Clamp01(t + (tPerSec * Time.deltaTime));
        }

        transform.localPosition = dstPos;
        transform.localRotation = dstRot;

        // restore shader params
        customPcxPointController.pointSize.Value = pointSizeOrig;
        customPcxPointController.intensity.Value = intensityOrig;
        customPcxPointController.noiseStrength.Value = noiseStrengthOrig;
        customPcxPointController.gridStrength.Value = gridStrengthOrig;
    }

    private void RotationUpdate()
    {
        var maxRotAmount = rotDegreePerSec * Time.deltaTime;
        angle.y += Input.GetAxis("Mouse X") * maxRotAmount; // yaw
        angle.x -= Input.GetAxis("Mouse Y") * maxRotAmount; // pitch
        transform.eulerAngles = angle;
    }

    private void PositionUpdate()
    {
        var moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveDirection += transform.right;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveDirection -= transform.right;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += transform.up;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection -= transform.up;
        }

        float mag = 1.0f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            mag = 2.0f;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            mag = 0.5f;
        }

        var moveAmount = moveUnitPerSec * mag * Time.deltaTime;

        transform.localPosition += moveDirection * moveAmount;
    }
}
