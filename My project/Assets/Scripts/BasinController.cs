using UnityEngine;
using TMPro;

public class BasinController : MonoBehaviour
{
    [SerializeField] private GameObject plant;

    [Header("External Provider")]
    [SerializeField] private TankController tankCtrl;

    [Header("Pool Settings")]
    public float initialWater = 1f;
    public float minWater = 0f;
    public float maxWater = 2.0f;
    public float minWaterWarning = 0.5f;
    public float maxWaterWarning = 1.5f;

    [Header("Consumption")]
    public float baseConsumptionRate = 0.1f;
    public float consumptionGrowthRate = 0.005f;
    public Vector3 plantGrowthRate = new Vector3(0.005f, 0.005f, 0.005f);

    [Header("Water Pool")]
    private float waterPool;
    private float consumptionRate;

    [Header("Text/UI")]
    public TextMeshProUGUI warningText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;

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
        if (waterPool < minWaterWarning)
        {
            warningText.text = "Underwatering!";
            warningText.gameObject.SetActive(true);
            if (waterPool < minWater)
                Die("Under-watered");
        }
        else if (waterPool > maxWaterWarning)
        {
            warningText.text = "Overwatering!";
            warningText.gameObject.SetActive(true);
            if (waterPool > maxWater)
                Die("Over-watered");
        }
        else
        {
            warningText.gameObject.SetActive(false);
        }

        // Debug
        Debug.Log(
            $"[Basin] Pool={waterPool:F2}  +{added:F2}  -{consumed:F2}  (rate={consumptionRate:F3})"
        );
    }

    private void Die(string reason)
    {
        // pause
        Time.timeScale = 0f;

        // hide any ongoing warnings
        warningText.gameObject.SetActive(false);

        // show the death UI
        deathText.gameObject.SetActive(true);
        deathPanel.gameObject.SetActive(true);

        // pick the correct message based on why the plant died
        switch (reason)
        {
            case "Under-watered":
                deathText.text =
                    "Under-watering a plant can cause droopy leaves, saggy stems, and eventually death.";
                break;

            case "Over-watered":
                deathText.text =
                    "Over-watering can lead to root rot, poor oxygen uptake, and eventually kill the plant.";
                break;

            default:
                deathText.text =
                    "The plant has died due to an unknown issue.";
                break;
        }
    }

}
