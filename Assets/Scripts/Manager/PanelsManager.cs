using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PanelsManager : Singleton<PanelsManager>
{
    //list of panels available to the manager
    public List<PanelModel> Panels;

    private List<PanelInstanceModel> _listInstances = new List<PanelInstanceModel>();

    public void ShowPanel(string panelid)
    {

        PanelModel panelModel = Panels.FirstOrDefault(predicate: panel => panel.PanelId == panelid);

        if (panelModel != null)
        {

            //create new instance of panel
            var newInstancePanel = Instantiate(panelModel.PanelPrefab, transform);

            newInstancePanel.transform.localPosition = Vector3.zero;

            //add new panel to queue
            _listInstances.Add(new PanelInstanceModel
            {

                PanelId = panelid,

                PanelInstance = newInstancePanel

            });

        }

        else 
        {

            Debug.LogWarning(message: $"bruh");
        
        }

    }

    public void HideLastPanel() 
    { 
    
        if(IsPanelShowing())
        {
            // Get the last shown panel
            var lastPanel = _listInstances[_listInstances.Count - 1];

            _listInstances.Remove(lastPanel);

            // Destroy that panel
            Destroy(lastPanel.PanelInstance);
        }
    
    }

    // returns if ANY panel is showing
    public bool IsPanelShowing() 
    {

        return GetAmountPanelsInQueue() > 0;
    
    }

    // returns # of panels we in queue
    public int GetAmountPanelsInQueue() 
    {

        return _listInstances.Count;
    
    }

}