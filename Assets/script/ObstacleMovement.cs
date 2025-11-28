using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    // Kecepatan pergerakan kaktus (public agar bisa diatur di Inspector)
    public float speed = 5f; 
    
    // Titik X di mana kaktus akan dihapus (di luar layar kiri)
    private float destroyPoint = -15f; 

    void Update()
    {
        // Pindahkan kaktus ke kiri secara terus-menerus
        // Time.deltaTime memastikan pergerakan halus dan tidak tergantung frame rate
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Cek jika kaktus sudah melewati batas kiri layar
        if (transform.position.x < destroyPoint)
        {
            // Hapus (Destroy) objek kaktus ini dari game
            Destroy(gameObject);
        }
    }
}