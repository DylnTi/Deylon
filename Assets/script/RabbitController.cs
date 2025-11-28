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
    
    // --- TEXT SKOR AKHIR DI PANEL (VARIABEL DIHAPUS SESUAI PERMINTAAN) ---
    
    // --- KONTROL ANIMASI ---
    private Animator anim; 

    
    // =========================================================
    // FUNGSI START
    // =========================================================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        anim = GetComponent<Animator>(); 

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
            // Tidak bisa melompat jika sedang Kinematic (sedang kebal/freeze)
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

        // Tabrakan dengan Rintangan
        // (Semua rintangan disamakan damagenya menjadi 1)
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("HeavyObstacle"))
        {
            if (isGameOver || isInvulnerable) 
            {
                return;
            }

            // --- 1. LOGIKA DAMAGE (1 HATI UNTUK SEMUA) ---
            currentLives -= 1;
            
            UpdateLivesUI();

            // --- 2. ANTI-DORONGAN & ANTI-JATUH ---
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // Hentikan kecepatan instan
                rb.angularVelocity = 0f;
                rb.isKinematic = true; // Kunci: Menghentikan Gravitasi dan Forces
            }

            // --- 3. CEK GAME OVER ---
            if (currentLives <= 0)
            {
                isGameOver = true; 
                Time.timeScale = 0f; 
                
                if (gameOverPanel != null)
                {
                    // Hanya set active panel, tidak ada update skor akhir
                    gameOverPanel.SetActive(true); 
                }
                if (pausePanel != null) pausePanel.SetActive(false); 
            }
            else
            {
                StartCoroutine(BecomeInvulnerable());
            }
        }
    }
    
    // =========================================================
    // FUNGSI KHUSUS
    // =========================================================
    
    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true; 
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Collider2D rabbitCollider = GetComponent<Collider2D>(); 

        // Nonaktifkan Collider sementara untuk menghindari hit berulang
        if (rabbitCollider != null) 
        {
            rabbitCollider.enabled = false;
        }
        
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
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Aktifkan kembali Collider dan Dynamic setelah kebal selesai
        if (!isGameOver)
        {
            // Mengembalikan status Dynamic agar gravitasi aktif kembali
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            
            if (rabbitCollider != null)
            {
                rabbitCollider.enabled = true;
            }
        }

        isInvulnerable = false; 
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
            // Hanya set active panel, tidak ada update skor akhir
            winPanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}