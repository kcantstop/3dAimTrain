using TMPro;
using UnityEngine;
using UnityEngine.UI;


[AddComponentMenu("Game/Managers/UI Manager")]
[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{

    private static UIManager _instance;

    public static UIManager Instance => _instance;

    public static bool Exists => _instance != null;
    

    [Header("HUD")]
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _accuracyText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private Image _healthBar;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _hitMarker;
    [SerializeField, Range(0.02f, 0.5f)] private float _hitMarkerTime = 0.08f;

    [Space(5), Header("Power-Up")]
    [SerializeField] private TMP_Text _powerUpText;

    [Space(5), Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private GameObject _resultsPanel;
    [SerializeField] private TMP_Text _resultsTitleText;
    [SerializeField] private TMP_Text _resultsStatsText;
    

    private PlayerHealth _playerHealth;
    private WeaponController _weapon;
    private MouseLook[] _mouseLooks;
    private float _hitMarkerTimer;
    

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) { _instance = null; }
    }

    private void Start()
    {
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
        _weapon = FindFirstObjectByType<WeaponController>();
        _mouseLooks = FindObjectsByType<MouseLook>(FindObjectsSortMode.None);

        if (_sensitivitySlider)
        {
            _sensitivitySlider.minValue = MouseLook.MinSliderValue;
            _sensitivitySlider.maxValue = MouseLook.MaxSliderValue;

            float current = _mouseLooks.Length > 0 ? _mouseLooks[0].Sensitivity : 0.0f;
            _sensitivitySlider.SetValueWithoutNotify(MouseLook.SliderFromSensitivity(current));
        }

        Subscribe();
        RefreshAll();

        ShowPanel(_pausePanel, false);
        ShowPanel(_resultsPanel, false);
        ShowPanel(_hitMarker, false);
        ShowPowerUp(false);

        if (AudioManager.Exists) { AudioManager.Instance.PlayGameMusic(); }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        UpdateTimer();
        UpdateHitMarker();
    }
    

    private void Subscribe()
    {
        if (GameManager.Exists)
        {
            GameManager gm = GameManager.Instance;

            gm.OnScoreChanged += HandleScoreChanged;
            gm.OnStateChanged += HandleStateChanged;
            gm.OnRoundEnded += HandleRoundEnded;
            gm.OnPowerUpTimerChanged += HandlePowerUpTimer;
        }

        if (_playerHealth) { _playerHealth.OnHealthChanged += HandleHealthChanged; }
        if (_weapon) { _weapon.OnShotResolved += HandleShotResolved; }
    }

    private void Unsubscribe()
    {
        if (GameManager.Exists)
        {
            GameManager gm = GameManager.Instance;

            gm.OnScoreChanged -= HandleScoreChanged;
            gm.OnStateChanged -= HandleStateChanged;
            gm.OnRoundEnded -= HandleRoundEnded;
            gm.OnPowerUpTimerChanged -= HandlePowerUpTimer;
        }

        if (_playerHealth) { _playerHealth.OnHealthChanged -= HandleHealthChanged; }
        if (_weapon) { _weapon.OnShotResolved -= HandleShotResolved; }
    }

    private void RefreshAll()
    {
        if (GameManager.Exists) { HandleScoreChanged(GameManager.Instance.Score); }

        if (_playerHealth) { HandleHealthChanged(_playerHealth.Health, _playerHealth.MaxHealth); }
    }

    private void HandleScoreChanged(int score)
    {
        if (_scoreText) { _scoreText.text = $"SCORE {score:N0}"; }

        UpdateAccuracy();
    }

    private void HandleHealthChanged(int health, int maxHealth)
    {
        if (_healthText) { _healthText.text = $"HP {health}"; }

        if (_healthBar)
        {
            _healthBar.fillAmount = maxHealth > 0 ? (float)health / maxHealth : 0.0f;
        }
    }

    private void HandleShotResolved(bool hit)
    {
        UpdateAccuracy();

        if (!hit || !_hitMarker) { return; }

        _hitMarker.SetActive(true);
        _hitMarkerTimer = _hitMarkerTime;
    }

    private void UpdateHitMarker()
    {
        if (_hitMarkerTimer <= 0.0f) { return; }

        _hitMarkerTimer -= Time.unscaledDeltaTime;

        if (_hitMarkerTimer <= 0.0f) { ShowPanel(_hitMarker, false); }
    }

    private void UpdateAccuracy()
    {
        if (!_accuracyText || !GameManager.Exists) { return; }

        _accuracyText.text = $"ACC {GameManager.Instance.AccuracyPercent:0}%";
    }

    private void UpdateTimer()
    {
        if (!_timerText || !GameManager.Exists) { return; }

        float remaining = GameManager.Instance.TimeRemaining;
        int minutes = Mathf.FloorToInt(remaining / 60.0f);
        int seconds = Mathf.FloorToInt(remaining % 60.0f);

        _timerText.text = $"{minutes:00}:{seconds:00}";
        _timerText.color = remaining <= 10.0f ? Color.red : Color.white;
    }


    private void HandlePowerUpTimer(float remaining, float total)
    {
        bool active = remaining > 0.0f;

        ShowPowerUp(active);

        if (active && _powerUpText)
        {
            _powerUpText.text = $"DOUBLE POINTS  {remaining:0.0}s";
        }
    }

    private void ShowPowerUp(bool visible)
    {
        if (_powerUpText) { _powerUpText.gameObject.SetActive(visible); }
    }

    private void HandleStateChanged(GameState state)
    {
        ShowPanel(_pausePanel, state == GameState.Paused);
        ShowPanel(_crosshair, state == GameState.Playing);

        if (state == GameState.Playing) { ShowPanel(_resultsPanel, false); }
    }

    private void HandleRoundEnded(bool victory)
    {
        ShowPanel(_resultsPanel, true);
        ShowPanel(_crosshair, false);

        if (_resultsTitleText)
        {
            _resultsTitleText.text = victory ? "ARENA CLEARED" : "GAME OVER";
        }

        if (_resultsStatsText && GameManager.Exists)
        {
            GameManager gm = GameManager.Instance;

            _resultsStatsText.text =
                $"SCORE     {gm.Score:N0}\n" +
                $"ACCURACY  {gm.AccuracyPercent:0.0}%  ({gm.ShotsHit}/{gm.ShotsFired})";
        }
    }

    private static void ShowPanel(GameObject panel, bool visible)
    {
        if (panel) { panel.SetActive(visible); }
    }

    // Applies to the live MouseLook components as well as the saved value, so
    // the change is felt as soon as the pause menu closes.
    public void OnSensitivityChanged(float sliderValue)
    {
        if (_mouseLooks == null) { return; }

        float sensitivity = MouseLook.SensitivityFromSlider(sliderValue);

        foreach (MouseLook look in _mouseLooks)
        {
            if (look) { look.Sensitivity = sensitivity; }
        }
    }

    public void OnResumeButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.TogglePause(); }
    }

    public void OnRestartButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.RestartRound(); }
    }

    public void OnMenuButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.ReturnToMenu(); }
    }

    public void OnQuitButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.QuitGame(); }
    }

    private static void PlayClick()
    {
        if (AudioManager.Exists) { AudioManager.Instance.PlayUiClick(); }
    }
}
