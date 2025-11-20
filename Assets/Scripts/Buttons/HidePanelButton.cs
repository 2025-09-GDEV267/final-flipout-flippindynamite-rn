using UnityEngine;

public class HidePanelButton : MonoBehaviour
{
    
    // Cache Panel Manager
    private PanelsManager _panelManager;

    public void Start()
    {
        // Cache this
        _panelManager = PanelsManager.Instance;

    }

    public void DoHidePanel()
    {

        _panelManager.HideLastPanel();

    }
}
