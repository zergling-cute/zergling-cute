using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BgmSoundManager : MonoBehaviour
{
    [Header("배경 음악 설정")]
    [Range(0f, 1f)]
    public float volume = 1f;
    // public float fadeDuration = 1f; // 페이드 기능 삭제

    private static BgmSoundManager _instance;
    private AudioSource _audioSource;

    // --- 페이드 관련 변수 모두 삭제 ---
    // private float _targetVolume;
    // private float _currentFadeTime;
    // private bool _isFading;
    // private System.Action _fadeOutCallback;
    // private float _startVolume; // (이전 제안 변수)

    public Dictionary<string, AudioClip> bgmTracks = new Dictionary<string, AudioClip>();

    private Dictionary<int, string> storyToBgmKey = new Dictionary<int, string>()
    {
        { 0, "Jungle" },
        { 1, "Fantasy" },
        { 2, "SF" },
        { 3, "Title" },
    };

    public static BgmSoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObject = new GameObject("BgmSoundManager");
                _instance = managerObject.AddComponent<BgmSoundManager>();
                DontDestroyOnLoad(managerObject);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.volume = volume; // Awake에서 마스터 볼륨으로 설정

            LoadBgmDataFromCSV();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV 파일에서 배경음악 데이터를 읽어와 딕셔너리에 등록합니다.
    /// </summary>
    private void LoadBgmDataFromCSV()
    {
        // CSVReader가 "Resources/DataTable/BgmTable.csv" 파일을 읽습니다.
        List<Dictionary<string, object>> bgmData = CSVReader.Read("BgmTable");

        foreach (var row in bgmData)
        {
            // CSV 헤더("ID", "FileName")와 동일한 키를 사용합니다.
            string id = row["ID"].ToString();
            string fileName = row["FileName"].ToString();

            // "Resources/Sounds/BGM/" 경로에서 오디오 클립을 로드합니다.
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/BGM/{fileName}");

            if (clip != null)
            {
                if (!bgmTracks.ContainsKey(id))
                {
                    bgmTracks.Add(id, clip);
                }
                else
                {
                    Debug.LogWarning($"[BgmSoundManager] 이미 등록된 BGM 키입니다: {id}");
                }
            }
            else
            {
                Debug.LogWarning($"[BgmSoundManager] 오디오 클립을 찾을 수 없습니다: Sounds/BGM/{fileName}");
            }
        }
    }

    // --- 아래의 BGM 재생 관련 코드는 기존과 동일합니다. ---

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    // 씬 로드시 자동 재생
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Loading")
        {
            PlayBgm("Loading");
            Debug.Log($"[BgmSoundManager] 씬 'Loading' 로드됨 - Loading 음악 재생");
            return;
        }
        else if (scene.name == "MainTitle")
        {
            PlayBgm("MainTitleTheme");
            Debug.Log($"[BgmSoundManager] 씬 'MainTitle' 로드됨 - MainTitleTheme 음악 재생");
            return;
        }
        else if (scene.name == "Title")
        {
            PlayBgm("TitleTheme");
            Debug.Log($"[BgmSoundManager] 씬 'Title' 로드됨 - TitleTheme 음악 재생");
            return;
        }
        else if (scene.name == "PlayScene")
        {
            if (GameManager.instance != null)
            {
                int story = GameManager.instance.Story;
                string playBgmKey = story switch
                {
                    0 => "Jungle_Play",
                    1 => "Fantasy_Play",
                    2 => "SF_Play",
                    _ => "TitleTheme" // 기본값
                };

                PlayBgm(playBgmKey);
                Debug.Log($"[BgmSoundManager] 씬 'PlayScene' 로드됨 - Story: {story} → {playBgmKey} 음악 재생");
            }
            else
            {
                Debug.LogWarning("[BgmSoundManager] GameManager.instance가 null입니다.");
            }
            return;
        }

        // 그 외 씬은 기존 스토리 값 기반 음악 재생
        if (GameManager.instance != null)
        {
            int story = GameManager.instance.Story;
            PlayStoryBgm(story);
            Debug.Log($"[BgmSoundManager] 씬 '{scene.name}' 로드됨. Story: {story} → 음악 변경");
        }
    }

    public void PlayStoryBgm(int storyValue)
    {
        if (storyToBgmKey.TryGetValue(storyValue, out string trackName))
        {
            PlayBgm(trackName);
        }
        else
        {
            Debug.LogWarning($"[BgmSoundManager] 스토리 {storyValue}에 해당하는 음악이 없습니다.");
        }
    }

    public void RegisterBgm(string trackName, AudioClip clip)
    {
        if (!bgmTracks.ContainsKey(trackName) && clip != null)
        {
            bgmTracks.Add(trackName, clip);
        }
        else if (bgmTracks.ContainsKey(trackName))
        {
            Debug.LogWarning($"[BgmSoundManager] 이미 등록된 키: {trackName}");
        }
        else if (clip == null)
        {
            Debug.LogWarning($"[BgmSoundManager] 클립이 null입니다: {trackName}");
        }
    }

    public AudioClip GetBgm(string trackName)
    {
        if (bgmTracks.TryGetValue(trackName, out AudioClip clip))
        {
            return clip;
        }
        Debug.LogWarning($"[BgmSoundManager] '{trackName}' 트랙이 없습니다.");
        return null;
    }


    // [수정] SetVolume: _targetVolume 제거
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        _audioSource.volume = volume; // AudioSource의 볼륨에 바로 적용
    }

    public float GetVolume() => volume;

    // [수정] PlayBgm: 페이드 로직 제거, 즉시 재생
    public void PlayBgm(string trackName)
    {
        AudioClip clip = GetBgm(trackName);
        if (clip != null)
        {
            // 이미 같은 음악이 재생 중이면 무시
            if (_audioSource.clip == clip && _audioSource.isPlaying)
                return;

            // 페이드 로직 전부 삭제
            _audioSource.clip = clip;
            _audioSource.volume = volume; // 마스터 볼륨으로 즉시 설정
            _audioSource.Play(); // 즉시 재생
        }
    }

    // --- FadeOut, FadeIn 메서드 삭제 ---
    // public void FadeOut(...) { ... }
    // public void FadeIn() { ... }

    public void StopBgm()
    {
        _audioSource.Stop();
    }

    public void PlayThema(string thema)
    {
        PlayBgm(thema);
    }

    // --- Update 메서드 삭제 ---
    // private void Update() { ... }
}
