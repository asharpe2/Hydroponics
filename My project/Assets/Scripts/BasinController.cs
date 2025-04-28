using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BasinController : MonoBehaviour
{
    [SerializeField] private GameObject plant;

    [Header("External Provider")]
    [SerializeField] private TankController tankCtrl;
    [SerializeField] private ThermometerController thermoCtrl;

    [Header("Pool Settings")]
    public float initialWater = 1f;
    public float minWater = 0f;
    public float maxWater = 2.0f;
    public float minWaterWarning = 0.5f;
    public float maxWaterWarning = 1.5f;

    [Header("Temperature Check")]
    public float tempThreshold = 2f;
    public float tempThresholdWarning = 1f;

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
        // Grow the plant
        plant.transform.localScale += plantGrowthRate * Time.deltaTime;

        // 1) Ramp up consumption
        consumptionRate += consumptionGrowthRate * Time.deltaTime;

        // 2) Fill from the tank
        float added = tankCtrl.waterProvided * Time.deltaTime;
        waterPool += added;

        // 3) Subtract consumption
        float consumed = consumptionRate * Time.deltaTime;
        waterPool -= consumed;

        // 4) Compute temperature difference
        float tempDiff = thermoCtrl.currentTemperature - thermoCtrl.desiredTemperature;

        // 5) Collect any active warnings
        var warnings = new List<string>();

        // Water warnings
        if (waterPool < minWaterWarning)
        {
            warnings.Add("Underwatering!");
            if (waterPool < minWater)
                Die("Under-watered");
        }
        if (waterPool > maxWaterWarning)
        {
            warnings.Add("Overwatering!");
            if (waterPool > maxWater)
                Die("Over-watered");
        }

        // Temperature warnings
        if (tempDiff > tempThresholdWarning)
        {
            warnings.Add("Overheating!");
            if (tempDiff > tempThreshold)
                Die("Overheated");
        }
        if (tempDiff < -tempThresholdWarning)
        {
            warnings.Add("Freezing!");
            if (tempDiff < -tempThreshold)
                Die("Too-cold");
        }

        // 6) Show or hide the warning panel
        if (warnings.Count > 0)
        {
            warningText.text = string.Join("\n", warnings);
            warningText.gameObject.SetActive(true);
        }
        else
        {
            warningText.gameObject.SetActive(false);
        }

        // Optional: debug output
        Debug.Log($"[Basin] Pool={waterPool:F2} +{added:F2} -{consumed:F2}  TempDiff={tempDiff:F2}");
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

            case "Too-cold":
                deathText.text = "Exposure to cold slows metabolism and halts growth.";
                break;
                
            case "Overheated":
                deathText.text = "Excess heat denatures proteins and burns the plant.";
                break;

            default:
                deathText.text =
                    "The plant has died due to an unknown issue.";
                break;
        }
    }

}
