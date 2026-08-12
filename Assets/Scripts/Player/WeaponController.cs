using System;
using UnityEngine;


[AddComponentMenu("Game/Player/Weapon Controller")]
public class WeaponController : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField, Range(1, 100)] private int _damage = 25;
    [SerializeField, Range(10.0f, 500.0f)] private float _range = 150.0f;
    [SerializeField] private LayerMask _hitMask = ~0;

    private Camera _cam;
    
    public int Damage
    {
        get => _damage;
        set => _damage = Mathf.Max(1, value);
    }
    
    public float Range
    {
        get => _range;
        set => _range = Mathf.Clamp(value, 1.0f, 1000.0f);
    }



    public event Action<bool> OnShotResolved;   // true when a target was hit

  

    private void Awake()
    {
  
        _cam = GetComponentInChildren<Camera>();

        if (!_cam) { _cam = Camera.main; }
    }

    private void Update()
    {
        if (GameManager.Exists && GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) { Fire(); }
    }
    

    private void Fire()
    {
        if (AudioManager.Exists) { AudioManager.Instance.PlayGunshot(); }

        Vector3 screenMiddle = new(_cam.pixelWidth * 0.5f, _cam.pixelHeight * 0.5f, 0.0f);
        Ray ray = _cam.ScreenPointToRay(screenMiddle);

        bool hitTarget = false;

       
        if (Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask, QueryTriggerInteraction.Ignore))
        {
            hitTarget = ResolveHit(hit);
        }

        if (GameManager.Exists) { GameManager.Instance.RegisterShot(hitTarget); }

        OnShotResolved?.Invoke(hitTarget);
    }

   
    private bool ResolveHit(RaycastHit hit)
    {
        Hitbox hitbox = hit.collider.GetComponent<Hitbox>();

        IDamageable target = hitbox
            ? hitbox.Owner
            : hit.collider.GetComponentInParent<IDamageable>();

        if (target == null || !target.IsAlive) { return false; }

        bool headshot = hitbox && hitbox.IsCritical;

        target.TakeDamage(_damage, headshot);

     
        if (AudioManager.Exists && target.IsAlive) { AudioManager.Instance.PlayEnemyHit(); }

        return true;
    }
}
