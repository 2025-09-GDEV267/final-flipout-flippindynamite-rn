using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(this.gameObject);
            Debug.LogWarning("Duplicate GameManager instance found. Destroying this one.");
        }
        else
        {
            // If no instance exists, set this one as the instance
            Instance = this;
            // Optional: Prevent the object from being destroyed when loading new scenes
            DontDestroyOnLoad(this.gameObject);
        }
    }

    

}
