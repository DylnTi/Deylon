using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections; 

public class RabbitController : MonoBehaviour
{
    // --- PENGATURAN FISIKA & LOMPATAN ---
    public float jumpForce = 500f; 
    private Rigidbody2D rb; 
    private bool isGrounded = true; 
    
    // --- PENGATURAN LIVES (HATI) ---
    public int maxLives = 3; 
    private int currentLives; 
    public TextMeshProUGUI livesText;
    public float invulnerabilityTime = 0.5f; // Durasi kebal setelah terkena hit
    private bool isInvulnerable = false;
    
    // --- PENGATURAN UI & SKOR ---
    public TextMeshProUGUI scoreText;
    private float score = 0f; 
    private const float WIN_SCORE = 440f; 
    
    // --- PENGATURAN GAME STATE & PANEL ---
    private bool isGameOver = false; 
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject pausePanel;
    
    // --- KONTROL ANIMASI & COLLIDER ---
    private Animator anim; 
    private Collider2D rabbitCollider; 

    
    // =========================================================
    // FUNGSI START
    // =========================================================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        anim = GetComponent<Animator>(); 
        rabbitCollider = GetComponent<Collider2D>(); 

        score = 0f; 
        isGameOver = false;
        currentLives = maxLives; 
        isInvulnerable = false;
        
        Time.timeScale = 1f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false); 
        
        // Pastikan kelinci dimulai sebagai Dynamic
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        UpdateLivesUI(); 
    }

    
    // =========================================================
    // FUNGSI UPDATE
    // =========================================================
    void Update()
    {
        if (isGameOver)
        {
            return; 
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // --- LOMPATAN & ANIMASI ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Hanya bisa melompat jika game tidak di-pause dan Rigidbody tidak Kinematic
            if (Time.timeScale > 0f && rb != null && !rb.isKinematic)
            {
                rb.AddForce(new Vector2(0f, jumpForce));
                isGrounded = false;
                anim.SetTrigger("Jump");
            }
        }

        if (anim != null)
        {
            anim.SetBool("IsGrounded", isGrounded); 
        }

        // --- UPDATE SKOR ---
        score += Time.deltaTime * 10f; 
        scoreText.text = "Score: " + Mathf.Round(score).ToString();
        
        if (score >= WIN_SCORE)
        {
            HandleGameWin();
        }
    }

    
    // =========================================================
    // FUNGSI TABRAKAN (Hanya mengecek "Obstacle")
    // =========================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Tabrakan dengan Tanah
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; 
        }

        // Tabrakan dengan Rintangan (HANYA mengecek tag "Obstacle")
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // 1. CEK: Jika sudah Game Over atau sudah Invulnerable, KELUAR
            if (isGameOver || isInvulnerable) 
            {
                return;
            }
            
            // --- EKSEKUSI DAMAGE & PENCEGAHAN SEGERA ---
            
            // 2. Cegah double hit: SET isInvulnerable = true SEGERA
            isInvulnerable = true; 
            
            // 3. LOGIKA DAMAGE (Mengurangi 1 nyawa)
            currentLives -= 1;
            UpdateLivesUI();

            // 4. Hentikan Coroutine lama (Penting untuk mencegah timing bug)
            StopAllCoroutines(); 

            // 5. ANTI-DORONGAN & ANTI-JATUH (Freeze sesaat)
            if (rb != null)
            {
                rb.velocity = Vector2.zero; 
                rb.angularVelocity = 0f;
                rb.isKinematic = true; // Set Kinematic untuk freeze posisi
            }
            
            // 6. Nonaktifkan Collider SEGERA sebelum Coroutine dimulai
             if (rabbitCollider != null) 
            {
                rabbitCollider.enabled = false; 
            }

            // 7. CEK GAME OVER
            if (currentLives <= 0)
            {
                isGameOver = true; 
                Time.timeScale = 0f; // Hentikan Game
                
                if (gameOverPanel != null) gameOverPanel.SetActive(true); 
                if (pausePanel != null) pausePanel.SetActive(false); 
            }
            else
            {
                // Mulai mode kebal
                StartCoroutine(BecomeInvulnerable());
            }
        }
    }
    
    // =========================================================
    // FUNGSI KHUSUS
    // =========================================================
    
    private IEnumerator BecomeInvulnerable()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Logika Kedip (Flashing)
        float startTime = Time.time;
        while (Time.time < startTime + invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
                // Toggle sprite visibility untuk efek kedip
                spriteRenderer.enabled = !spriteRenderer.enabled; 
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        // Pastikan sprite terlihat kembali
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Aktifkan kembali Collider dan Dynamic setelah kebal selesai
        if (!isGameOver)
        {
            if (rb != null)
            {
                rb.isKinematic = false; // Kembalikan gravitasi (Dynamic)
            }
            
            if (rabbitCollider != null)
            {
                rabbitCollider.enabled = true; // Aktifkan Collider
            }
        }

        isInvulnerable = false; // Matikan mode kebal
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = " " + currentLives.ToString();
        }
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        if (Time.timeScale == 0f)
        {
            // Unpause
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false); 
        }
        else 
        {
            // Pause
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);  
        }
    }

    void HandleGameWin()
    {
        isGameOver = true; 
        Time.timeScale = 0f; // Freeze game
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}