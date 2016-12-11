using UnityEngine;

public class FireSpawner : MonoBehaviour
{
    public float speed;
    public float degree;
    public float delay;
    public float life = 10;

    public GameObject projectile;

    public GameObject gameStateObject;
    private GameState gameState;
    
	void OnEnable ()
    {
        CancelInvoke();
        gameState = gameStateObject.GetComponent<GameState>();
        Invoke("Spawn", delay);
	}

    void OnDisable ()
    {
        CancelInvoke();
    }
	
	void Spawn ()
    {
        var projectileInstance = (Instantiate(projectile, this.transform.position, Quaternion.identity) as GameObject);
        //TODO: pass to a variable
        Destroy(projectileInstance, life);
        Rigidbody2D instance = projectileInstance.GetComponent<Rigidbody2D>();
        instance.velocity = new Vector2(Mathf.Cos(degree * Mathf.Deg2Rad), Mathf.Sin(degree * Mathf.Deg2Rad)).normalized * speed;        
    }
}