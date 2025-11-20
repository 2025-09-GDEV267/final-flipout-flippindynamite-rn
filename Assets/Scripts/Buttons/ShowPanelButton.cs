using UnityEngine;

public class ShowPanelButton : MonoBehaviour
{
    
    // panel that we want to SHOW
    public string PanelId;

    // Cache Panel Manager
    private PanelsManager _panelManager;

    public void Start()
    {
        // Cache this
        _panelManager = PanelsManager.Instance;
       
    }

    public void DoShowPanel()
    {

        _panelManager.ShowPanel(PanelId);
    
    }
}
