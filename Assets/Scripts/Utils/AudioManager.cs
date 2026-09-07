using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Roteamento (Audio Mixer)")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Hit único (rusher / atirador / tanque / boss)")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private Vector2 hitPitchRange = new Vector2(0.85f, 1.15f);
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.9f;
    [Tooltip("Cada instância concorrente de hit precisa da própria AudioSource: AudioSource.pitch afeta TODAS as reproduções em andamento naquela fonte (inclusive via PlayOneShot), então um pool pequeno evita que um hit novo 'puxe' o pitch de um hit anterior que ainda está tocando. 8 cobre bem rajadas de morte simultânea (ex.: lâminas orbitais / tiro em leque acertando vários inimigos no mesmo frame).")]
    [SerializeField] private int hitVoicePoolSize = 8;

    [Header("Música (faixa única ambiente/tensão)")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    private AudioSource[] _hitPool;
    private int _hitCursor;
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 

        BuildHitPool();
        BuildMusicSource();
    }

    private void Start()
    {
        if (musicClip != null)
            _musicSource.Play();
    }

    private void BuildHitPool()
    {
        int size = Mathf.Max(1, hitVoicePoolSize);
        _hitPool = new AudioSource[size];
        for (int i = 0; i < size; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.clip = hitClip;
            src.outputAudioMixerGroup = sfxGroup;
            src.volume = hitVolume;
            _hitPool[i] = src;
        }
    }

    private void BuildMusicSource()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.clip = musicClip;
        _musicSource.outputAudioMixerGroup = musicGroup;
        _musicSource.volume = musicVolume;
    }

    public void PlayHit()
    {
        if (hitClip == null || _hitPool == null || _hitPool.Length == 0) return;

        AudioSource src = _hitPool[_hitCursor];
        _hitCursor = (_hitCursor + 1) % _hitPool.Length;

        src.pitch = Random.Range(hitPitchRange.x, hitPitchRange.y);
        src.Play();
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        if (_musicSource == null || newMusic == null) return;
        if (_musicSource.clip == newMusic) return; 

        _musicSource.Stop();
        _musicSource.clip = newMusic;
        _musicSource.Play();
    }
}