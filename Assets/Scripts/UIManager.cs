using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    private CardObject selectedCard = null;
    private GameObject Outline = null;

    [Header("Outline Prefab")]
    public GameObject outlinePrefab;   // Assign in Inspector
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

    [Header("Card Spacing")]
    public float cardSpacing = 2.5f;

    [Header("Movement Settings")]
    public float moveDuration = 0.35f;               // This affects the movement tween speed
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Flip Animation Settings")]
    
    public float SquashDuration = 0.15f;               // This affects the movement tween speed (First Half)
    public AnimationCurve SquashCurve = AnimationCurve.EaseInOut(0,0,1,1);
    public float StretchDuration = 0.15f;               // This affects the stretch tween speed (Second Half)
    public AnimationCurve StretchCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // sorting order control
    public int frontOffset = 1000;
    public int outlineRelativeOffset = 999;

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
    }

    private void OnDisable()
    {
        CardObject.OnHoverEnter -= CreateOutline;
        CardObject.OnHoverExit -= DestroyOutline;
    }
    #region Outline
    private void CreateOutline(CardObject card)
    {
        if (card == null) return;
        if (selectedCard != null) return;
        if (card.gameObject.tag == "Invalid") return;
        if (card.gameObject.tag == "Selected") return;
        if (card.gameObject.tag == "Outline") return;

        SpriteRenderer originalSR = card.GetComponent<SpriteRenderer>();
        if (originalSR == null)
        {
            Debug.LogWarning("CreateOutline: card missing SpriteRenderer");
            return;
        }

        // Prevent duplicate outlines
        if (Outline != null)
            Destroy(Outline);

        // --- Instantiate outline prefab ---
        Outline = Instantiate(outlinePrefab);
        Outline.name = "Outline";
        Outline.tag = "Outline";

        // Parent it to card
        Outline.transform.SetParent(card.transform, true);

        // Position behind card
        Outline.transform.position = card.transform.position + behindOffset;

        // Scale slightly larger
        Outline.transform.localScale = card.transform.localScale * scaleMultiplier;



        // Configure sprite sorting
        SpriteRenderer outlineSR = Outline.GetComponent<SpriteRenderer>();
        if (outlineSR != null)
        {
            outlineSR.sortingLayerName = originalSR.sortingLayerName;
            outlineSR.sortingOrder = 999;
        }

        // Save original card sorting so we can restore later
        if (!originalSortingOrders.ContainsKey(card))
        {
            originalSortingOrders[card] = originalSR.sortingOrder;
            originalSortingLayerNames[card] = originalSR.sortingLayerName;
        }

        // Bring card to front
        originalSR.sortingOrder += frontOffset;
    }

    private void DestroyOutline(CardObject card)
    {
        if (card == null)
        {
            RestoreAllAndClear();
            return;
        }

        if (originalSortingOrders.TryGetValue(card, out int originalOrder) && (card.gameObject.tag != "Selected"))
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
#endregion
    private void RestoreAllAndClear()
    {
        foreach (var kv in originalSortingOrders)
        {
            var card = kv.Key;
            if (card == selectedCard)
            continue;
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
#region Selection
    public void ToggleSelection(CardObject card)
    {
        if (card == null) return;

        // 1st click -> select card
        if (selectedCard == null)
        {
            SetSelectedCard(card);
            return;
        }

        // Clicking the selected card -> unselect
        if (selectedCard == card)
        {
            ClearSelection();
            return;
        }

        // Second click: if card is "Valid", perform swap
        if (card.gameObject.tag == "Valid" && selectedCard.gameObject.tag == "Selected")
        {
            PerformCardSwap(selectedCard, card);
            return;
        }

    // Otherwise ignore
    Debug.Log("Invalid second selection.");


    // Selecting a new card
    SetSelectedCard(card);
    }

    private void SetSelectedCard(CardObject card)
    {
        if (card.gameObject.tag == "Invalid") return;
        selectedCard = card;

        //Scale selected card up
        SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            card.transform.localScale = Vector3.one * selectedScaleMultiplier;

            //Bring to front
            sr.sortingOrder = 5000; 
        }

        //Apply tagging rules
        ApplySelectionTags(card);
        //Darken every OTHER card
        DarkenAllExcept(card);

        //Remove hover copy (hovering makes no sense while selected)
        RestoreAllAndClear();

    }

    public void ApplySelectionTags(CardObject selectedCard)
    {
        if (selectedCard == null) return;

        int selectedOwner = selectedCard.cardPOD.ownerPlayerID;

        // Loop through all players
        for (int player = 0; player < GameManager.Instance.players.Length; player++)
        {
            PlayerXClient p = GameManager.Instance.players[player];
            if (p == null || p.hand == null) continue;

            for (int i = 0; i < p.hand.Length; i++)
            {
                CardPODClient pod = p.hand[i];
                if (pod == null || pod.cardObject == null) continue;

                CardObject card = pod.cardObject;

            if (player == selectedOwner)
            {
                // Do not change any card already marked as Selected
                if (card.gameObject.tag == "Selected")
                    continue;

                if (card == selectedCard)
                    card.gameObject.tag = "Selected";
                else
                    card.gameObject.tag = "Invalid";
            }
                else
                {
                    // Other players
                    card.gameObject.tag = "Valid";
                }
            }
        }
    }
    private void DarkenAllExcept(CardObject keepLit)
    {
        var allCards = GameObject.FindGameObjectsWithTag("Invalid");

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
        ResetAllCardTags();

        
    }

    private void RestoreAllCardColors()
    {
        var allCards = GameObject.FindGameObjectsWithTag("Invalid");

        foreach (var obj in allCards)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = normalColor;
        }

    }

    private void ResetAllCardTags()
    {
        var invalidCards = GameObject.FindGameObjectsWithTag("Invalid");
        var selectedCards = GameObject.FindGameObjectsWithTag("Selected");
        foreach (var obj in invalidCards)
            obj.tag = "Valid";  // Default tag
        foreach (var obj in selectedCards)
            obj.tag = "Valid";  // Default tag
    }
    #endregion
    #region Move Card
    public void MoveCard(CardObject card, Transform from, Transform toHolder, int slotIndex = -1)
    {
        if (card == null || toHolder == null)
            {
                Debug.LogWarning("MoveCard called with missing references!");
                return;
            }

        Vector3 targetPos = toHolder.position;

        // ---- FIND CORRECT DESTINATION POSITION (cross-player compatible) ----
        if (slotIndex >= 0)
        {
            foreach (var holder in playerHolders)
            {
                // Hand slots
                if (holder.slots != null &&
                        slotIndex < holder.slots.Length &&
                        holder.slots[slotIndex] == toHolder)
                {
                    targetPos = holder.slots[slotIndex].position;
                    goto BEGIN_MOVE;
                }

                // Player hand anchors
                if (holder.playerHandHolders != null &&
                    slotIndex < holder.playerHandHolders.Length &&
                    holder.playerHandHolders[slotIndex] == toHolder)
                {
                    targetPos = holder.playerHandHolders[slotIndex].position;
                    goto BEGIN_MOVE;
                }

                // Score pile slots
                if (holder.playerScoreHolders != null &&
                    slotIndex < holder.playerScoreHolders.Length &&
                    holder.playerScoreHolders[slotIndex] == toHolder)
                {
                    targetPos = holder.playerScoreHolders[slotIndex].position;
                    goto BEGIN_MOVE;
                }

                // Draw pile anchor
                if (holder.drawPileHolder == toHolder)
                {
                    targetPos = holder.drawPileHolder.position;
                    goto BEGIN_MOVE;
                }
            }
        }

        BEGIN_MOVE:

            //START MOVING ANIMATION 
            StartCoroutine(AnimateCardMovement(card, targetPos));

            //UPDATE CARD STATE + OWNER 
            CardPODClient pod = card.cardPOD;

            //Detect destination holder's playerIndex
            int newOwner = GetPlayerIndexFromHolder(toHolder);
            if (newOwner != -1)
                pod.ownerPlayerID = newOwner;

            //Detect card state based on destination holder
            pod.state = GetCardStateFromHolder(toHolder);

            //update tag logic
            card.gameObject.tag = "Valid";
    }

    private int FindCardIndexInPlayer(CardPODClient pod)
    {
        PlayerXClient player = GameManager.Instance.players[pod.ownerPlayerID];
        for (int i = 0; i < player.hand.Length; i++)
        {
           if (player.hand[i] == pod)
               return i;
         }
        return -1;
    }

    public int GetPlayerIndexFromHolder(Transform t)
    {
        for (int i = 0; i < playerHolders.Length; i++)
        {
            UIHolder h = playerHolders[i];

            if (h.holderRoot == t) return i;

            if (h.playerHandHolders != null)
            {
                foreach (var slot in h.playerHandHolders)
                    if (slot == t) return i;
            }

            if (h.playerScoreHolders != null)
            {
                foreach (var slot in h.playerScoreHolders)
                    if (slot == t) return i;
            }

            if (h.slots != null)
            {
                foreach (var slot in h.slots)
                    if (slot == t) return i;
            }

            if (h.drawPileHolder == t)
                return i;
        }
        return -1; // not owned by any player
    }

    public CardState GetCardStateFromHolder(Transform t)
    {
    foreach (var h in playerHolders)
        {
        if (h.drawPileHolder == t)
            return CardState.drawPile;

        if (h.playerHandHolders != null)
            if (System.Array.Exists(h.playerHandHolders, x => x == t))
                return CardState.playerHolder;

        if (h.playerScoreHolders != null)
            if (System.Array.Exists(h.playerScoreHolders, x => x == t))
                return CardState.scorePile;
        }

        return CardState.invalid;
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

    public Transform[] GenerateHandSlots(Transform parent, Vector3 localOffset, int numSlots = 6)
    {
        Transform[] slots = new Transform[numSlots];

        float half = (numSlots - 1) / 2f;

        for (int i = 0; i < numSlots; i++)
        {
            GameObject slot = new GameObject($"HandSlot_{i}");
            slot.transform.SetParent(parent);

            // Centered offset: positions go from -half to +half
            float xOffset = (i - half) * cardSpacing;

            slot.transform.localPosition = new Vector3(xOffset, 0f, -0.01f * i) + localOffset;

            slots[i] = slot.transform;
        }

        return slots;
    }
    private void PerformCardSwap(CardObject cardA, CardObject cardB)
    {
        int ownerA = cardA.cardPOD.ownerPlayerID;
        int ownerB = cardB.cardPOD.ownerPlayerID;

        int indexA = FindCardIndexInPlayer(cardA.cardPOD);
        int indexB = FindCardIndexInPlayer(cardB.cardPOD);

        if (indexA == -1 || indexB == -1)
        {
            Debug.LogError("Swap failed: could not find card indices.");
            return;
        }


        //Swap in backend data
        PlayerXClient pA = GameManager.Instance.players[ownerA];
        PlayerXClient pB = GameManager.Instance.players[ownerB];

        CardPODClient temp = pA.hand[indexA];
        pA.hand[indexA] = pB.hand[indexB];
        pB.hand[indexB] = temp;

        // Update POD ownership
        pA.hand[indexA].ownerPlayerID = ownerA;
        pB.hand[indexB].ownerPlayerID = ownerB;

        //Swap visually using MoveCard()
        Transform slotA = UIManager.Instance.playerHolders[ownerA].playerHandHolders[indexA];
        Transform slotB = UIManager.Instance.playerHolders[ownerB].playerHandHolders[indexB];

        MoveCard(cardA, cardA.transform, slotB, indexB);
        MoveCard(cardB, cardB.transform, slotA, indexA);

        //Reset selection/tags
        ClearSelection();

        Debug.Log($"Swapped card {cardA.cardPOD.cardID} with card {cardB.cardPOD.cardID}");
    }

    #endregion

    #region FlipCard


    private Vector3 Squashscale = new Vector3 (0f, 1f, 1f);
    private Vector3 Stretchscale = new Vector3 (1f, 1f, 1f);
    public IEnumerator Squashcard(CardObject card, Vector3 Scale,)
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

            t.position = Vector3.Lerp(start, scale, curve);

            time += Time.deltaTime;
            yield return null; // correct Unity coroutine yield
        }

        t.position = scale; // ensure final exact position
    }
    public IEnumerator stretchcard(CardObject card)
    {
        scale = new Vector3 (1f, 1f, 1f);
        if (card == null)
            yield break;

        Transform t = card.transform;
        Vector3 start = t.position;
        float time = 0f;

        while (time < moveDuration)
        {
            
            float p = time / moveDuration;
            float curve = moveCurve.Evaluate(p);

            t.position = Vector3.Lerp(start, scale, curve);

            time += Time.deltaTime;
            yield return null; // correct Unity coroutine yield
        }

        t.position = scale; // ensure final exact position
    }
}
#endregion
#region UIHolder Class
[System.Serializable]
public class UIHolder
{
    [Header("Root of this player's UI hierarchy")]
    public Transform holderRoot;

    [Header("Hand slots")]
    public Transform[] slots;

    [Header("Player Anchors")]
    public Transform[] playerHandHolders;
    public Transform[] playerScoreHolders;

    [Header("Draw Pile Anchor")]
    public Transform drawPileHolder;
}
#endregion