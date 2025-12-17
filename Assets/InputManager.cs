using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Mouse clicked");

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            CardSelect card;

            if (hit != null)
            {
                if (hit.TryGetComponent<CardSelect>(out card))
                {
                    Debug.Log("Clicked on: " + hit.gameObject.name);

                    card.click();
                }
                else
                {
                    Debug.Log("Nothing clicked");
                }
            }
        }
    }
}
