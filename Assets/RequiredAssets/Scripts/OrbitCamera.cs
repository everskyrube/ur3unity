using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("What the camera orbits around. If null, tries to find a GameObject named 'base_link' or uses this camera's current forward point.")]
    public Transform target;

    [Tooltip("Offset from the target pivot (world units). Useful to aim at the middle of the arm rather than its base.")]
    public Vector3 targetOffset = new(0f, 0.3f, 0f);

    [Header("Starting view")]
    public float distance = 1.5f;
    public float yawDeg = 45f;
    public float pitchDeg = 25f;

    [Header("Limits")]
    public float minDistance = 0.3f;
    public float maxDistance = 10f;
    public float minPitchDeg = -80f;
    public float maxPitchDeg = 85f;

    [Header("Sensitivity")]
    public float orbitSpeed = 250f;   // deg per screen width per drag
    public float panSpeed = 1.2f;     // world units per screen width per drag
    public float zoomSpeed = 4f;      // distance fraction per scroll tick
    public float smoothTime = 0.08f;  // 0 for instant

    [Header("Input (legacy Input Manager)")]
    public int orbitMouseButton = 0; // left
    public int panMouseButton = 2;   // middle; right (1) also pans
    public KeyCode resetKey = KeyCode.F;

    Vector3 pivot;
    Vector3 pivotVel;
    float distVel, yawVel, pitchVel;
    float targetDistance, targetYaw, targetPitch;

    void Start()
    {
        if (target == null)
        {
            var found = GameObject.Find("base_link");
            if (found != null) target = found.transform;
        }

        pivot = (target != null ? target.position : transform.position + transform.forward * distance) + targetOffset;
        targetDistance = distance;
        targetYaw = yawDeg;
        targetPitch = pitchDeg;
        ApplyImmediately();
    }

    void LateUpdate()
    {
        if (target != null) pivot = target.position + targetOffset;

        HandleInput();

        distance = Mathf.SmoothDamp(distance, targetDistance, ref distVel, smoothTime);
        yawDeg   = Mathf.SmoothDamp(yawDeg,   targetYaw,      ref yawVel,   smoothTime);
        pitchDeg = Mathf.SmoothDamp(pitchDeg, targetPitch,    ref pitchVel, smoothTime);

        Apply();
    }

    void HandleInput()
    {
        // Orbit
        if (Input.GetMouseButton(orbitMouseButton) && !Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            targetYaw   += dx * orbitSpeed * Time.deltaTime;
            targetPitch -= dy * orbitSpeed * Time.deltaTime;
            targetPitch = Mathf.Clamp(targetPitch, minPitchDeg, maxPitchDeg);
        }

        // Pan with middle or right mouse
        if (Input.GetMouseButton(panMouseButton) || Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            float scale = panSpeed * distance * Time.deltaTime;
            Vector3 delta = (-right * dx - up * dy) * scale;
            if (target != null) targetOffset += Quaternion.Inverse(target.rotation) * delta;
            else pivot += delta;
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetDistance *= Mathf.Exp(-scroll * zoomSpeed);
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // Reset
        if (Input.GetKeyDown(resetKey))
        {
            targetYaw = 45f;
            targetPitch = 25f;
            targetDistance = Mathf.Clamp(1.5f, minDistance, maxDistance);
        }
    }

    void Apply()
    {
        Quaternion rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
        Vector3 pos = pivot - rot * Vector3.forward * distance;
        transform.SetPositionAndRotation(pos, rot);
    }

    void ApplyImmediately()
    {
        distance = targetDistance;
        yawDeg = targetYaw;
        pitchDeg = targetPitch;
        Apply();
    }
}
