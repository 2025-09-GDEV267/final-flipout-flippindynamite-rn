using UnityEngine;

public class CardActionMenu : MonoBehaviour
{
    public CardObject ownerCard;

    public void Initialize(CardObject card)
    {
        ownerCard = card;
    }

    public void OnActionClicked()
    {
      return;
    }
}