using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Game/Combat/Power Up Pickup")]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField, Range(1.0f, 10.0f)] private float _multiplier = 2.0f;
    [SerializeField, Range(1.0f, 60.0f)] private float _duration = 10.0f;

    [Space(5), Header("Respawn")]
    [SerializeField, Range(1.0f, 120.0f)] private float _respawnDelay = 15.0f;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField, Range(0.0f, 5.0f)] private float _spawnHeight = 1.0f;

    [Space(5), Header("Motion")]
    [SerializeField] private float _spinSpeed = 90.0f;
    [SerializeField] private float _bobHeight = 0.3f;
    [SerializeField] private float _bobSpeed = 2.0f;

    private Vector3 _basePosition;
    private bool _isAvailable = true;

    public bool IsAvailable => _isAvailable;

    public float Multiplier
    {
        get => _multiplier;
        set => _multiplier = Mathf.Clamp(value, 1.0f, 10.0f);
    }

    public float Duration
    {
        get => _duration;
        set => _duration = Mathf.Clamp(value, 0.1f, 120.0f);
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _basePosition = transform.position;
    }

    private void Start()
    {
        Relocate();
    }

    private void Update()
    {
        if (!_isAvailable) { return; }

        transform.Rotate(0.0f, _spinSpeed * Time.deltaTime, 0.0f);

        float bob = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
        transform.position = _basePosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isAvailable || !other.CompareTag("Player")) { return; }

        if (GameManager.Exists)
        {
            GameManager.Instance.ActivateScoreMultiplier(_multiplier, _duration);
        }

        if (AudioManager.Exists) { AudioManager.Instance.PlayPickup(); }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        SetAvailable(false);

        yield return new WaitForSeconds(_respawnDelay);

        Relocate();
        SetAvailable(true);
    }

    private void Relocate()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0) { return; }

        Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        _basePosition = new Vector3(point.position.x, _spawnHeight, point.position.z);
        transform.position = _basePosition;
    }

    private void SetAvailable(bool available)
    {
        _isAvailable = available;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = available;
        }

        GetComponent<Collider>().enabled = available;
    }
}
