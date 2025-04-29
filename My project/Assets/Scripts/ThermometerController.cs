using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class ThermometerController : MonoBehaviour, IAdjustableResource
{
    [Header("Temperature Range (°C)")]
    public float minTemp = 10f;
    public float maxTemp = 40f;

    [Header("Desired Temperature")]
    [Tooltip("Player is trying to match this by turning the dial")]
    public float desiredTemperature = 25f;

    [Header("Auto-change Settings")]
    [Tooltip("Seconds to wait before picking a new target")]
    public float minChangeInterval = 15f;
    public float maxChangeInterval = 30f;
    [Tooltip("How fast the needle moves (° per second)")]
    public float needleSpeed = 1.5f;

    [Header("UI Elements")]
    [SerializeField] private Image currentTempBar = null;
    [SerializeField] private RectTransform needle = null;
    [SerializeField] private TextMeshProUGUI desiredTempText = null;

    [HideInInspector] public float currentTemperature;

    private float _barHeight;
    private float _needleX;
    private float targetDesiredTemperature;

    void Awake()
    {
        // cache geometry
        if (currentTempBar != null)
            _barHeight = currentTempBar.rectTransform.rect.height;
        if (needle != null)
            _needleX = needle.anchoredPosition.x;

        // start both at the same value
        targetDesiredTemperature = desiredTemperature;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(TargetRoutine());
    }

    void Start()
    {
        currentTemperature = desiredTemperature;
        RedrawAll();
    }

    void Update()
    {
        // 1) drift the desiredTemperature toward the target at a fixed speed
        if (!Mathf.Approximately(desiredTemperature, targetDesiredTemperature))
        {
            desiredTemperature = Mathf.MoveTowards(
                desiredTemperature,
                targetDesiredTemperature,
                needleSpeed * Time.deltaTime
            );
        }

        // 2) always redraw UI (bar, needle, text)
        RedrawAll();
    }

    IEnumerator TargetRoutine()
    {
        while (true)
        {
            // wait a random interval
            float wait = Random.Range(minChangeInterval, maxChangeInterval);
            yield return new WaitForSeconds(wait);

            // choose a new random target
            targetDesiredTemperature = Random.Range(minTemp, maxTemp);
        }
    }

    public void ApplyDialValue(float t)
    {
        // dial → actual temp
        currentTemperature = Mathf.Lerp(maxTemp, minTemp, t);
    }

    private void RedrawAll()
    {
        UpdateBarFill();
        UpdateNeedlePosition();
        UpdateDesiredText();
    }

    private void UpdateBarFill()
    {
        if (currentTempBar == null) return;
        float norm = Mathf.InverseLerp(minTemp, maxTemp, currentTemperature);
        currentTempBar.fillAmount = norm;
    }

    private void UpdateNeedlePosition()
    {
        if (needle == null) return;
        float normD = Mathf.InverseLerp(minTemp, maxTemp, desiredTemperature);
        float localY = Mathf.Lerp(-_barHeight * 0.5f, _barHeight * 0.5f, normD);
        needle.anchoredPosition = new Vector2(_needleX, localY);
    }

    private void UpdateDesiredText()
    {
        if (desiredTempText != null)
            desiredTempText.text = $"Target: {desiredTemperature:F1}°C";
    }
}
