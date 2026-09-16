using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public Button goToMainButton;
    public Button continueButton;

    public SkillUIManager skillUIManager;

    void Start()
    {
        // 버튼에 기능 연결
        goToMainButton.onClick.AddListener(GoToMainScene);
        continueButton.onClick.AddListener(ClosePausePopup);

        // UI는 시작할 때 항상 비활성화 상태로 둡니다.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 일시정지 팝업을 열고 게임을 멈춥니다.
    /// </summary>
    public void OpenPausePopup()
    {
        // ... (기존 OpenPausePopup() 로직 유지)

        // UI를 활성화하여 화면에 표시
        gameObject.SetActive(true);

        // (선택 사항) 스킬 UI 갱신 로직
        var foundSkillUIManager = Object.FindFirstObjectByType<SkillUIManager>();
        if (foundSkillUIManager != null)
        {
            foundSkillUIManager.RefreshSkillUI();
        }

        // 게임의 시간을 멈춥니다.
        Time.timeScale = 0f;

        // 씬에 있는 모든 루프 사운드를 찾아서 '일시정지' 시킵니다.
        var loopingSfxList = Object.FindObjectsByType<LoopingAudioSource>(FindObjectsSortMode.None);
        foreach (var sfx in loopingSfxList)
        {
            AudioSource source = sfx.GetComponent<AudioSource>();
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    /// <summary>
    /// 일시정지 팝업을 닫고 게임을 다시 시작합니다.
    /// </summary>
    void ClosePausePopup()
    {
        // UI를 다시 비활성화하여 숨깁니다.
        gameObject.SetActive(false);

        //  [추가된 부분] 팝업이 닫힐 때 툴팁을 강제로 숨깁니다.
        TooltipTrigger.HideTooltip();

        // 게임의 시간을 원래대로 되돌립니다.
        Time.timeScale = 1f;

        // 씬에 있는 모든 루프 사운드를 찾아서 '다시 재생' 시킵니다.
        var loopingSfxList = Object.FindObjectsByType<LoopingAudioSource>(FindObjectsSortMode.None);
        foreach (var sfx in loopingSfxList)
        {
            AudioSource source = sfx.GetComponent<AudioSource>();
            if (source != null)
            {
                // AudioSource의 일시정지 해제는 UnPause() 입니다.
                source.UnPause();
            }
        }
    }

    /// <summary>
    /// 메인 타이틀 씬으로 돌아갑니다.
    /// </summary>
    void GoToMainScene()
    {
        // 씬을 이동하기 전에는 반드시 Time.timeScale을 1로 되돌려야 합니다.
        // 그렇지 않으면 다음 씬이 멈춘 상태로 로드될 수 있습니다.
        Time.timeScale = 1f;
        SceneManager.LoadScene("StoryTitle"); // "StoryTitle"은 실제 씬 이름으로 변경해야 합니다.
    }
}
