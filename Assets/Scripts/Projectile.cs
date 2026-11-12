using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public string typeName = "Zwykły";
    public bool destroyOnHit = true;        // typ 1: true, typ 2: false
    public float lifetimeAfterHit = 5f;     // typ 2: znika po 5 s od uderzenia
    public float missLifetime = 10f;        // zabezpieczenie, gdy pocisk nic nie trafi
    public GameObject hitEffectPrefab;      // efekt cząsteczek
    public AudioClip hitSound;              // opcjonalny dźwięk

    Rigidbody rb;
    bool hasHit = false;
    float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // szybkie pociski nie przenikają
        spawnTime = Time.time;
    }

    public void Launch(Vector3 direction, float speed)
    {
        rb.AddForce(direction * speed, ForceMode.VelocityChange);
    }

    void Update()
    {
        if (!hasHit && Time.time - spawnTime > missLifetime) Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Vector3 point = collision.GetContact(0).point;

        // tarcza (pkt 5) i obiekty destrukcyjne (pkt 6)
        Target target = collision.collider.GetComponentInParent<Target>();
        if (target != null) target.Hit(typeName);

        DestructibleObject destructible = collision.collider.GetComponentInParent<DestructibleObject>();
        if (destructible != null) destructible.Break(point);

        // efekt uderzenia (pkt 2)
        if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, point, Quaternion.identity);
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, point);

        if (destroyOnHit || destructible != null)
        {
            Destroy(gameObject);                    // typ 1: znika od razu
        }
        else
        {
            rb.isKinematic = true;                  // typ 2: zostaje w miejscu
            Destroy(gameObject, lifetimeAfterHit);  // i znika po 5 s od uderzenia
        }
    }
}