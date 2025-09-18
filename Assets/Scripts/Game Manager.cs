using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] ghosts;
    public GameObject pacman;
    public Transform pellets;

    public int score { get; private set; }
    public int lives { get; private set; } = 3;

    public static GameManager instance;

    // Script references for better performance
    private Pacman pacmanScript;
    private Ghost[] ghostScripts;
    
    bool gameEnded = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Cache script references
        if (pacman != null)
            pacmanScript = pacman.GetComponent<Pacman>();
        
        ghostScripts = new Ghost[ghosts.Length];
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null)
                ghostScripts[i] = ghosts[i].GetComponent<Ghost>();
        }
    }

    void Update()
    {
        if (!gameEnded)
        {
            if (pellets.childCount == 0)
            {
                WinGame();
                gameEnded = true;
            }
            else if (lives <= 0)
            {
                GameOver();
                gameEnded = true;
            }
        }
    }

    public void AddScore(int points) => score += points;

    public void LoseLife()
    {
        lives--;
        if (lives > 0)
            ResetPositions();
    }

    public void ScareGhosts()
    {
        foreach (var ghost in ghostScripts)
        {
            if (ghost != null)
                ghost.BecomeScared();
        }
    }

    void WinGame() 
    {
        Debug.Log($"WIN! Score: {score}");
        DisableGameplay();
    }
    
    void GameOver() 
    {
        Debug.Log($"GAME OVER! Score: {score}");
        DisableGameplay();
    }

    void DisableGameplay()
    {
        // Disable scripts to stop gameplay
        if (pacmanScript != null)
            pacmanScript.enabled = false;
        
        foreach (var ghost in ghostScripts)
        {
            if (ghost != null)
                ghost.enabled = false;
        }
    }

    void ResetPositions()
    {
        // Reset Pacman position
        if (pacmanScript != null)
            pacmanScript.Respawn();
        
        // Reset ghost positions
        foreach (var ghost in ghostScripts)
        {
            if (ghost != null)
                ghost.ResetToStart();
        }
    }

    public void RestartGame()
    {
        // Reset game state
        score = 0;
        lives = 3;
        gameEnded = false;
        
        // Enable gameplay scripts
        if (pacmanScript != null)
        {
            pacmanScript.enabled = true;
            pacmanScript.ResetToStart();
        }
        
        foreach (var ghost in ghostScripts)
        {
            if (ghost != null)
            {
                ghost.enabled = true;
                ghost.ResetToStart();
            }
        }
        
        Debug.Log("Game Restarted!");
    }
}

public class Pacman : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning("Animator not found on Pacman!");
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"), v = Input.GetAxis("Vertical");
        if (anim != null)
        {
            if (h > 0) anim.SetTrigger("WalkRight");
            else if (h < 0) anim.SetTrigger("WalkLeft");
            else if (v > 0) anim.SetTrigger("WalkUp");
            else if (v < 0) anim.SetTrigger("WalkDown");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            GameManager.instance.AddScore(10);
        }
        else if (other.CompareTag("PowerPellet"))
        {
            Destroy(other.gameObject);
            GameManager.instance.AddScore(50);
            GameManager.instance.ScareGhosts();
        }
        else if (other.CompareTag("Ghost"))
        {
            var ghost = other.GetComponent<Ghost>();
            if (ghost != null && ghost.isScared)
            {
                GameManager.instance.AddScore(200);
                ghost.GetEaten();
            }
            else
            {
                if (anim != null)
                    anim.SetTrigger("Dead");
                GameManager.instance.LoseLife();
            }
        }
    }

    public void Respawn()
    {
        // Reset to starting position - you'll need to set this
        // transform.position = startPosition;
        
        // Reset animation state
        if (anim != null)
            anim.SetTrigger("Idle");
    }

    public void ResetToStart()
    {
        // Complete reset for new game
        Respawn();
        // Reset any other Pacman state here
    }
}

public class Ghost : MonoBehaviour
{
    public bool isScared;
    public bool isDead;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning("Animator not found on Ghost!");
    }

    public void BecomeScared()
    {
        if (isScared || isDead) return;
        isScared = true;
        if (anim != null)
            anim.SetTrigger("Scared");
        CancelInvoke();
        Invoke(nameof(StartRecovering), 7f);
        Invoke(nameof(ReturnNormal), 10f);
    }

    void StartRecovering()
    {
        if (isScared && !isDead && anim != null)
            anim.SetTrigger("Recovering");
    }

    void ReturnNormal()
    {
        if (isDead) return;
        isScared = false;
        if (anim != null)
            anim.SetTrigger("Walking");
    }

    public void GetEaten()
    {
        isDead = true;
        isScared = false;
        if (anim != null)
            anim.SetTrigger("Dead");
        
        CancelInvoke();
        Invoke(nameof(Respawn), 3f); // Respawn after 3 seconds
    }

    void Respawn()
    {
        isDead = false;
        isScared = false;
        // Reset to starting position - you'll need to set this
        // transform.position = startPosition;
        
        if (anim != null)
            anim.SetTrigger("Walking");
    }

    public void ResetToStart()
    {
        CancelInvoke();
        isDead = false;
        isScared = false;
        // Reset to starting position
        // transform.position = startPosition;
        
        if (anim != null)
            anim.SetTrigger("Walking");
    }
}