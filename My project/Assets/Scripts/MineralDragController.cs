using UnityEngine;
using UnityEngine.EventSystems;

public class MineralDragController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Drag-and-Drop Settings")]
    [Tooltip("Prefab of the fertilizer clump (UI Image)")]
    [SerializeField] private GameObject clumpPrefab;
    [Tooltip("How many mineral units to apply on drop")]
    [SerializeField] private float mineralsPerDrop = 0.5f;

    private GameObject clumpInstance;
    private RectTransform clumpRect;
    private Canvas rootCanvas;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
            Debug.LogError("MineralDragController must be under a Canvas");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clumpPrefab == null) return;
        clumpInstance = Instantiate(clumpPrefab, rootCanvas.transform);
        clumpRect = clumpInstance.GetComponent<RectTransform>();
        UpdateClumpPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (clumpRect != null)
            UpdateClumpPosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (clumpInstance == null) return;

        // Raycast through UI
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            var basin = r.gameObject.GetComponentInParent<BasinController>();
            if (basin != null)
            {
                basin.ApplyMinerals(mineralsPerDrop);
                break;
            }
        }

        Destroy(clumpInstance);
    }

    private void UpdateClumpPosition(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            rootCanvas.worldCamera,
            out localPos
        );
        clumpRect.anchoredPosition = localPos;
    }
}