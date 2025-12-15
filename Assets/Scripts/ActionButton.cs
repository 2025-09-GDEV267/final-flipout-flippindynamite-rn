using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    [Header("Action this button performs")]
    public CardActionType actionType;

    private CardObject ownerCard;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    /// <summary>
    /// Called by CardActionMenu when the menu opens
    /// </summary>
    public void SetOwnerCard(CardObject card)
    {
        ownerCard = card;
    }

    private void OnClicked()
    {
        if (ownerCard == null)
        {
            Debug.LogError("ActionButton clicked but ownerCard is NULL.");
            return;
        }

        EmitActionRequest();
        UIManager.Instance.ToggleSelectionExternal(ownerCard);
    }

    private void EmitActionRequest()
    {
        UISignals.OnCardActionRequested?.Invoke(
            new CardActionRequest
            {
                actionType = actionType,
                sourceCard = ownerCard
            }
        );
    }
}
public static class UISignals
{
    public static System.Action<CardActionRequest> OnCardActionRequested;
}