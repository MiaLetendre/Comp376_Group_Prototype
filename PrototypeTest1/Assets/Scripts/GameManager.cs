using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public Rigidbody playerRb;

    public GameObject gameOverPanel;
    public GameObject winPanel;
    public MonoBehaviour gameCamera;
    public Throwing throwScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        LockGame();
    }

    
    void LockGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        Cursor.lockState = CursorLockMode.None;
        throwScript.totalProjectiles = throwScript.maxProjectiles;
        Cursor.visible = true;
        gameCamera.enabled = false;
        Time.timeScale = 0f;
        playerRb.linearVelocity = Vector2.zero;
    }

    public void ResetGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (winPanel != null)
        {
            Debug.Log(winPanel.name);
            winPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameCamera.enabled = true;
       
        playerRb.position = Vector2.zero;
        playerRb.linearVelocity = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
