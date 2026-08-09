using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Game/Combat/Fireball")]
public class Fireball : MonoBehaviour
{
    [SerializeField] private float _speed = 15.0f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _lifetime = 5.0f;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Fireballs that never hit anything would otherwise fly forever.
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        transform.Translate(0.0f, 0.0f, _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball")) { return; }

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player) { player.TakeDamage(_damage); }

        Destroy(gameObject);
    }
}
