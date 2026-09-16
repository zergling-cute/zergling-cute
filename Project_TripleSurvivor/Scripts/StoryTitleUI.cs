using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 사용을 위해 추가

public class StoryTitleUI : MonoBehaviour
{
    // 기존 팝업창들
    public GameObject startPopup;
    public GameObject talentPopup;
    public GameObject optionsPopup;

    // 컷신 팝업창 (Inspector에서 할당해야 함)
    public GameObject cutscenePopup;
    // 컷신 이미지를 표시할 Image 컴포넌트 (Inspector에서 할당해야 함)
    public Image cutsceneImageComponent;

    // 각 스토리에 맞는 컷신 이미지들 (Inspector에서 할당해야 함)
    public Sprite cutsceneImageStory0;
    public Sprite[] cutsceneImagesStory1; // 2개의 이미지를 할당할 배열
    public Sprite cutsceneImageStory2;

    // 기존 버튼들
    public Button startButton;
    public Button talentButton;
    public Button toTheTitleButton;
    public Button optionsButton;
    public Button optionsCloseButton;

    void Start()
    {
        // Find 대신 Inspector에서 직접 할당하는 것을 권장하지만, 기존 코드를 유지합니다.
        talentPopup = GameObject.Find("TalentPopup");
        startPopup = GameObject.Find("StartPopup");
        optionsPopup = GameObject.Find("OptionPopup");

        // 버튼 리스너 설정
        startButton.onClick.AddListener(OnClickStartButton); // 기존 OpenStartPopup을 새로운 메소드로 변경
        talentButton.onClick.AddListener(OpenTalentPopup);
        optionsButton.onClick.AddListener(OpenOptionsPopup);
        toTheTitleButton.onClick.AddListener(GoToTitle);

        // 시작 시 모든 팝업 비활성화
        startPopup.SetActive(false);
        talentPopup.SetActive(false);
        optionsPopup.SetActive(false);
        cutscenePopup.SetActive(false); // 컷신 팝업도 비활성화
    }

    // 시작 버튼을 눌렀을 때 컷신 코루틴을 실행
    void OnClickStartButton()
    {
        StartCoroutine(ShowCutsceneAndOpenStartPopup());
    }

    // 컷신을 보여주고 시작 팝업을 여는 코루틴
    IEnumerator ShowCutsceneAndOpenStartPopup()
    {
        CloseAllPopups();
        cutscenePopup.SetActive(true);

        int storyValue = GameManager.instance.Story;

        switch (storyValue)
        {
            case 0:
                cutsceneImageComponent.sprite = cutsceneImageStory0;
                yield return StartCoroutine(WaitForClickOrTime(3.0f));
                break;

            case 1:
                // 첫 번째 이미지
                if (cutsceneImagesStory1.Length >= 1)
                {
                    cutsceneImageComponent.sprite = cutsceneImagesStory1[0];
                    // 2.5초 기다리거나 클릭 대기
                    yield return StartCoroutine(WaitForClickOrTime(3f));
                }

                // 두 번째 이미지 (클릭되지 않았고 시간이 다 지났을 경우)
                if (cutsceneImagesStory1.Length >= 2)
                {
                    cutsceneImageComponent.sprite = cutsceneImagesStory1[1];
                    // 추가 2.5초 기다리거나 클릭 대기
                    yield return StartCoroutine(WaitForClickOrTime(3f));
                }
                break;

            case 2:
                cutsceneImageComponent.sprite = cutsceneImageStory2;
                yield return StartCoroutine(WaitForClickOrTime(3.0f));
                break;

            default:
                Debug.LogWarning("정의되지 않은 Story 값입니다: " + storyValue);
                break;
        }

        // 컷신이 끝나면 컷신 팝업을 닫고 시작 팝업을 연다.
        cutscenePopup.SetActive(false);
        OpenStartPopup();
    }

    // 지정된 시간 또는 좌클릭이 발생할 때까지 기다리는 코루틴
    IEnumerator WaitForClickOrTime(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            if (Input.GetMouseButtonDown(0)) // 좌클릭 감지
            {
                yield break; // 클릭되면 코루틴 즉시 종료
            }
            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }
    }

    void OpenStartPopup()
    {
        CloseAllPopups();
        startPopup.SetActive(true);
    }

    void OpenTalentPopup()
    {
        CloseAllPopups();
        talentPopup.SetActive(true);
    }

    void OpenOptionsPopup()
    {
        CloseAllPopups();
        optionsPopup.SetActive(true);
    }

    void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    void CloseAllPopups()
    {
        startPopup.SetActive(false);
        talentPopup.SetActive(false);
        optionsPopup.SetActive(false);
        cutscenePopup.SetActive(false); // 모든 팝업 닫을 때 컷신 팝업도 포함
    }
}
