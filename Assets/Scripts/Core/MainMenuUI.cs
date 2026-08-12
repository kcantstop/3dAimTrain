using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Game/Managers/Main Menu UI")]
public class MainMenuUI : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text _highScoreText;

    [Space(5), Header("Volume")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1.0f;

        if (_highScoreText && GameManager.Exists)
        {
            _highScoreText.text = $"BEST {GameManager.Instance.HighScore:N0}";
        }

        if (AudioManager.Exists)
        {
            AudioManager.Instance.PlayMenuMusic();

            if (_musicSlider) { _musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume); }
            if (_sfxSlider) { _sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume); }
        }
    }

    public void OnPlayButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.LoadArena(); }
    }

    public void OnQuitButton()
    {
        PlayClick();
        if (GameManager.Exists) { GameManager.Instance.QuitGame(); }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Exists) { AudioManager.Instance.MusicVolume = value; }
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Exists) { AudioManager.Instance.SfxVolume = value; }
    }

    private static void PlayClick()
    {
        if (AudioManager.Exists) { AudioManager.Instance.PlayUiClick(); }
    }
}
