using UnityEngine;

[AddComponentMenu("Game/Combat/Wandering")]
public class Wandering : MonoBehaviour
{
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private float _obstacleRange = 5.0f;
    [SerializeField] private float _sphereRadius = 0.75f;
    
    [SerializeField] private float _castHeight = 1.0f;

    [SerializeField] private GameObject _fireballPrefab;
    [HideInInspector] public GameObject Fireball;

    private bool _isAlive;

    public bool IsAlive
    {
        get => _isAlive;
        set => _isAlive = value;
    }

    private void Start()
    {
        IsAlive = true;
    }

    private void Update()
    {
        if (!IsAlive) { return; }

        transform.Translate(0.0f, 0.0f, _speed * Time.deltaTime);

        Ray ray = new(transform.position + Vector3.up * _castHeight, transform.forward);

        if (Physics.SphereCast(ray, _sphereRadius, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                // Guarded so dummies with no fireball assigned never call
                // Instantiate on a null prefab.
                if (_fireballPrefab && !Fireball)
                {
                    Fireball = Instantiate(
                        _fireballPrefab,
                        transform.TransformPoint(new Vector3(0.0f, _castHeight, 1.5f)),
                        transform.rotation
                    );
                }
            }

            else if (hit.distance < _obstacleRange && !hit.transform.CompareTag("Fireball"))
            {
                float theta = Random.Range(-135.0f, 135.0f);
                transform.Rotate(0.0f, theta, 0.0f);
            }
        }
    }
}
