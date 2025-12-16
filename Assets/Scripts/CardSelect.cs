using UnityEngine;
using UnityEngine.UI;

public class CardSelect : MonoBehaviour
{
    public Button cardClicker;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Button btn = cardClicker.GetComponent<Button>();
        btn.onClick.AddListener(CardClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CardClicked()
    {
        Debug.Log("I been touched vro,,,,");
    }
}
