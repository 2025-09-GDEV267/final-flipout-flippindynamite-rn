using System.Collections.Generic;
using UnityEngine;

public enum UIState
{
SelectionPending,
NothingSelected,
};

public class CardHoverUIManager : MonoBehaviour
{
    private GameObject hoverCopy = null;

    [Header("Hover Settings")]
    public float scaleMultiplier = 1.10f;
    public Vector3 behindOffset = new Vector3(0f, 0f, 1f);
    [Tooltip("Sprite used for the white silhouette")]
    public Sprite whiteSprite;

    [Header("Selected Settings")]
    public float selectedScaleMultiplier = 1.25f;
    public Color darkenColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color normalColor = Color.white;


    // How much to bump the hovered card's sortingOrder so it sits above everything.
    // Use a large number to avoid conflicts with other ordering logic.
    public int frontOffset = 1000;

    // Small negative offset so the outline sits directly behind the boosted card.
    public int outlineRelativeOffset = -1;

    // We store original sorting orders per-card so they can be restored precisely.
    private Dictionary<CardObject, int> originalSortingOrders = new Dictionary<CardObject, int>();
    private Dictionary<CardObject, string> originalSortingLayerNames = new Dictionary<CardObject, string>();

    private void OnEnable()
    {
        CardObject.OnHoverEnter += CreateHoverCopy;
        CardObject.OnHoverExit += DestroyHoverCopy;
    }

    private void OnDisable()
    {
        CardObject.OnHoverEnter -= CreateHoverCopy;
        CardObject.OnHoverExit -= DestroyHoverCopy;
    }

    private void CreateHoverCopy(CardObject card)
    {
        if (card == null) return;

        SpriteRenderer originalSR = card.GetComponent<SpriteRenderer>();
        if (originalSR == null)
        {
            Debug.LogWarning("CreateHoverCopy: card missing SpriteRenderer");
            return;
        }

        // If we are already hovering this card, do nothing
        if (originalSortingOrders.ContainsKey(card))
            return;

        // Store original order & layer
        originalSortingOrders[card] = originalSR.sortingOrder;
        originalSortingLayerNames[card] = originalSR.sortingLayerName;

        // Boost real card to front
        originalSR.sortingOrder = originalSortingOrders[card] + frontOffset;

        // Remove any previous hover copy (safety)
        if (hoverCopy != null)
        {
            Destroy(hoverCopy);
            hoverCopy = null;
        }

        // Create the hover copy
        hoverCopy = new GameObject("HoverCopy");
        hoverCopy.transform.SetParent(card.transform, true);
        hoverCopy.transform.position = card.transform.position + behindOffset;
        hoverCopy.transform.localScale = card.transform.localScale * scaleMultiplier;

        SpriteRenderer copySR = hoverCopy.AddComponent<SpriteRenderer>();

        // Use the white sprite provided in the inspector
        if (whiteSprite != null)
            copySR.sprite = whiteSprite;
        else
        {
            // fallback to white rectangle if sprite missing (safe fallback)
            copySR.sprite = originalSR.sprite;
            Debug.LogWarning("CardHoverUIManager: whiteSprite not assigned — using original sprite as fallback.");
        }

        // Visual settings
        copySR.color = Color.white;
        copySR.sortingLayerName = originalSR.sortingLayerName;
        // Outline should sit just behind the boosted original
        copySR.sortingOrder = originalSR.sortingOrder + outlineRelativeOffset;

        // Ensure the hover copy won't intercept raycasts / mouse events
        foreach (var col in hoverCopy.GetComponents<Collider2D>())
            col.enabled = false;

        // Also remove any automatically copied components that could interfere
        // (If you used Instantiate(original.gameObject) earlier this would be more important)
    }

    private void DestroyHoverCopy(CardObject card)
    {
        if (card == null)
        {
            // cleanup any lingering hover copy
            RestoreAllAndClear();
            return;
        }

        // Restore the card's original sorting order if we stored it
        if (originalSortingOrders.TryGetValue(card, out int originalOrder))
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = originalOrder;
                if (originalSortingLayerNames.TryGetValue(card, out string originalLayer))
                    sr.sortingLayerName = originalLayer;
            }

            originalSortingOrders.Remove(card);
            originalSortingLayerNames.Remove(card);
        }

        if (hoverCopy != null)
        {
            Destroy(hoverCopy);
            hoverCopy = null;
        }
    }

    // If something unexpected happens, this forces a full restore.
    private void RestoreAllAndClear()
    {
        foreach (var kv in originalSortingOrders)
        {
            var card = kv.Key;
            var originalOrder = kv.Value;
            if (card != null)
            {
                var sr = card.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = originalOrder;
            }
        }

        originalSortingOrders.Clear();
        originalSortingLayerNames.Clear();

        if (hoverCopy != null)
        {
            Destroy(hoverCopy);
            hoverCopy = null;
        }
    }
}
