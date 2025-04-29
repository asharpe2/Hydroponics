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

    [Header("Minerals Pool")]
    [Tooltip("Starting minerals in the basin")]
    public float initialMinerals = 1f;
    [Tooltip("Warning threshold for minerals (low)")]
    public float minMineralsWarning = 0.5f;
    [Tooltip("Warning threshold for minerals (high)")]
    public float maxMineralsWarning = 1.5f;
    [Tooltip("Death threshold for minerals (low)")]
    public float minMinerals = 0f;
    [Tooltip("Death threshold for minerals (high)")]
    public float maxMinerals = 2f;
    [Tooltip("How many minerals the plant uses per second")]
    public float mineralsConsumptionRate = 0.05f;

    private float mineralsPool;

    [Header("Text/UI")]
    public TextMeshProUGUI warningText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;
    private bool isGameOver = false;

    private GameController gameController;

    void Start()
    {
        waterPool = initialWater;
        consumptionRate = baseConsumptionRate;
        mineralsPool = initialMinerals;
    }
    
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }

    void Update()
    {
        if (isGameOver)
            return;

        // 0) Grow the plant
        plant.transform.localScale += plantGrowthRate * Time.deltaTime;

        // 1) Win check: when scale.x >= 1, we stop and show the Victory panel
        if (plant.transform.localScale.x >= 1f)
        {
            Die("Victory");
            return;
        }

        // 2) Ramp up water consumption
        consumptionRate += consumptionGrowthRate * Time.deltaTime;

        // 3) Add water from the tank
        float added = tankCtrl.waterProvided * Time.deltaTime;
        waterPool += added;

        // 4) Subtract plant’s consumption
        float consumed = consumptionRate * Time.deltaTime;
        waterPool -= consumed;

        // 5) Subtract mineral drawdown
        mineralsPool -= mineralsConsumptionRate * Time.deltaTime;

        // 6) Temperature delta
        float tempDiff = thermoCtrl.currentTemperature - thermoCtrl.desiredTemperature;

        // 7) Build up any warnings
        var warnings = new List<string>();
        if (waterPool < minWaterWarning)
        {
            warnings.Add("Underwatering!");
            if (waterPool < minWater) Die("Under-watered");
        }
        if (waterPool > maxWaterWarning)
        {
            warnings.Add("Overwatering!");
            if (waterPool > maxWater) Die("Over-watered");
        }
        if (tempDiff > tempThresholdWarning)
        {
            warnings.Add("Overheating!");
            if (tempDiff > tempThreshold) Die("Overheated");
        }
        if (tempDiff < -tempThresholdWarning)
        {
            warnings.Add("Freezing!");
            if (tempDiff < -tempThreshold) Die("Too-cold");
        }
        if (mineralsPool < minMineralsWarning)
        {
            warnings.Add("Minerals low!");
            if (mineralsPool < minMinerals) Die("Under-minerals");
        }
        if (mineralsPool > maxMineralsWarning)
        {
            warnings.Add("Mineral Overload!");
            if (mineralsPool > maxMinerals) Die("Over-minerals");
        }

        // 8) Show/hide the warning text
        if (warnings.Count > 0)
        {
            warningText.text = string.Join("\n", warnings);
            warningText.gameObject.SetActive(!isGameOver);
        }
        else
        {
            warningText.gameObject.SetActive(false);
        }

        // (optional) debug
        Debug.Log($"[Basin] Water={waterPool:F2}+{added:F2}-{consumed:F2}, " +
                  $"Min={mineralsPool:F2}, TempDiff={tempDiff:F2}");
    }

    private void Die(string reason)
    {
        // mark it so Update() stops
        isGameOver = true;

        // hide warnings
        warningText.gameObject.SetActive(false);

        // pause the game
        Time.timeScale = 0f;

        // show the death/win UI
        deathPanel.SetActive(true);
        deathText.gameObject.SetActive(true);

        switch (reason)
        {
            case "Under-watered":
                deathText.text =
                    "Without enough water plants can't maintain turgor pressure, causing wilted leaves, drooping stems, and eventual death.";
                break;

            case "Over-watered":
                deathText.text =
                    "Too much water drowns the roots, blocks oxygen, and encourages root rot, ultimately killing the plant.";
                break;

            case "Too-cold":
                deathText.text =
                    "Cold temperatures slow a plant’s metabolism, stop growth, and can permanently damage cells.";
                break;

            case "Overheated":
                deathText.text =
                    "Extreme heat damages proteins and tissues in plants, leading to dehydration and death.";
                break;

            case "Under-minerals":
                deathText.text =
                    "Without enough minerals, plants can't perform essential processes like photosynthesis and growth, even if water and light are plentiful.";
                break;

            case "Over-minerals":
                deathText.text =
                    "Excess minerals create a toxic environment, disrupting water balance and damaging roots.";
                break;

            case "Victory":
                Time.timeScale = 0f;
                deathText.text =
                    "🎉 Congratulations! You’ve grown a fully mature plant!\n\n" +
                    "Hydroponics uses up to 90% less water than soil, " +
                    "allows precise nutrient control, and can grow fresh crops " +
                    "in zero-gravity environments, making it a key technology " +
                    "for long-term space missions.";
                break;

            default:
                deathText.text =
                    "The plant has died due to an unknown issue.";
                break;
        }
    }

    /// <summary>
    /// Called by MineralDragController on a successful drop.
    /// </summary>
    public void ApplyMinerals(float amount)
    {
        mineralsPool += amount;
        Debug.Log($"[Basin] Added {amount} minerals. Pool = {mineralsPool:F2}");
    }
}
