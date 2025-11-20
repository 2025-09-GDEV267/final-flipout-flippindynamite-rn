using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class TextChat : NetworkBehaviour
{
    string submittedText;
    public TMP_InputField inputField;
    public NetworkVariable<string> textLog = new NetworkVariable<string>("Hewwo Wowd", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    string userText;

    ServerScript serverScript;

    public override void OnNetworkSpawn()
    {
        serverScript = FindFirstObjectByType<ServerScript>();
    }

    // Update is called once per frame
    void Update()
    {
         userText = inputField.text;
    }
    
    [Rpc(SendTo.Server)]
    public void sendMessageRpc()
    {
        UnityEngine.Debug.Log("Client RPC called");
        serverScript.textLog.Value = inputField.GetComponent<TMP_InputField>().text;
    }
}
