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

    [Header("selected Settings")]
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

    [Header("Action Menu")]
    public GameObject actionMenuPrefab;
    public Vector3 actionMenuOffset = new Vector3(3f, 0f, 0f);

    public GameObject leftactionMenuPrefab;
    public Vector3 leftactionMenuOffset = new Vector3(-3f, 0f, 0f);
    private GameObject activeActionMenu;
    // sorting order control
    public int frontOffset = 1000;
    public int outlineRelativeOffset = 999;

    private Dictionary<CardObject, int> originalSortingOrders = new Dictionary<CardObject, int>();
    private Dictionary<CardObject, string> originalSortingLayerNames = new Dictionary<CardObject, string>();
    private Dictionary<CardObject, Vector3> originalScale = new Dictionary<CardObject, Vector3>();

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

    void Update()
    {
        if (selectedCard == null || activeActionMenu == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsClickOnSelectedCardOrMenu())
            {
                ClearSelection();
            }
        }
    }

    //This checks if the player clicked off a menu. If they did, the action menu closes.
    private bool IsClickOnSelectedCardOrMenu()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);

        // Check selected card
        Collider2D cardCol = selectedCard.GetComponent<Collider2D>();
        if (cardCol != null && cardCol.OverlapPoint(point))
            return true;

        // Check menu and its children
        Collider2D[] menuColliders = activeActionMenu.GetComponentsInChildren<Collider2D>();
        foreach (var col in menuColliders)
        {
            if (col.OverlapPoint(point))
                return true;
        }

        return false;
    }

    private void CreateOutline(CardObject card)
    {
        if (card == null) return;
        if (selectedCard != null) return;
        if (card.gameObject.tag == "invalid") return;

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

        if (originalSortingOrders.TryGetValue(card, out int originalOrder) && (card.gameObject.tag != "selected"))
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

    private void ToggleSelection(CardObject card)
    {
        if (card == null) return;

        // Clicking the currently selected card unselects it
        if (selectedCard == card)
        {
            ClearSelection();
            return;
        }
    
    if(GameManager.Instance.IsInHighlightMode != true)
    // Selecting a new card
    SetselectedCard(card);
    }

    public void ToggleSelectionExternal(CardObject card)
    {
        ToggleSelection(card);
        Debug.Log("Selection Toggle Activated");
    }


    private void SetselectedCard(CardObject card)
    {
        if (card.gameObject.tag == "invalid") return;
        selectedCard = card;
        if (!originalScale.ContainsKey(card))
            {
                originalScale[card] = card.transform.localScale;
            }    
        //Scale selected card up
        SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
 
            card.transform.localScale = Vector3.one * selectedScaleMultiplier;

            //Bring to front
            sr.sortingOrder = 5000; 
        }

        ShowActionMenu(card);

        //Apply tagging rules
        ApplySelectionTags(card);
        //Darken every OTHER card
        DarkenAllExcept(card);

        //Remove hover copy (hovering makes no sense while selected)
        RestoreAllAndClear();

    }

    public void ApplySelectionTags(CardObject selectedCard)
    {
        if (selectedCard == null)
            return;

        int selectedOwnerId = selectedCard.cardPOD.ownerPlayerID;

        GameStateClient gameState = GameStateClient.CurrentGameStateClient;
        if (gameState == null)
            return;

        int totalPlayers = GameStateClient.GetTotalPlayers();

        for (int playerNum = 0; playerNum < totalPlayers; playerNum++)
        {
            PlayerXClient player = gameState.GetPlayerByNumber(playerNum);
            if (player == null || player.hand == null)
                continue;

            bool isselectedPlayersHand = player.playerId == selectedOwnerId;

            for (int i = 0; i < player.hand.Length; i++)
            {
                CardPODClient pod = player.hand[i];
                if (pod == null || pod.cardObject == null)
                    continue;

                CardObject card = pod.cardObject;

                if (isselectedPlayersHand)
                {
                    // Same player's hand
                    if (card == selectedCard)
                    {
                        card.gameObject.tag = "selected";
                    }
                    else
                    {
                        // Do NOT override selected
                        if (card.gameObject.tag != "selected")
                            card.gameObject.tag = "invalid";
                    }
                }
                else
                {
                    // Other players' cards
                    card.gameObject.tag = "valid";
                }
            }
        }
    }

    private void DarkenAllExcept(CardObject keepLit)
    {
        var allCards = GameObject.FindGameObjectsWithTag("invalid");

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

    private void RestoreScale()
    {
        if (selectedCard == null)
            return;

        if (originalScale.TryGetValue(selectedCard, out Vector3 original))
        {
            selectedCard.transform.localScale = original;
        }
    }

    private void ClearSelection()
    {
        if (selectedCard != null)
        {
            RestoreScale();

            SpriteRenderer sr = selectedCard.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 0;
        }

        selectedCard = null;

        HideActionMenu();
        RestoreAllCardColors();
        ResetAllCardTags();
        
    if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearHighlightedCards();
            Debug.Log("Cleared Highlighted Cards");
        }
    }

    private void RestoreAllCardColors()
    {
        var allCards = GameObject.FindGameObjectsWithTag("invalid");

        foreach (var obj in allCards)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = normalColor;
        }

    }

    private void ResetAllCardTags()
    {
        var invalidCards = GameObject.FindGameObjectsWithTag("invalid");
        var selectedCards = GameObject.FindGameObjectsWithTag("selected");
        foreach (var obj in invalidCards)
            obj.tag = "valid";  // Default tag
        foreach (var obj in selectedCards)
            obj.tag = "valid";  // Default tag
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

    private void ShowActionMenu(CardObject card)
    {
        PlayerXClient player =
            GameStateClient.CurrentGameStateClient
                .GetPlayerByID(card.cardPOD.ownerPlayerID);

        if (player == null) return;
        HideActionMenu();
        int slotIndex = player.GetIndexOfCardByID(card.cardPOD.cardID);

        if (slotIndex == 4 || slotIndex == 5)
        {
        activeActionMenu = Instantiate(leftactionMenuPrefab);
        activeActionMenu.name = "CardActionMenu";

        // Parent to card so it follows movement
        activeActionMenu.transform.SetParent(card.transform, false);

        // Position it to the RIGHT of the card
        activeActionMenu.transform.localPosition = leftactionMenuOffset;

        // Sorting: above outline, below card
        SpriteRenderer[] renderers = activeActionMenu.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            r.sortingOrder = card.GetComponent<SpriteRenderer>().sortingOrder - 1;
        }
        CardActionMenu menu = activeActionMenu.GetComponent<CardActionMenu>();
        menu.Initialize(card);
        if (activeActionMenu == null)
        {
            Debug.LogWarning("DID NOT CREATE MENU");
        }
        else
        {
            Debug.Log("Menu Created!");
        }
        }
        else
        {        
        activeActionMenu = Instantiate(actionMenuPrefab);
        activeActionMenu.name = "CardActionMenu";

        // Parent to card so it follows movement
        activeActionMenu.transform.SetParent(card.transform, false);

        // Position it to the RIGHT of the card
        activeActionMenu.transform.localPosition = actionMenuOffset;

        // Sorting: above outline, below card
        SpriteRenderer[] renderers = activeActionMenu.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            r.sortingOrder = card.GetComponent<SpriteRenderer>().sortingOrder - 1;
        }
        CardActionMenu menu = activeActionMenu.GetComponent<CardActionMenu>();
        menu.Initialize(card);
        if (activeActionMenu == null)
        {
            Debug.LogWarning("DID NOT CREATE MENU");
        }
        else
        {
            Debug.Log("Menu Created!");
        }
        }
    }

    public void HideActionMenu()
    {
        if (activeActionMenu != null)
        {
            Destroy(activeActionMenu);
            activeActionMenu = null;
        }
    }
}


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