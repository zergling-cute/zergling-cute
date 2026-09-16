using UnityEngine;
using UnityEngine.UI;

public class SoundOptionsController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    // Update is called once per frame
    void Update()
    {
        
    }


    [Header("UI 토글")]
    public Toggle bgmToggle;
    public Toggle sfxToggle;
    public Toggle joyStickToggle;
    public Image bgm;
    public Image sfx;
    public Image joystick;

    public Button optionCloseButton;

    private void Start()
    {
        // 저장된 설정 불러오기 (기본값: ON)
        bool bgmOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
        

        // 토글 상태 설정
        bgmToggle.isOn = bgmOn;
        sfxToggle.isOn = sfxOn;
        
        // 실제 볼륨 설정
        BgmSoundManager.Instance.SetVolume(bgmOn ? 0.5f : 0f);
        EffectsSoundManager.Instance.SetVolume(sfxOn ? 1f : 0f);

        //회전값 초기화
        bgm.transform.rotation = Quaternion.Euler(0f, bgmOn ? 180f : 0f, 0f);
        sfx.transform.rotation = Quaternion.Euler(0f, sfxOn ? 180f : 0f, 0f);

        // 이벤트 리스너 연결
        bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        //joyStickToggle.onValueChanged.AddListener(OnJoyStickToggleChanged);

        optionCloseButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnBgmToggleChanged(bool isOn)
    {
        float yRotation = isOn ? 180f : 0f;
        bgm.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        BgmSoundManager.Instance.SetVolume(isOn ? 0.3f : 0f);
        PlayerPrefs.SetInt("BGM_ON", isOn ? 1 : 0);
    }

    private void OnSfxToggleChanged(bool isOn)
    {
        float yRotation = isOn ? 180f : 0f;
        sfx.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        EffectsSoundManager.Instance.SetVolume(isOn ? 1f : 0f);
        PlayerPrefs.SetInt("SFX_ON", isOn ? 1 : 0);

    }
    private void OnJoyStickToggleChanged(bool isOn)
    {
        float yRotation = isOn ? 180f : 0f;
        joystick.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
