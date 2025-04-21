using UnityEngine;

public class BasinController : MonoBehaviour
{
    [SerializeField] private GameObject plant;

    [Header("External Provider")]
    [SerializeField] private TankController tankCtrl;

    [Header("Pool Settings")]
    public float initialWater = 1f;
    public float minWater = 0.3f;
    public float maxWater = 2.0f;

    [Header("Consumption")]
    [Tooltip("Base water use per second at start")]
    public float baseConsumptionRate = 0.1f;

    [Tooltip("How much per second the consumptionRate increases")]
    public float consumptionGrowthRate = 0.005f;

    public Vector3 plantGrowthRate = new Vector3(0.005f, 0.005f, 0.005f);

    private float waterPool;
    private float consumptionRate;

    void Start()
    {
        waterPool = initialWater;
        consumptionRate = baseConsumptionRate;
    }

    void Update()
    {
        plant.transform.localScale += plantGrowthRate * Time.deltaTime;

        // 1) Slowly ramp up the plant’s consumption rate
        consumptionRate += consumptionGrowthRate * Time.deltaTime;

        // 2) Add whatever water the tank is providing (units/sec -> units/frame)
        float added = tankCtrl.waterProvided * Time.deltaTime;
        waterPool += added;

        // 3) Subtract the (now higher) consumption
        float consumed = consumptionRate * Time.deltaTime;
        waterPool -= consumed;

        // 4) Check for under/over‑water
        if (waterPool < (minWater * 2))
        {
            warningText = "Underwatering!";
            if (waterPool < minWater)
                Die("Under‑watered!");
        }
        else if (waterPool < (maxWater / 2))
        {
            warningText = "Underwatering!";
            if (waterPool < minWater)
                Die("Under‑watered!");
        }
        else if (waterPool > maxWater)
            Die("Over‑watered!");

        // Debug
        Debug.Log(
            $"[Basin] Pool={waterPool:F2}  +{added:F2}  -{consumed:F2}  (rate={consumptionRate:F3})"
        );
    }

    private void Die(string reason)
    {
        Debug.Log($"Plant died: {reason}");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
