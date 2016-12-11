using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 80;
    public GameObject GameStateObject;
    public GameObject MarkObject;
    public GameObject MessageCanvas;
    public Text MessageField;
    private Rigidbody2D rigidBody;
    private GameState gameState;
    private string interactingTag;
    private bool canMove;

    void Start ()
    {
        MarkObject.SetActive(false);
        MessageCanvas.SetActive(false);
        rigidBody = GetComponent<Rigidbody2D>();
        gameState = GameStateObject.GetComponent<GameState>();
        canMove = true;
	}

    void Update ()
    {
        if (MarkObject.activeSelf)
        {
            //TODO: Fix for controller
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 0"))
            {
                ShowMessage(gameState.InteractWithTag(interactingTag));
            }
        }
        else
        {
            if (MessageCanvas.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 0"))
                {
                    canMove = true;
                    MarkObject.SetActive(true);
                    MessageCanvas.SetActive(false);
                }
            }
        }
    }

	void FixedUpdate ()
    {
        if (canMove)
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            var movementVector = new Vector2(horizontal, vertical).normalized * speed;

            rigidBody.velocity = movementVector;
        }
        else
            rigidBody.velocity = Vector2.zero;
	}

    void OnTriggerEnter2D (Collider2D collider)
    {
        if(collider.tag == "Enemy")
        {
            gameState.PlayerDamaged();
            return;
        }

        if(collider.tag == "Survive")
        {
            gameState.PlayerSurvived();
            return;
        }
        MarkObject.SetActive(true);
        interactingTag = collider.tag;
    }

    void OnTriggerExit2D (Collider2D collider)
    {
        MarkObject.SetActive(false);
        interactingTag = null;
    }

    void ShowMessage (string message)
    {
        if (message != null)
        {
            canMove = false;
            MarkObject.SetActive(false);
            MessageCanvas.SetActive(true);
            MessageField.text = message;
        }
    }
}