using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Game/Managers/Game Manager")]
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    
    public static GameManager Instance => _instance;
    
    public static bool Exists => _instance != null;
    

    [Header("Scenes")]
    [SerializeField] private string _menuSceneName = "MainMenu";
    [SerializeField] private string _arenaSceneName = "Arena";

    [Space(5), Header("Round Settings")]
    [SerializeField, Range(15.0f, 300.0f)] private float _roundDuration = 60.0f;

    [Space(5), Header("Scoring")]
    [SerializeField, Range(1.0f, 5.0f)] private float _headshotMultiplier = 2.0f;

    private const string HighScoreKey = "BLAIM_HighScore";

    private GameState _state = GameState.Menu;
    private int _score;
    private int _shotsFired;
    private int _shotsHit;
    private float _timeRemaining;
    private float _scoreMultiplier = 1.0f;
    private Coroutine _multiplierRoutine;
    
    public GameState State
    {
        get => _state;
        private set
        {
            if (_state == value) { return; }

            _state = value;
            ApplyStateSideEffects(_state);
            OnStateChanged?.Invoke(_state);
        }
    }
    
    public int Score
    {
        get => _score;
        private set
        {
            _score = Mathf.Max(0, value);
            OnScoreChanged?.Invoke(_score);
        }
    }
    
    public int HighScore
    {
        get => PlayerPrefs.GetInt(HighScoreKey, 0);
        private set
        {
            if (value <= HighScore) { return; }

            PlayerPrefs.SetInt(HighScoreKey, value);
            PlayerPrefs.Save();
        }
    }

   
    public float ScoreMultiplier
    {
        get => _scoreMultiplier;
        private set => _scoreMultiplier = Mathf.Max(1.0f, value);
    }
    
    public int ShotsFired => _shotsFired;
    
    public int ShotsHit => _shotsHit;
    
    public float Accuracy => _shotsFired == 0 ? 1.0f : (float)_shotsHit / _shotsFired;
    
    public float AccuracyPercent => Accuracy * 100.0f;
    
    public float TimeRemaining
    {
        get => _timeRemaining;
        private set => _timeRemaining = Mathf.Max(0.0f, value);
    }
    
    public float RoundDuration => _roundDuration;
    
    public string ArenaSceneName => _arenaSceneName;
    
    public string MenuSceneName => _menuSceneName;
    

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<bool> OnRoundEnded;
    public event Action<float, float> OnPowerUpTimerChanged; 
    

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == _arenaSceneName && State != GameState.Playing)
        {
            StartRound();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _instance = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            (State == GameState.Playing || State == GameState.Paused))
        {
            TogglePause();
        }

        if (State != GameState.Playing) { return; }
        
        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0.0f)
        {
            EndRound(false);
        }
    }
    
    public void StartRound()
    {
        _score = 0;
        _shotsFired = 0;
        _shotsHit = 0;
        _scoreMultiplier = 1.0f;

        TimeRemaining = _roundDuration;

        if (_multiplierRoutine != null)
        {
            StopCoroutine(_multiplierRoutine);
            _multiplierRoutine = null;
        }

        State = GameState.Playing;

        OnScoreChanged?.Invoke(_score);
    }
    
    public void EndRound(bool victory)
    {
        if (State == GameState.GameOver || State == GameState.Victory) { return; }

        HighScore = Score;

        State = victory ? GameState.Victory : GameState.GameOver;
        OnRoundEnded?.Invoke(victory);
    }
    
    public void PlayerDied()
    {
        EndRound(false);
    }
    
    public void RegisterShot(bool hitTarget)
    {
        _shotsFired++;

        if (hitTarget) { _shotsHit++; }
    }
    
    public int AddScore(int basePoints, bool headshot)
    {
        float points = basePoints * ScoreMultiplier;

        if (headshot) { points *= _headshotMultiplier; }

        int awarded = Mathf.RoundToInt(points);

        Score += awarded;

        return awarded;
    }
    
    public void ActivateScoreMultiplier(float multiplier, float duration)
    {
        if (_multiplierRoutine != null) { StopCoroutine(_multiplierRoutine); }

        _multiplierRoutine = StartCoroutine(ScoreMultiplierRoutine(multiplier, duration));
    }

    private IEnumerator ScoreMultiplierRoutine(float multiplier, float duration)
    {
        ScoreMultiplier = multiplier;

        float remaining = duration;

        while (remaining > 0.0f)
        {
            remaining -= Time.deltaTime;
            OnPowerUpTimerChanged?.Invoke(remaining, duration);
            yield return null;
        }

        ScoreMultiplier = 1.0f;
        OnPowerUpTimerChanged?.Invoke(0.0f, duration);
        _multiplierRoutine = null;
    }

    public void TogglePause()
    {
        State = State == GameState.Paused ? GameState.Playing : GameState.Paused;
    }

    public void RestartRound()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(_arenaSceneName);
    }

    public void LoadArena()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(_arenaSceneName);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1.0f;
        State = GameState.Menu;
        SceneManager.LoadScene(_menuSceneName);
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        ReturnToMenu();
#else
        Application.Quit();
#endif
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _arenaSceneName)
        {
            StartRound();
        }
        else
        {
            State = GameState.Menu;
        }
    }

    private void ApplyStateSideEffects(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1.0f;
                SetCursorLocked(true);
                break;

            case GameState.Paused:
            case GameState.GameOver:
            case GameState.Victory:
                Time.timeScale = 0.0f;
                SetCursorLocked(false);
                break;

            case GameState.Menu:
                Time.timeScale = 1.0f;
                SetCursorLocked(false);
                break;
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
