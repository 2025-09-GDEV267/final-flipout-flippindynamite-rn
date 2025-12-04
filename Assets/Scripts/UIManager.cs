using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    private CardObject selectedCard = null;
    private GameObject Outline = null;

    [Header("Outline Settings")]
    public float scaleMultiplier = 1.10f;
    public Vector3 behindOffset = new Vector3(0f, 0f, 1f);
    [Tooltip("Sprite used for the white silhouette")]
    public Sprite whiteSprite;

    [Header("Selected Settings")]
    public float selectedScaleMultiplier = 1.25f;
    public Color darkenColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color normalColor = Color.white;

    [Header("UI Position References")]
    [Tooltip("Assign one element per player. Each element contains hand slots and score pile transform.")]
    public UIHolder[] playerHolders;

    [Tooltip("Location of the draw pile in the scene.")]
    public Transform drawPileTransform;

    [Header("Movement Settings")]
    public float moveDuration = 0.35f;               // This affects the movement tween speed
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // sorting order control
    public int frontOffset = 1000;
    public int outlineRelativeOffset = -1;

    private Dictionary<CardObject, int> originalSortingOrders = new Dictionary<CardObject, int>();
    private Dictionary<CardObject, string> originalSortingLayerNames = new Dictionary<CardObject, string>();

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        CardObject.OnHoverEnter += CreateOutline;
        CardObject.OnHoverExit += DestroyOutline;
        CardObject.onCardClicked += ToggleSelection;
    }

    private void OnDisable()
    {
        CardObject.OnHoverEnter -= CreateOutline;
        CardObject.OnHoverExit -= DestroyOutline;
        CardObject.onCardClicked -= ToggleSelection;
    }

    private void CreateOutline(CardObject card)
    {
        if (card == null) return;
        if (selectedCard != null) return;
        SpriteRenderer originalSR = card.GetComponent<SpriteRenderer>();
        if (originalSR == null)
        {
            Debug.LogWarning("CreateOutline: card missing SpriteRenderer");
            return;
        }

        if (originalSortingOrders.ContainsKey(card))
            return;

        originalSortingOrders[card] = originalSR.sortingOrder;
        originalSortingLayerNames[card] = originalSR.sortingLayerName;

        originalSR.sortingOrder = originalSortingOrders[card] + frontOffset;

        if (Outline != null)
        {
            Destroy(Outline);
            Outline = null;
        }

        Outline = new GameObject("Outline");
        Outline.transform.SetParent(card.transform, true);
        Outline.transform.position = card.transform.position + behindOffset;
        Outline.transform.localScale = card.transform.localScale * scaleMultiplier;

        SpriteRenderer copySR = Outline.AddComponent<SpriteRenderer>();

        if (whiteSprite != null)
            copySR.sprite = whiteSprite;
        else
        {
            copySR.sprite = originalSR.sprite;
            Debug.LogWarning("CardHoverUIManager: whiteSprite not assigned — using original sprite as fallback.");
        }

        copySR.color = Color.white;
        copySR.sortingLayerName = originalSR.sortingLayerName;
        copySR.sortingOrder = originalSR.sortingOrder + outlineRelativeOffset;

        foreach (var col in Outline.GetComponents<Collider2D>())
            col.enabled = false;
        
    }

    private void DestroyOutline(CardObject card)
    {
        if (card == null)
        {
            RestoreAllAndClear();
            return;
        }

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

        if (Outline != null)
        {
            Destroy(Outline);
            Outline = null;
        }
    }

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

        if (Outline != null)
        {
            Destroy(Outline);
            Outline = null;
        }
    }

    private void ToggleSelection(CardObject card)
    {
        if (card == null) return;

        // Clicking the currently selected card unselects it
        if (selectedCard == card)
        {
            ClearSelection();
            return;
        }

    // Selecting a new card
    SetSelectedCard(card);
    }

    private void SetSelectedCard(CardObject card)
    {
        selectedCard = card;

        //Scale selected card up
        SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            card.transform.localScale = Vector3.one * selectedScaleMultiplier;

            //Bring to front
            sr.sortingOrder = 5000; 
        }

        //Darken every OTHER card
        DarkenAllExcept(card);

        //Remove hover copy (hovering makes no sense while selected)
        RestoreAllAndClear();
    }

    private void DarkenAllExcept(CardObject keepLit)
    {
        var allCards = GameObject.FindGameObjectsWithTag("Card");

        foreach (var obj in allCards)
        {
            var c = obj.GetComponent<CardObject>();
            if (c == null) continue;

            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            if (c == keepLit)
                sr.color = normalColor;   // selected stays normal
            else
                sr.color = darkenColor;   // others darken
        }
    }

    private void ClearSelection()
    {
        if (selectedCard != null)
        {
            // Restore scale
            selectedCard.transform.localScale = Vector3.one;

            // Restore sorting order
            SpriteRenderer sr = selectedCard.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 0;
        }

        selectedCard = null;

        // Reset all card colors
        RestoreAllCardColors();
    }

    private void RestoreAllCardColors()
    {
        var allCards = GameObject.FindGameObjectsWithTag("Card");

        foreach (var obj in allCards)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = normalColor;
        }
    }

    public void MoveCard(CardObject card, Transform from, Transform toHolder, int slotIndex = -1)
    {
        if (card == null || toHolder == null)
        {
            Debug.LogWarning("MoveCard called with missing references!");
            return;
        }

        // Default target position is the holder's position
        Vector3 targetPos = toHolder.position;

        if (slotIndex >= 0)
        {
            // Look through each UIPlayerHolder
            foreach (var holder in playerHolders)
            {
                // Check hand slots
                if (holder.slots != null && slotIndex < holder.slots.Length && holder.slots[slotIndex] == toHolder)
                {
                    targetPos = holder.slots[slotIndex].position;
                    break;
                }

                // Check player hand anchors
                if (holder.playerHandHolders != null && slotIndex < holder.playerHandHolders.Length && holder.playerHandHolders[slotIndex] == toHolder)
                {
                    targetPos = holder.playerHandHolders[slotIndex].position;
                    break;
                }

                // Check score slots
                if (holder.playerScoreHolders != null && slotIndex < holder.playerScoreHolders.Length && holder.playerScoreHolders[slotIndex] == toHolder)
                {
                    targetPos = holder.playerScoreHolders[slotIndex].position;
                    break;
                }
            }

            // Check draw pile
            foreach (var holder in playerHolders)
            {
                if (holder.drawPileHolder != null && holder.drawPileHolder == toHolder)
                {
                    targetPos = holder.drawPileHolder.position;
                    break;
                }
            }
        }

        // Start the movement animation
        StartCoroutine(AnimateCardMovement(card, targetPos));
    }

    private IEnumerator AnimateCardMovement(CardObject card, Vector3 targetPos)
    {
        if (card == null)
            yield break;

        Transform t = card.transform;
        Vector3 start = t.position;
        float time = 0f;

        while (time < moveDuration)
        {
            float p = time / moveDuration;
            float curve = moveCurve.Evaluate(p);

            t.position = Vector3.Lerp(start, targetPos, curve);

            time += Time.deltaTime;
            yield return null; // correct Unity coroutine yield
        }

        t.position = targetPos; // ensure final exact position
    }
}

[System.Serializable]
public class UIHolder
{
    [Header("Hand slots")]
    [Tooltip("Positions for cards in the player's hand (0–5 slots).")]
    public Transform[] slots;               // Individual slots for each card in hand

    [Header("Player Anchors")]
    [Tooltip("Empty GameObjects representing player hand and score areas.")]
    public Transform[] playerHandHolders;      // Anchor objects for the player's hand
    public Transform[] playerScoreHolders;     // Anchor objects for the player's score pile

    [Header("Draw Pile Anchor")]
    [Tooltip("Anchor for the draw pile for this player.")]
    public Transform drawPileHolder;           // Anchor for draw pile UI

    // Note: Avoid referencing any objects that contain UIManager itself
    // or that could indirectly reference back to UIManager to prevent serialization cycles.
}