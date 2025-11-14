using UnityEngine;
using Unity.Netcode;

/*
 * Short transform changing script to test transference of information between instances 
 */
public class TestScript : NetworkBehaviour
{
    bool snatch = false;
    private void Update()
    {
        if (!snatch)
        {
            Invoke("updatePos", 2);
            snatch = true;
        }
    }
    void updatePos()
    {
        gameObject.transform.position = new Vector3(Random.Range(-3, 4),0,0);
        snatch = false;
    }
}
