using UnityEngine;

public class CardSelect : MonoBehaviour
{
    public PlayerScript player;

    public void click()
    {
        player.Flip(gameObject.GetComponent<SpriteRenderer>());
    }
}
