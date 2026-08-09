using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Game/Combat/Hitbox")]
public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Attributes")]
    [SerializeField] private bool _isCritical = true;

    private IDamageable _owner;
    
    
    public IDamageable Owner
    {
        get
        {
            _owner ??= GetComponentInParent<IDamageable>();
            return _owner;
        }
    }
    
    public bool IsCritical => _isCritical;

    private void Awake()
    {
        _owner = GetComponentInParent<IDamageable>();
    }
}
