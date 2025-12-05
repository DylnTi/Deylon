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
    public float invulnerabilityTime = 0.5f; 
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

        score = 0f; // Score diset 0 di awal
        isGameOver = false;
        currentLives = maxLives; 
        isInvulnerable = false;
        
        Time.timeScale = 1f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false); 
        
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
    // FUNGSI TABRAKAN
    // =========================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Tabrakan dengan Tanah
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; 
        }

        // --- PENCEGAHAN DOUBLE HIT SEGERA ---
        if (isInvulnerable)
        {
            return; // Abaikan semua tabrakan jika sedang kebal
        }

        // 1. Tabrakan dengan Rintangan Biasa (TAG: "Obstacle") - 1 Nyawa Hilang
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeHit(1, false); // Damage 1, TIDAK reset skor
        }
        
        // 2. Tabrakan dengan Rintangan Mematikan (TAG: "Lethal") - Game Over & Reset Skor
        if (collision.gameObject.CompareTag("Lethal"))
        {
            // Mengambil semua nyawa (Game Over Instan) DAN reset skor
            TakeHit(maxLives, true); 
        }
    }
    
    // =========================================================
    // FUNGSI KHUSUS
    // =========================================================

    // Fungsi TakeHit kini menerima damageAmount dan flag resetScore
    public void TakeHit(int damageAmount, bool resetScore)
    {
        // Pengecekan keamanan
        if (isGameOver || isInvulnerable) 
        {
            return;
        }
        
        // --- BLOK PENCEGAHAN KRUSIAL ---
        
        // 1. SET isInvulnerable = true SEGERA
        isInvulnerable = true; 
        
        // 2. Nonaktifkan Collider SEGERA (Penting!)
        if (rabbitCollider != null) 
        {
            rabbitCollider.enabled = false; 
        }

        // 3. JEDA FISIKA: Hentikan Rigidbody (Kinematic = Freeze Posisi)
        if (rb != null)
        {
            rb.velocity = Vector2.zero; 
            rb.angularVelocity = 0f;
            rb.isKinematic = true; 
        }
        
        // LOGIKA DAMAGE
        currentLives -= damageAmount;
        UpdateLivesUI();

        // LOGIKA RESET SKOR (Baru ditambahkan)
        if (resetScore)
        {
            score = 0f;
            // Update skor di UI segera
            scoreText.text = "Score: " + Mathf.Round(score).ToString();
        }

        StopAllCoroutines(); 

        // CEK GAME OVER
        if (currentLives <= 0)
        {
            isGameOver = true; 
            Time.timeScale = 0f; 
            
            if (gameOverPanel != null) gameOverPanel.SetActive(true); 
            if (pausePanel != null) pausePanel.SetActive(false); 
            
            StopAllCoroutines(); 
        }
        else
        {
            // Mulai mode kebal
            StartCoroutine(BecomeInvulnerable());
        }
    }
    
    private IEnumerator BecomeInvulnerable()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Logika Kedip (Flashing)
        float startTime = Time.time;
        while (Time.time < startTime + invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
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
                rabbitCollider.enabled = true; // Aktifkan Collider kembali
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
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false); 
        }
        else 
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);  
        }
    }

    void HandleGameWin()
    {
        isGameOver = true; 
        Time.timeScale = 0f; 
        
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