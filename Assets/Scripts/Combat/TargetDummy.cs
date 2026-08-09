using System.Collections;
using UnityEngine;


[AddComponentMenu("Game/Combat/Target Dummy")]
public class TargetDummy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Range(1, 500)] private int _maxHealth = 50;

    [Space(5), Header("Scoring")]
    [SerializeField, Range(1, 500)] private int _pointValue = 10;

    [Space(5), Header("Defeat")]
    [SerializeField, Range(0.1f, 5.0f)] private float _defeatAnimTime = 0.6f;
    [SerializeField] private bool _respawns = true;
    [SerializeField, Range(0.5f, 30.0f)] private float _respawnDelay = 3.0f;

    private int _health;
    private bool _isAlive = true;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    
    
    public int Health
    {
        get => _health;
        private set => _health = Mathf.Clamp(value, 0, _maxHealth);
    }

    public int MaxHealth => _maxHealth;
    
    public int PointValue
    {
        get => _pointValue;
        set => _pointValue = Mathf.Max(0, value);
    }
    
    public bool IsAlive => _isAlive;


    private Wandering _wandering;

    private void Awake()
    {
        _health = _maxHealth;
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _wandering = GetComponent<Wandering>();
    }
    

    public void TakeDamage(int amount, bool headshot)
    {
        if (!_isAlive) { return; }

        Health -= amount;

        if (Health <= 0) { Defeat(headshot); }
    }

    private void Defeat(bool headshot)
    {
        _isAlive = false;

        if (_wandering) { _wandering.IsAlive = false; }

        if (GameManager.Exists) { GameManager.Instance.AddScore(_pointValue, headshot); }
        if (AudioManager.Exists) { AudioManager.Instance.PlayEnemyDeath(); }

        StartCoroutine(DefeatRoutine());
    }

    private IEnumerator DefeatRoutine()
    {
        float timer = 0.0f;

        Quaternion initRotation = transform.rotation;
        Quaternion endRotation = transform.rotation * Quaternion.Euler(-80.0f, 0.0f, 0.0f);

        while (timer < _defeatAnimTime)
        {
            transform.rotation = Quaternion.Lerp(initRotation, endRotation, timer / _defeatAnimTime);

            timer += Time.deltaTime;

            yield return null;  // Skip a frame.
        }

        transform.rotation = endRotation;

        if (!_respawns)
        {
            Destroy(gameObject);
            yield break;
        }

        SetVisible(false);

        yield return new WaitForSeconds(_respawnDelay);

        Respawn();
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(_startPosition, _startRotation);

        _health = _maxHealth;
        _isAlive = true;

        if (_wandering) { _wandering.IsAlive = true; }

        SetVisible(true);
    }
    
    private void SetVisible(bool visible)
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = visible;
        }

        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = visible;
        }
    }
}
