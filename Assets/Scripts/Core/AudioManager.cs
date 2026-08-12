using System.Collections;
using UnityEngine;


[AddComponentMenu("Game/Managers/Audio Manager")]
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{


    private static AudioManager _instance;

    public static AudioManager Instance => _instance;

    public static bool Exists => _instance != null;



    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _uiSource;

    [Space(5), Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;

    [Space(5), Header("Sound Effects")]
    [SerializeField] private AudioClip _gunshot;
    [SerializeField] private AudioClip _enemyHit;
    [SerializeField] private AudioClip _enemyDeath;
    [SerializeField] private AudioClip _playerHurt;
    [SerializeField] private AudioClip _pickup;

    [Space(5), Header("UI Sounds")]
    [SerializeField] private AudioClip _uiClick;

    [Space(5), Header("Volumes")]
    [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)] private float _sfxVolume = 1.0f;
    
    [Space(5), Header("Clip Balance")]
    [SerializeField, Range(0.0f, 4.0f)] private float _gunshotVolume = 1.0f;
    [SerializeField, Range(0.0f, 4.0f)] private float _enemyHitVolume = 1.0f;
    [SerializeField, Range(0.0f, 4.0f)] private float _enemyDeathVolume = 1.0f;
    [SerializeField, Range(0.0f, 4.0f)] private float _playerHurtVolume = 1.0f;
    [SerializeField, Range(0.0f, 4.0f)] private float _pickupVolume = 1.0f;

    
    [Space(5), Header("Clip Delays")]
    [SerializeField, Range(0.0f, 0.3f)] private float _enemyHitDelay = 0.05f;
    [SerializeField, Range(0.0f, 0.3f)] private float _enemyDeathDelay = 0.05f;
    
    private const string MusicVolumeKey = "BLAIM_MusicVolume";
    private const string SfxVolumeKey = "BLAIM_SfxVolume";


    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            if (_musicSource) { _musicSource.volume = _musicVolume; }

            PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
            PlayerPrefs.Save();
        }
    }


    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            if (_sfxSource) { _sfxSource.volume = _sfxVolume; }
            if (_uiSource) { _uiSource.volume = _sfxVolume; }

            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
            PlayerPrefs.Save();
        }
    }



    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();

        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, _musicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _sfxVolume);
    }

    private void OnDestroy()
    {
        if (_instance == this) { _instance = null; }
    }
    
    private void EnsureSources()
    {
        if (!_musicSource)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
        }

        if (!_sfxSource)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
        }

        if (!_uiSource)
        {
            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;

            
            _uiSource.ignoreListenerPause = true;
        }
    }

   

    public void PlayMenuMusic() => PlayMusic(_menuMusic);

    public void PlayGameMusic() => PlayMusic(_gameMusic);

    public void PlayMusic(AudioClip clip)
    {
        if (!clip || !_musicSource) { return; }

       
        if (_musicSource.clip == clip && _musicSource.isPlaying) { return; }

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource) { _musicSource.Stop(); }
    }

    
    
    public void PlaySfx(AudioClip clip, float pitchVariance = 0.0f, float volumeScale = 1.0f, float delay = 0.0f)
    {
        if (!clip || !_sfxSource) { return; }

        if (delay > 0.0f)
        {
            StartCoroutine(PlayDelayed(clip, pitchVariance, volumeScale, delay));
            return;
        }

        _sfxSource.pitch = pitchVariance > 0.0f
            ? 1.0f + Random.Range(-pitchVariance, pitchVariance)
            : 1.0f;

        _sfxSource.PlayOneShot(clip, _sfxVolume * volumeScale);
    }

    
    private IEnumerator PlayDelayed(AudioClip clip, float pitchVariance, float volumeScale, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        PlaySfx(clip, pitchVariance, volumeScale);
    }

    public void PlayGunshot() => PlaySfx(_gunshot, 0.06f, _gunshotVolume);

    public void PlayEnemyHit() => PlaySfx(_enemyHit, 0.1f, _enemyHitVolume, _enemyHitDelay);

    public void PlayEnemyDeath() => PlaySfx(_enemyDeath, 0.08f, _enemyDeathVolume, _enemyDeathDelay);

    public void PlayPlayerHurt() => PlaySfx(_playerHurt, 0.0f, _playerHurtVolume);

    public void PlayPickup() => PlaySfx(_pickup, 0.0f, _pickupVolume);

  

    public void PlayUiClick() => PlayUiSound(_uiClick);

    
    public void PlayUiSound(AudioClip clip)
    {
        if (!clip || !_uiSource) { return; }

        _uiSource.PlayOneShot(clip, _sfxVolume);
    }
}
