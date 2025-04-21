using UnityEngine;

/// <summary>
/// Tracks how much water you’re providing via the dial.
/// </summary>
public class TankController : MonoBehaviour, IAdjustableResource
{
    [Header("Dial → Water Flow (units/frame)")]
    [Tooltip("At dial=0")]
    public float minWaterOutput = 0.9f;
    [Tooltip("At dial=1")]
    public float maxWaterOutput = 1.1f;

    [HideInInspector]
    public float waterProvided;  // set by the dial each frame

    public void ApplyDialValue(float t)
    {
        // map knob[0…1] → tiny flow
        waterProvided = Mathf.Lerp(maxWaterOutput, minWaterOutput, t);
        Debug.Log($"[Tank] Providing {waterProvided:F6} water");
    }
}
