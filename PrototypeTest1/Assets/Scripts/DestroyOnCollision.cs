using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public LayerMask destroyLayers;
    [SerializeField] private Throwing throwingScript; 

    

    private void OnCollisionEnter(Collision collision)
    {
        
        if (destroyLayers == (destroyLayers | (1 << collision.gameObject.layer)))
        {
            throwingScript.teleportToPosition(collision.contacts[0].point + collision.contacts[0].normal * 0.5f);
            Destroy(gameObject);
        }
    
        
    }
}
