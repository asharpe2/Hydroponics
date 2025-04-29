using UnityEngine;

public class DialController : MonoBehaviour
{
    [Header("Dial Sweep (degrees)")]
    [SerializeField] private float minAngle = -135f;
    [SerializeField] private float maxAngle = +135f;

    [Header("Value Output")]
    [SerializeField] private MonoBehaviour targetComponent = null;
    private IAdjustableResource targetResource;

    private bool isDragging;
    private float startMouseAngle;  // angle of the mouse at the moment we clicked
    private float startDialAngle;   // dial’s Z rotation at the moment we clicked

    void Awake()
    {
        if (targetComponent is IAdjustableResource rsrc)
            targetResource = rsrc;
        else
            Debug.LogError($"[{name}] targetComponent must implement IAdjustableResource");
    }

    void OnMouseDown()
    {
        isDragging = true;

        // compute the world-space angle of the mouse relative to dial center
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float raw = Mathf.Atan2(mw.y - transform.position.y,
                               mw.x - transform.position.x) * Mathf.Rad2Deg;

        startMouseAngle = raw;

        // grab the dial’s current Z angle and normalize to –180…+180
        float z = transform.rotation.eulerAngles.z;
        if (z > 180f) z -= 360f;
        startDialAngle = z;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging || targetResource == null)
            return;

        // current mouse angle
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float rawDeg = Mathf.Atan2(mw.y - transform.position.y,
                                   mw.x - transform.position.x) * Mathf.Rad2Deg;

        // how much the mouse has moved since OnMouseDown
        float delta = Mathf.DeltaAngle(startMouseAngle, rawDeg);

        // apply that delta to the dial’s starting angle
        float angle = startDialAngle + delta;

        // clamp into your allowed sweep
        float clamped = Mathf.Clamp(angle, minAngle, maxAngle);

        // rotate the knob
        transform.rotation = Quaternion.Euler(0, 0, clamped);

        // map into [0…1] and send to the resource
        float t = Mathf.InverseLerp(minAngle, maxAngle, clamped);
        targetResource.ApplyDialValue(t);
    }
}
