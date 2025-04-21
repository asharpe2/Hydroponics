// DialController.cs
using UnityEngine;

public class DialController : MonoBehaviour
{
    [Header("Dial Sweep (degrees)")]
    [Tooltip("Fully CCW")]
    [SerializeField] private float minAngle = -135f;
    [Tooltip("Fully CW")]
    [SerializeField] private float maxAngle = +135f;

    [Header("Value Output")]
    [Tooltip("0→1 will be passed to the target")]
    [SerializeField] private MonoBehaviour targetComponent = null;

    private IAdjustableResource targetResource;
    private bool isDragging;

    void Awake()
    {
        // Cache the IAdjustableResource interface
        if (targetComponent == null)
            Debug.LogError($"[{name}] targetComponent not assigned!");
        else if (!(targetComponent is IAdjustableResource))
            Debug.LogError($"[{name}] {targetComponent.GetType().Name} does not implement IAdjustableResource!");
        else
            targetResource = targetComponent as IAdjustableResource;
    }

    void OnMouseDown() => isDragging = true;
    void OnMouseUp() => isDragging = false;

    void Update()
    {
        if (!isDragging || targetResource == null) return;

        // 1. Mouse → world → direction
        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseW - transform.position;

        // 2. Angle from +X axis
        float rawDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 3. Clamp into dial arc
        float clamped = Mathf.Clamp(rawDeg, minAngle, maxAngle);

        // 4. Rotate the knob
        transform.rotation = Quaternion.Euler(0, 0, clamped);

        // 5. Map angle → 0…1
        float t = Mathf.InverseLerp(minAngle, maxAngle, clamped);

        // 6. Apply to your resource
        targetResource.ApplyDialValue(t);
    }
}
