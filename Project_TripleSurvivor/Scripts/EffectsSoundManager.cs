using UnityEngine;
using System.Collections.Generic;

public class EffectsSoundManager : MonoBehaviour
{
    [Header("사운드 설정")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("보관된 사운드")]
    public Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

    private static EffectsSoundManager _instance;
    public static EffectsSoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObject = new GameObject("EffectsSoundManager");
                _instance = managerObject.AddComponent<EffectsSoundManager>();
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
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // CSV 파일로부터 효과음 데이터를 로드하는 함수 호출
        LoadSfxDataFromCSV();
    }

    private void LoadSfxDataFromCSV()
    {
        // 이 부분은 CSVReader 스크립트가 있다고 가정합니다.
        List<Dictionary<string, object>> sfxData = CSVReader.Read("SfxTable");

        foreach (var row in sfxData)
        {
            string id = row["ID"].ToString();
            string fileName = row["FileName"].ToString();
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/SFX/{fileName}");

            if (clip != null)
            {
                if (!soundEffects.ContainsKey(id))
                {
                    soundEffects.Add(id, clip);
                }
            }
            else
            {
                Debug.LogWarning($"[EffectsSoundManager] 오디오 클립을 찾을 수 없습니다: Sounds/SFX/{fileName}");
            }
        }
    }

    // --- 기존 기능 (전혀 수정되지 않음) ---

    /// <summary> 사운드 등록 </summary>
    public void RegisterSound(string soundName, AudioClip clip)
    {
        if (!soundEffects.ContainsKey(soundName) && clip != null)
            soundEffects.Add(soundName, clip);
    }

    /// <summary> 사운드 가져오기 </summary>
    public AudioClip GetSound(string soundName)
    {
        if (soundEffects.ContainsKey(soundName))
            return soundEffects[soundName];
        Debug.LogWarning($"[EffectsSoundManager] 등록되지 않은 사운드: {soundName}");
        return null;
    }

    /// <summary> 위치 기반 사운드 재생 (씬 전환 시 끊길 수 있음) </summary>
    public void PlaySound(string soundName, Vector3 playPosition, float sourceVolumeScale = 1f)
    {
        AudioClip clip = GetSound(soundName);
        if (clip == null) return;

        GameObject soundObject = new GameObject($"SoundEffect_{soundName}");
        soundObject.transform.position = playPosition;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * sourceVolumeScale;
        source.spatialBlend = 0f; // UI/2D 사운드용
        source.Play();

        Destroy(soundObject, clip.length + 0.1f);
    }

    /// <summary> 씬 전환 시에도 끝까지 재생되는 사운드 </summary>
    public void PlayPersistentSound(string soundName, Vector3 playPosition, float sourceVolumeScale = 1f)
    {
        AudioClip clip = GetSound(soundName);
        if (clip == null) return;

        GameObject soundObject = new GameObject($"PersistentSound_{soundName}");
        soundObject.transform.position = playPosition;
        DontDestroyOnLoad(soundObject);

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * sourceVolumeScale;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(soundObject, clip.length + 0.1f);
    }

    /// <summary> AudioSource를 통해 한 번만 재생 (기존 소스 사용) </summary>
    public void PlaySoundOneShot(AudioSource source, string soundName, float sourceVolumeScale = 1f)
    {
        AudioClip clip = GetSound(soundName);
        if (source != null && clip != null)
            source.PlayOneShot(clip, volume * sourceVolumeScale);
    }

    /// <summary> 전체 효과음 볼륨 설정 </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
    }

    /// <summary> 현재 볼륨 가져오기 </summary>
    public float GetVolume()
    {
        return volume;
    }

    // --- ▼▼▼ 여기에 새로운 기능만 '추가' ▼▼▼ ---

    /// <summary>
    /// [추가된 기능] 루프 사운드를 시작하고, 제어할 수 있는 AudioSource를 반환합니다.
    /// </summary>
    /// <param name="soundName">재생할 루프 사운드의 ID</param>
    /// <param name="playPosition">최초 재생 위치</param>
    /// <returns>제어 가능한 AudioSource 컴포넌트</returns>
    public AudioSource PlayLoopSound(string soundName, Vector3 playPosition)
    {
        AudioClip clip = GetSound(soundName);
        if (clip == null)
        {
            Debug.LogError($"[EffectsSoundManager] 루프 사운드를 찾을 수 없습니다: {soundName}");
            return null;
        }

        GameObject soundObject = new GameObject($"LoopSound_{soundName}");
        soundObject.transform.position = playPosition;

        // ▼▼▼ 여기에 한 줄 추가 ▼▼▼
        soundObject.AddComponent<LoopingAudioSource>(); // 루프 사운드임을 표시하는 태그 추가
                                                        // ▲▲▲ 추가 끝 ▲▲▲

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.spatialBlend = 0f;
        source.Play();

        return source;
    }

    /// <summary>
    /// [추가된 기능] PlayLoopSound로 시작한 루프 사운드를 중지하고 오브젝트를 파괴합니다.
    /// </summary>
    /// <param name="loopSource">중지할 AudioSource</param>
    public void StopLoopSound(AudioSource loopSource)
    {
        // AudioSource나 그 게임오브젝트가 이미 파괴된 경우를 대비한 안전 코드
        if (loopSource != null && loopSource.gameObject != null)
        {
            loopSource.Stop();
            Destroy(loopSource.gameObject);
        }
    }
}
