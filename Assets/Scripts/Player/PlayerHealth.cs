using System;
using UnityEngine;


[AddComponentMenu("Game/Player/Player Health")]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Range(1, 500)] private int _maxHealth = 100;

    [Space(5), Header("Damage Response")]
    [SerializeField, Range(0.0f, 2.0f)] private float _invulnerabilityTime = 0.5f;

    private int _health;
    private float _invulnerableUntil;

  
    public int Health
    {
        get => _health;
        private set
        {
            int clamped = Mathf.Clamp(value, 0, _maxHealth);

            if (clamped == _health) { return; }

            _health = clamped;
            OnHealthChanged?.Invoke(_health, _maxHealth);
        }
    }

  
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            int previous = _maxHealth;
            _maxHealth = Mathf.Max(1, value);
            Health = _health + (_maxHealth - previous);
        }
    }

  
    public float HealthFraction => _maxHealth > 0 ? (float)_health / _maxHealth : 0.0f;


    public bool IsAlive => _health > 0;

  
    public bool IsInvulnerable => Time.time < _invulnerableUntil;



    public event Action<int, int> OnHealthChanged;  // current, max
    public event Action OnDamaged;
    public event Action OnDied;

  

    private void Awake()
    {
        _health = _maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_health, _maxHealth);
    }



    public void TakeDamage(int amount)
    {
        if (amount <= 0 || !IsAlive || IsInvulnerable) { return; }

        Health -= amount;
        _invulnerableUntil = Time.time + _invulnerabilityTime;

        OnDamaged?.Invoke();

        if (AudioManager.Exists) { AudioManager.Instance.PlayPlayerHurt(); }

        if (!IsAlive) { Die(); }
    }

 
    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive) { return; }

        Health += amount;
    }

    private void Die()
    {
        OnDied?.Invoke();

        if (GameManager.Exists) { GameManager.Instance.PlayerDied(); }
    }
}
