using UnityEngine;
using UnityEngine.UI;

public class GameAudioController : MonoBehaviour
{
    private const string c_musicKey = "ColourGuy.MusicOn";
    private const string c_effectsKey = "ColourGuy.SoundEffectsOn";
    private const string c_vibrationKey = "ColourGuy.VibrationOn";

    [SerializeField] private AudioClip m_musicTrack;
    [SerializeField] private AudioClip m_buttonPressSound;
    [SerializeField] private AudioClip m_jumpSound;

    private AudioSource m_musicSource;
    private AudioSource m_effectsSource;
    private bool m_musicOn;
    private bool m_effectsOn;
    private bool m_vibrationOn;

    public static GameAudioController Instance { get; private set; }
    public bool MusicOn => m_musicOn;
    public bool EffectsOn => m_effectsOn;
    public bool VibrationOn => m_vibrationOn;

    private void Awake()
    {
        Instance = this;
        // Fresh installs start with sound on and saved choices take over after that
        m_musicOn = PlayerPrefs.GetInt(c_musicKey, 1) == 1;
        m_effectsOn = PlayerPrefs.GetInt(c_effectsKey, 1) == 1;
        m_vibrationOn = PlayerPrefs.GetInt(c_vibrationKey, 1) == 1;

        // Menu stays loaded between runs so the song keeps its place
        m_musicSource = gameObject.AddComponent<AudioSource>();
        m_musicSource.clip = m_musicTrack;
        m_musicSource.loop = true;
        m_musicSource.playOnAwake = false;
        m_musicSource.spatialBlend = 0f;
        m_musicSource.mute = !m_musicOn;
        m_musicSource.Play();

        // Effects get their own source so they can play over the song
        m_effectsSource = gameObject.AddComponent<AudioSource>();
        m_effectsSource.playOnAwake = false;
        m_effectsSource.spatialBlend = 0f;
    }

    // These choices save as soon as they change
    public void ToggleMusic()
    {
        m_musicOn = !m_musicOn;
        m_musicSource.mute = !m_musicOn;
        PlayerPrefs.SetInt(c_musicKey, m_musicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleEffects()
    {
        m_effectsOn = !m_effectsOn;
        PlayerPrefs.SetInt(c_effectsKey, m_effectsOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVibration()
    {
        m_vibrationOn = !m_vibrationOn;
        PlayerPrefs.SetInt(c_vibrationKey, m_vibrationOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RegisterButtonSound(Button button)
    {
        // Leave the button's action in place and add a tap sound
        button.onClick.AddListener(PlayButtonPress);
    }

    public void PlayJump()
    {
        if (m_effectsOn)
        {
            m_effectsSource.PlayOneShot(m_jumpSound);
        }
    }

    public void VibrateOnDeath()
    {
        if (!m_vibrationOn)
        {
            return;
        }

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    private void PlayButtonPress()
    {
        if (m_effectsOn)
        {
            m_effectsSource.PlayOneShot(m_buttonPressSound);
        }
    }
}
