using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TestUI : NetworkBehaviour
{
    [SerializeField]
    TMP_Text text;

    [SerializeField]
    ServerScript script;
    private void Update()
    {
        //sets script te the ServerScript Singleton
        //prolly better to do this in OnNetWorkSpawn to save memory or whatever but this works
        script = GameObject.FindFirstObjectByType<ServerScript>();

        if (script != null)
        {
            if(!script.randomValues.Value.colorOne.Equals(null))
            //updates text to show the values as they change on the server side
            text.text = "ColorOne = " + script.randomValues.Value.colorOne + ", ColorTwo = " + script.randomValues.Value.colorTwo + "ColorShownToOwner = " + script.randomValues.Value.ColorVisibleToOwner;
        }
    }
}
