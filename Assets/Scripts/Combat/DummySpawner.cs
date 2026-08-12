using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Combat/Dummy Spawner")]
public class DummySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject _dummyPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    [Space(5), Header("Timing")]
    [SerializeField, Range(0.5f, 15.0f)] private float _spawnInterval = 3.0f;
    [SerializeField, Range(1, 30)] private int _maxAlive = 8;

    private readonly List<GameObject> _spawned = new();
    private float _timer;

    public int AliveCount => _spawned.Count;

    public float SpawnInterval
    {
        get => _spawnInterval;
        set => _spawnInterval = Mathf.Clamp(value, 0.1f, 60.0f);
    }

    private void Update()
    {
        if (GameManager.Exists && GameManager.Instance.State != GameState.Playing) { return; }

       
        _spawned.RemoveAll(dummy => !dummy);

        _timer += Time.deltaTime;

        if (_timer < _spawnInterval) { return; }

        _timer = 0.0f;

        if (_spawned.Count >= _maxAlive) { return; }

        Spawn();
    }

    private void Spawn()
    {
        if (!_dummyPrefab || _spawnPoints.Length == 0) { return; }

        Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        GameObject dummy = Instantiate(
            _dummyPrefab,
            point.position,
            Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)
        );

      
        TargetDummy target = dummy.GetComponent<TargetDummy>();

        if (target) { target.Respawns = false; }

        _spawned.Add(dummy);
    }
}
