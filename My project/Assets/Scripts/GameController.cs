// GameController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Instruction Screen")]
    [SerializeField] private GameObject instructionsPanel = null;
    [SerializeField] private Button startButton = null;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject deathPanel = null;
    [SerializeField] private Button restartButton = null;

    [Header("Dials to Enable/Disable")]
    [Tooltip("Hook in each of your DialController instances here")]
    [SerializeField] private DialController[] dials = null;

    void Awake()
    {
        // 1) Freeze everything until Start is pressed
        Time.timeScale = 0f;

        // 2) Show instructions, hide game-over UI
        instructionsPanel.SetActive(true);
        deathPanel.SetActive(false);

        // 3) Disable all dial scripts so they ignore input
        foreach (var d in dials)
            d.enabled = false;

        // 4) Hook up buttons
        startButton.onClick.AddListener(OnStartPressed);
        restartButton.onClick.AddListener(OnRestartPressed);
    }

    private void OnStartPressed()
    {
        // Hide instructions & unfreeze
        instructionsPanel.SetActive(false);
        Time.timeScale = 1f;

        // Re-enable the dials
        foreach (var d in dials)
            d.enabled = true;
    }

    private void OnRestartPressed()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Call this from BasinController.Die(...) when the game ends.
    /// </summary>
    public void ShowGameOverUI()
    {
        // Pause has already been done by BasinController
        deathPanel.SetActive(true);

        // also lock out the dials
        foreach (var d in dials)
            d.enabled = false;
    }
}
